using cw_3.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace cw_3.Services
{
    public class SqlServerDbService : IStudentDbService
    {
        public static readonly string conString = "Data Source=db-mssql;Initial Catalog=s19270;Integrated Security=True";
        public string AddStudent(NewStudent student)
        {
            if (student.Studies == null || student.FirstName == null ||
                student.LastName == null || student.Birthdate == null ||
                student.IndexNumber == null) return "Brak danych";
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand com = new SqlCommand())
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction("SampleTransaction");
                com.Connection = con;
                com.Transaction = trans;
                try
                {
                    com.CommandText = "select IndexNumber from Student;";
                    SqlDataReader dr = com.ExecuteReader();
                    while (dr.Read())
                    {
                        if (student.IndexNumber == dr["IndexNumber"].ToString()) return "Podany numer indexu juz istnieje";
                    }
                    com.CommandText = "select * from Studies inner join Enrollment on Studies.IdStudy = Enrollment.IdStudy where Name = @studies and Semester = 1;";
                    com.Parameters.AddWithValue("studies", student.Studies);
                    dr.Close();
                    dr = com.ExecuteReader();
                    if (!dr.Read())
                    {
                        com.CommandText = "insert into Studies(IdStudy, Name) " +
                                            "values((select max(IdStudy) from Studies) + 1, @studies) " +
                                            "insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) " +
                                            "values((select max(IdEnrollment) from Enrollment) + 1, 1, (select IdStudy from Studies where Name = @studies), GETDATE())";
                        dr.Close();
                        com.ExecuteNonQuery();
                    }
                    com.CommandText = "insert into Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                                        "values(@indexnumber, @firstname, @lastname, @birthdate, (select IdEnrollment from Enrollment where IdStudy = (select IdStudy from Studies where Name = @studies and Semester = 1)));";
                    com.Parameters.AddWithValue("indexnumber", student.IndexNumber);
                    com.Parameters.AddWithValue("firstname", student.FirstName);
                    com.Parameters.AddWithValue("lastname", student.LastName);
                    com.Parameters.AddWithValue("birthdate", student.Birthdate);
                    dr.Close();
                    com.ExecuteNonQuery();
                    trans.Commit();
                    return "Dodano nowego studenta";
                }
                catch (Exception ex)
                {
                    try
                    {
                        trans.Rollback("Wystapili bledy");
                    }
                    catch (Exception ex2) {
                    }
                }
                return "Wystapili bledy";
            }
        }

        public List<Student> GetStudents()
        {
            var list = new List<Student>();

            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select * from Student";

                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    list.Add(st);
                }

            }
            return (list);
        }

        public bool Logging(string indexnumber, string password)
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand com = new SqlCommand())
            {
                con.Open();
                com.Connection = con;
                com.CommandText = "select 1 from Student where IndexNumber = @indexnumber and Password = @password;";
                com.Parameters.AddWithValue("indexnumber", indexnumber);
                com.Parameters.AddWithValue("password", password);
                SqlDataReader dr = com.ExecuteReader();
                if (dr.Read())
                {
                    return true;
                }
                else return false;
            }
        }

        public String PromoteStudents(string studies, int semester)
        {
            using (SqlConnection con = new SqlConnection(conString))
            using (SqlCommand com = new SqlCommand())
            {
                con.Open();
                SqlTransaction trans = con.BeginTransaction("SampleTransaction");
                com.Connection = con;
                com.Transaction = trans;
                try
                {
                    com.CommandText = "exec Promote @Studies = @studies, @Semester = @semester;";
                    com.Parameters.AddWithValue("studies", studies);
                    com.Parameters.AddWithValue("semester", semester);
                    com.ExecuteNonQuery();
                    trans.Commit();
                    return "Studenci uzyskali promocje";
                }
                catch (Exception ex)
                {
                    try
                    {
                        trans.Rollback("Wystapili bledy");
                    }
                    catch (Exception ex2)
                    {
                    }
                }
                return "Wystapily bledy";
            }
        }
    }
}
