using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StorageChallenge.Models;
using StorageChallenge.Testing;

namespace StorageChallenge.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Files()
        {
            ViewBag.Result = new BlobTestResult();
            return View(new FileTestData());
        }
        [HttpPost]
        public ActionResult Files(FileTestData data)
        {
            var test = new Test();
            BlobTestResult result = null;
            if(TestType.TestPublicStorage && TestType.TestPrivateStorage)
            {
                var privateResult = test.TestPrivateBlob(data);
                var publicResult = test.TestPublicBlob(data);
                result = new BlobTestResult(privateResult,publicResult);
            }
            else if(TestType.TestPublicStorage)
            {
                result = test.TestPublicBlob(data);
            }
            else
            {
                result = test.TestPrivateBlob(data);
            }
            ViewBag.Result = result;
            return View(data);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }


}