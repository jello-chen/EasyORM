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
            var dc = new DataContext("SQLServer");
            var query = from q in dc.Set<T_Department>()
                        select q;
            foreach (var item in query)
            {
                Console.WriteLine("ID:{0},Name:{1}", item.ID, item.Name);
            }
            Console.ReadKey();
        }
    }
}
