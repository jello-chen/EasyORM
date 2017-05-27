using EasyORM.Samples.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EasyORM.Samples.Web.Controllers
{
    public class StudentController : Controller
    {
        // GET: Student
        public ActionResult Index()
        {
            var dataContext = new DataContext("SQLServer");
            var students = dataContext.Set<Student>();
            return View(students.ToList());
        }

        public ActionResult Edit(int id)
        {
            var dataContext = new DataContext("SQLServer");
            var students = dataContext.Set<Student>();
            var student = students.FirstOrDefault(s => s.ID == id);
            student.Age *= 2;
            dataContext.SaveChanges();
            return Redirect("/Student");
        }
    }
}