using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
            ViewBag.Status = "Click Test to test";
            return View(new FileTestData());
        }
        [HttpPost]
        public ActionResult Files(FileTestData data)
        {
            var uris = new List<string>();
            try
            {
                var account = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(data.storageAccountName, data.storageAccountKey), true);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("public");
                if(container.Exists())
                {
                    if(container.GetPermissions().PublicAccess == BlobContainerPublicAccessType.Blob)
                    {
                        foreach(var blob in container.ListBlobs())
                        {
                            uris.Add(blob.Uri.AbsoluteUri);
                        }
                        if (uris.Count > 0)
                        {
                            ViewBag.Uris = uris;
                            ViewBag.Status = "You have completed this challenge successfully.  You can test the links below:";
                        } else
                        {
                            ViewBag.Status = "You have configured the storage container correctly, but there are no blobs in the container.";
                        }
                    } else
                    {
                        ViewBag.Status = "Your container does not have the correct security setting.";
                    }
                } else
                {
                    ViewBag.Status = "Public container does not exist.";
                }
            }
            catch
            {
                ViewBag.Status = "Invalid storage account or key.";

            }
            ViewBag.Uris = uris;
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

    public class FileTestData
    {
        [Display(Name="Storage account name")]
        public string storageAccountName { get; set; }
        [Display(Name = "Storage account key")]
        public string storageAccountKey { get; set; }
        [Display(Name = "Shared access signature")]
        public string storageAccountSAS { get; set; }
    }
}