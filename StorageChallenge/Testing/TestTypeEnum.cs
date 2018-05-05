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
    public class TestType
    {
        private static int testValue;
        static TestType()
        {
            testValue = int.Parse(CloudConfigurationManager.GetSetting("TestType"));
        }
        public static bool TestPublicStorage { get { return (testValue & (int)TestTypeEnum.PublicStorage) > 0; } }
        public static bool TestPrivateStorage { get { return (testValue & (int)TestTypeEnum.PrivateStorage) > 0; } }
        public static bool TestSQLServer { get { return (testValue & (int)TestTypeEnum.SQLServer) > 0; } }
        public static bool TestMySQL { get { return (testValue & (int)TestTypeEnum.MySQL) > 0; } }
        public static bool TestCosmosDB { get { return (testValue & (int)TestTypeEnum.CosmosDB) > 0; } }
        public static bool TestSearch { get { return (testValue & (int)TestTypeEnum.Search) > 0; } }
        public static bool TestStorage { get { return (testValue & (int)(TestTypeEnum.PublicStorage | TestTypeEnum.PrivateStorage)) > 0; } }
        public static bool TestRelational { get { return (testValue & (int)(TestTypeEnum.SQLServer | TestTypeEnum.MySQL)) > 0; } }
        public static bool TestNoSQL { get { return (testValue & (int)(TestTypeEnum.CosmosDB | TestTypeEnum.Search)) > 0; } }

    }
    public enum TestTypeEnum : int
    {
        PublicStorage = 1,
        PrivateStorage = 2,
        SQLServer = 4,
        MySQL = 8,
        CosmosDB = 16,
        Search = 32
    }
    public class Test
    {
        public BlobTestResult TestPublicBlob(FileTestData data)
        {
            var result = new BlobTestResult { Passed = false , Ignore = false};
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
            return result;
           
        }
        public BlobTestResult TestPrivateBlob(FileTestData data)
        {
            //Normalize the SAS - CLI and PowerShell differ.
            data.storageAccountSAS = data.storageAccountSAS.Replace("?", "");
            var result = new BlobTestResult { Passed = false, Ignore = false };
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
                            foreach(var SASProp in data.storageAccountSAS.Split("&".ToCharArray()))
                            {
                                //Reference SAS from CLI - sp=r&sv=2017-07-29&sr=c&sig=jfqVykBTpWib53o65r4UO1irdJWosR3bAQac1tocC2Q%3D
                                var pair = SASProp.Split("=".ToCharArray());
                                if(pair.Count()==2)
                                {
                                    if (pair[0] == "sp") { SASpermission = pair[1] == "r"; }
                                    if (pair[0] == "sr") { SASobject = pair[1] == "c"; }
                                }
                            }
                            if(SASpermission && SASobject)
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
                                } catch(Exception ex)
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
            return result;

        }
    }
    public class BlobTestResult {
        public List<string> PrivateBlobs { get; set; }
        public List<string> PublicBlobs { get; set; }
        public string Status { get; set; }
        public bool Passed { get; set; }
        public string SAS { get; set; }
        public bool Ignore { get; set; }
        public BlobTestResult()
        {
            PrivateBlobs = new List<string>();
            PublicBlobs = new List<string>();
            Ignore = true;
        }
        public BlobTestResult(BlobTestResult privateResult, BlobTestResult publicResult)
        {
            this.Ignore = false;
            this.PrivateBlobs = new List<string>(privateResult.PrivateBlobs);
            this.PublicBlobs = new List<string>(publicResult.PublicBlobs);
            this.SAS = publicResult.SAS;
            if (publicResult.Passed && privateResult.Passed)
            {
                this.Status = "You have correctly configured the public and private blob containers.  You can test the links below to confirm:";
                this.Passed = true;
            }
            else if (publicResult.Passed)
            {
                this.Status = "You have properly configured the public blob container but not the private blob container." + privateResult.Status;
                this.Passed = false;
            }
            else if (privateResult.Passed)
            {
                this.Status = "You have properly configured the private blob container but not the public blob container." + publicResult.Status;
                this.Passed = false;
            }
            else
            {
                this.Status = "There are errors with the configuration of both the public blob container and the private blob container.  The issue may be with either the storage account or the storage account key.";
                this.Passed = false;
            }
        }

    }
}