using Microsoft.Azure;
using StorageChallenge.Models;
using StorageChallenge.Testing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StorageChallenge.Controllers
{
    public class TestController : ApiController
    {
        private TestProcessor test = new TestProcessor();
        private FileTestData fileTestData
        {
            get
            {
                return new FileTestData
                {
                    Advanced = true,
                    storageAccountKey = CloudConfigurationManager.GetSetting("StorageAccountKey"),
                    storageAccountName = CloudConfigurationManager.GetSetting("StorageAccountName"),
                    storageAccountSAS = CloudConfigurationManager.GetSetting("StorageAccountSAS")
                };
            }
        }
        private SQLTestData sqlTestData
        {
            get
            {
                return new SQLTestData
                {
                    Advanced = true,
                    MySQLConnection = System.Configuration.ConfigurationManager.ConnectionStrings["CustomerDataConnectionString"].ConnectionString,
                    SQLConnection = System.Configuration.ConfigurationManager.ConnectionStrings["CorpDataConnectionString"].ConnectionString
                };
            }
        }
        private CosmosTestData cosmosTestData
        {
            get
            {
                return new CosmosTestData
                {
                    Advanced = true,
                    CosmosDBKey = CloudConfigurationManager.GetSetting("ListingsKey"),
                    CosmosDBUri = CloudConfigurationManager.GetSetting("ListingsURI"),
                    SearchKey = CloudConfigurationManager.GetSetting("SearchKey"),
                    SearchName = CloudConfigurationManager.GetSetting("SearchAccount"),

                };
            }
        }


        [HttpGet]
        public TestResult PublicStorage()
        {
            var t = test.TestPublicBlob(this.fileTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult PrivateStorage()
        {
            var t = test.TestPrivateBlob(this.fileTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult SQLData()
        {
            var t = test.TestSQLServer(this.sqlTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult MySQLData()
        {
            var t = test.TestMySQL(this.sqlTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult CosmosDB()
        {
            var t = test.TestCosmos(this.cosmosTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult Search()
        {
            var t = test.TestSearch(this.cosmosTestData);
            var result = new TestResult { Passed = t.Passed, Status = t.Status };
            return result;
        }
        [HttpGet]
        public TestResult TestRun(bool passed)
        {
            var result = new TestResult() { Passed = passed, Status = passed ? "Congrats, you passed" : "Fail" };

            return result;
        }




    }
    public class TestResult
    {
        public bool Passed { get; set; }
        public string Status { get; set; }
    }
}
