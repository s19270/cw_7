using cw_3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw_3.DAL
{
    public class MockDbService : IDbService
    {
        public static IEnumerable<Student> _students;
        static MockDbService()
        {
            _students = new List<Student>{ 
                new Student{FirstName = "Jan", LastName = "Kowalski"},
                new Student{FirstName = "Robert", LastName = "Lewandowski"},
                new Student{FirstName = "Geralt", LastName = "Riv"},
                new Student{FirstName = "Darth", LastName = "Vader"}
            };
        }
        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
