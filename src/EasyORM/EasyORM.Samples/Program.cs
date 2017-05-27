using System;
using System.Linq;
using EasyORM.Samples.Models;

namespace EasyORM.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var rules = string.Join(string.Empty, Enumerable.Range(1, 20).Select(t => "-"));

            Edit();

            Edit1();

            Query();


            //var dataContext = new DataContext("SQLServer");
            //var departments = dataContext.Set<T_Department>();
            //var students = dataContext.Set<Student>();

            //Console.WriteLine($"{rules}Query{rules}");
            //var query = from s in departments
            //            select s;
            //foreach (var item in query)
            //{
            //    Console.WriteLine($"ID:{item.ID},Name:{item.Name}");
            //}

            //Console.WriteLine($"{rules}Insert{rules}");
            //for (int i = 0; i < 10; i++)
            //{
            //    var department = new T_Department
            //    {
            //        Name = $"D{i}"
            //    };
            //    departments.Add(department);
            //}
            
            //for (int i = 0; i < 10; i++)
            //{
            //    var student = new T_Student
            //    {
            //        Name = $"Name{i}",
            //        Age = 20 + i,
            //        DepartmentID = i
            //    };
            //    students.Add(student);
            //}
            //dataContext.SaveChanges();

            //Console.WriteLine($"{rules}Join{rules}");
            //var j = from q in students
            //            join p in departments
            //            on q.DepartmentID equals p.ID
            //            select new {
            //                ID = q.ID,
            //                StudentName = q.Name,
            //                StudentAge = q.Age,
            //                DepartmentName = p.Name
            //            };
            //foreach (var item in j)
            //{
            //    Console.WriteLine($"ID:{item.ID},StudentName:{item.StudentName},StudentAge:{item.StudentAge},DepartmentName:{item.DepartmentName}");
            //}

            //Console.WriteLine($"{rules}Delete{rules}");
            //var studentModel = students.FirstOrDefault(t => t.DepartmentID == 0);
            //students.Remove(studentModel);
            //dataContext.SaveChanges();

            Console.ReadKey();
        }

        static void Edit()
        {
            var dataContext = new DataContext("SQLServer");
            var students = dataContext.Set<Student>();
            var studentModel1 = students.FirstOrDefault(t => t.ID == 2);
            studentModel1.Age = 20;
            dataContext.SaveChanges();
        }

        static void Edit1()
        {
            var dataContext = new DataContext("SQLServer");
            var students = dataContext.Set<Student>();
            var studentModel1 = students.FirstOrDefault(t => t.ID == 2);
            studentModel1.Age = 21;
            dataContext.SaveChanges();
        }

        static void Query()
        {
            var dataContext = new DataContext("SQLServer");
            var students = dataContext.Set<Student>();
            foreach (var student in students)
            {
                Console.WriteLine($"ID:{student.ID},Age:{student.Age}");
            }
        }
    }
}
