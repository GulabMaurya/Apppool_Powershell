using AppPool.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppPool.Controllers
{  
        public class AppPoolController : Controller
        {
            private readonly IisService _iisService;

            public AppPoolController()
            {
                _iisService = new IisService("Servername", "Username", "Password");
            }

            public ActionResult Index()
            {
                var appPools = _iisService.GetAppPools();
                return View(appPools);
            }

            public ActionResult Start(string name)
            {
                _iisService.StartAppPool(name);
                return RedirectToAction("Index");
            }

            public ActionResult Stop(string name)
            {
                _iisService.StopAppPool(name);
                return RedirectToAction("Index");
            }

            public ActionResult Recycle(string name)
            {
                _iisService.RecycleAppPool(name);
                return RedirectToAction("Index");
            }
        }

    }
