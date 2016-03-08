using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyORM.Samples.Models;

namespace EasyORM.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("{0}Query{0}", "-------------------");
            var dataContext = new DataContext("SQLServer");
            var dbSets = dataContext.Set<T_User>();
            var query = from q in dbSets
                        select q;
            foreach (var item in query)
            {
                Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
            }
            Console.WriteLine("{0}Insert{0}", "-------------------");
            var user = new T_User();
            user.ID = 19;
            user.Name = "Tommy";
            user.Age = 20;
            var user1 = new T_User();
            user1.ID = 20;
            user1.Name = "David";
            user1.Age = 20;
            dbSets.Add(user);
            dbSets.Add(user1);
            dataContext.SaveChanges();
            foreach (var item in dbSets)
            {
                Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
            }
            Console.WriteLine("{0}Update{0}", "-------------------");
            var model = dbSets.FirstOrDefault();
            if (model != null)
            {
                model.Name += model.Age;
            }
            dataContext.SaveChanges();
            foreach (var item in dbSets)
            {
                Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
            }
            Console.WriteLine("{0}Delete{0}", "-------------------");
            model = dbSets.FirstOrDefault(u => u.ID == 11);
            if (model != null)
            {
                dbSets.Remove(model);
            }
            dataContext.SaveChanges();
            foreach (var item in dbSets)
            {
                Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
            }
            Console.ReadKey();
        }
    }
}
