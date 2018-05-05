using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using StorageChallenge.Models;

namespace StorageChallenge.Testing
{
    public class Test
    {
        System.Web.Caching.Cache cache = null;
        public Test()
        {
            cache = System.Web.HttpContext.Current.Cache;
        }
        public BlobTestResult TestPublicBlob(FileTestData data)
        {
            var result = new BlobTestResult { Passed = false, Ignore = false };
            try
            {
                var account = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(data.storageAccountName, data.storageAccountKey), true);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("public");
                if (container.Exists())
                {
                    if (container.GetPermissions().PublicAccess == BlobContainerPublicAccessType.Blob)
                    {
                        foreach (var blob in container.ListBlobs())
                        {
                            result.PublicBlobs.Add(blob.Uri.AbsoluteUri);
                        }
                        if (result.PublicBlobs.Count > 0)
                        {

                            result.Status = "You have completed this challenge successfully.  You can test the links below:";
                            result.Passed = true;
                        }
                        else
                        {
                            result.Status = "You have configured the storage container correctly, but there are no blobs in the container.";
                        }
                    }
                    else
                    {
                        result.Status = "Your container does not have the correct security setting.";
                    }
                }
                else
                {
                    result.Status = "Public container does not exist.";
                }
            }
            catch
            {
                result.Status = "Invalid storage account or key.";

            }
            setTestStatus(result, TestTypeEnum.PublicStorage);
            return result;

        }



        public BlobTestResult TestPrivateBlob(FileTestData data)
        {
            //Normalize the SAS - CLI and PowerShell differ.
            data.storageAccountSAS = data.storageAccountSAS.Replace("?", "");
            var result = new BlobTestResult { Passed = false, Ignore = false, SAS = data.storageAccountSAS };
            try
            {
                var account = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(data.storageAccountName, data.storageAccountKey), true);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("private");
                if (container.Exists())
                {
                    if (container.GetPermissions().PublicAccess == BlobContainerPublicAccessType.Off)
                    {
                        foreach (var blob in container.ListBlobs())
                        {
                            result.PrivateBlobs.Add(blob.Uri.AbsoluteUri);
                        }
                        if (result.PrivateBlobs.Count > 0)
                        {
                            //Test SAS
                            bool SASpermission = false;
                            bool SASobject = false;
                            foreach (var SASProp in data.storageAccountSAS.Split("&".ToCharArray()))
                            {
                                //Reference SAS from CLI - sp=r&sv=2017-07-29&sr=c&sig=jfqVykBTpWib53o65r4UO1irdJWosR3bAQac1tocC2Q%3D
                                var pair = SASProp.Split("=".ToCharArray());
                                if (pair.Count() == 2)
                                {
                                    if (pair[0] == "sp") { SASpermission = pair[1] == "r"; }
                                    if (pair[0] == "sr") { SASobject = pair[1] == "c"; }
                                }
                            }
                            if (SASpermission && SASobject)
                            {
                                var url = result.PrivateBlobs[0] + "?" + data.storageAccountSAS;
                                var rqst = WebRequest.CreateHttp(url);
                                try
                                {
                                    using (var resp = rqst.GetResponse())
                                    {
                                        if (resp.ContentLength > 0)
                                        {
                                            result.Passed = true;
                                            result.Status = "You have successfully created and populated a private blob container, and generated a valid SAS token for the container.";
                                        }
                                        else
                                        {
                                            result.Passed = false;
                                            result.Status = "There is a problem with your SAS. It has the correct format but did not provide read access to the blobs in the private container.";

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result.Passed = false;
                                    result.Status = "There is a problem with your SAS. An error was returned when trying to retrieve a file form the private blob container using the token. " + ex.Message;
                                }
                            }

                        }
                        else
                        {
                            result.Status = "You have configured the storage container correctly, but there are no blobs in the container.";
                        }
                    }
                    else
                    {
                        result.Status = "Your container does not have the correct security setting.";
                    }
                }
                else
                {
                    result.Status = "Private container does not exist.";
                }
            }
            catch
            {
                result.Status = "Invalid storage account or key.";

            }
            setTestStatus(result, TestTypeEnum.PrivateStorage);
            return result;

        }

        private void setTestStatus(BlobTestResult result, TestTypeEnum type)
        {
            if (result.Passed)
            {
                TestStatus = TestStatus | (int)type;
            }
            else
            {
                TestStatus = TestStatus & ~(int)type;

            }
        }
        private int TestStatus
        {
            get
            {
                int status = 0;
                if(cache["TestStatus"]!=null)
                {
                    status = (int)cache["TestStatus"];
                }
                return status;
            }
            set
            {
                cache["TestStatus"] = value;
            }
        }
    }

}