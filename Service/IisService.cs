using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AppPool.Models;
using System.IO;

namespace AppPool.Service
{
    public class IisService
    {
        private string remoteServer;
        private string username;
        private string password;

        public IisService(string remoteServer, string username, string password)
        {
            this.remoteServer = remoteServer;
            this.username = username;
            this.password = password;
        }

        private PowerShell CreatePowerShellSession()
        {
            PowerShell ps = PowerShell.Create();
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            ps.Runspace = runspace;

            var securePassword = new NetworkCredential("", password).SecurePassword;
            var credential = new PSCredential(username, securePassword);
            var command = new Command("New-PSSession");
            command.Parameters.Add("ComputerName", remoteServer);
            command.Parameters.Add("Credential", credential);
            command.Parameters.Add("UseSSL", true); // Use HTTPS

            ps.Commands.AddCommand(command);
            ps.Invoke();
            WriteMessageInFile();
            if (ps.HadErrors) //check this property will be true//Deeps
            {
                var errors = ps.Streams.Error;
                foreach (var error in errors) //check error here in variable error //Deeps
                {

                    Console.WriteLine(error.ToString());
                }
                throw new InvalidOperationException("Failed to create PowerShell session.");
            }

            return ps;
        }

        public List<AppPools> GetAppPools()
        {
            var appPools = new List<AppPools>();
            using (var ps = CreatePowerShellSession())
            {
                ps.Commands.Clear();
                ps.AddScript("Import-Module WebAdministration; Get-ChildItem IIS:\\AppPools | Select-Object Name, State");

                var results = ps.Invoke();

                foreach (var result in results)
                {
                    appPools.Add(new AppPools
                    {
                        Name = result.Members["Name"].Value.ToString(),
                        Status = result.Members["State"].Value.ToString()
                    });
                }
            }
            return appPools;
        }

        public void StartAppPool(string appPoolName)
        {
            using (var ps = CreatePowerShellSession())
            {
                ps.Commands.Clear();
                ps.AddScript($"Import-Module WebAdministration; Start-WebAppPool -Name \"{appPoolName}\"");
                ps.Invoke();
            }
        }

        public void StopAppPool(string appPoolName)
        {
            using (var ps = CreatePowerShellSession())
            {
                ps.Commands.Clear();
                ps.AddScript($"Import-Module WebAdministration; Stop-WebAppPool -Name \"{appPoolName}\"");
                ps.Invoke();
            }
        }

        public void RecycleAppPool(string appPoolName)
        {
            using (var ps = CreatePowerShellSession())
            {
                ps.Commands.Clear();
                ps.AddScript($"Import-Module WebAdministration; Restart-WebAppPool -Name \"{appPoolName}\"");
                ps.Invoke();
            }
        }
        public void WriteMessageInFile() {
            string message = "Hello, this is a message written to a text file.";
            string filePath = @"C:\Development\Test\file.txt"; // Replace with your desired file path

            try
            {
                // Write the message to the text file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(message);
                }

                Console.WriteLine("Message has been written to the file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

}