using System;
using BLTDB;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Configuration;

namespace BLTDB.Test
{
    [TestClass]
    public class UnitTest1
    {
        //test 
        private string connectionString = "metadata=res://*/BLTEntities.csdl|res://*/BLTEntities.ssdl|res://*/BLTEntities.msl;provider=Npgsql;provider connection string=';Database=blt;Host=bltnew.ck2zppz9pgsw.us-east-1.rds.amazonaws.com;Username={0};PASSWORD={1};ApplicationName=blt';";
        
        [TestMethod]
        public void BLTDBConnectionTest()
        {
            using (bltEntities context = new bltEntities(string.Format(connectionString, ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"])))
            {
                DbConnection conn = context.Database.Connection;
                try
                {
                    if (!context.Database.Exists()) throw new Exception("db does ont exist");
                    conn.Open();
                    Assert.IsTrue(true);

                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, ex.Message);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }

            }
        }//end NSSDBConnectionTest

         [TestMethod]
        public void BLTDBQueryTest()
        {
            using (bltEntities context = new bltEntities(string.Format(connectionString, ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"])))
            {
                try
                {
                    var testQuery = context.ai_class.ToList();
                    Assert.IsNotNull(testQuery, testQuery.Count.ToString());
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, ex.Message);
                }

            }
        }//end NSSDBConnectionTest
    }
    
}
