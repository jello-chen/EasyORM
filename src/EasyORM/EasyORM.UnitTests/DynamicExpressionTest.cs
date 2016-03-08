using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyORM.UnitTests.Models;

namespace EasyORM.UnitTests
{
    [TestClass]
    public class DynamicExpressionTest
    {
        private DataContext dc = new DataContext("mysqlite");

        [TestMethod]
        public void TestDataContext()
        {
            var dbSet = dc.Set<T_User>();
            foreach (var item in dbSet)
            {
                Console.WriteLine(item.ID);
            }
        }
    }
}
