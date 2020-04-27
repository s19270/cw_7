using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cw_3.Login;
using cw_3.Models;
using cw_3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace cw_3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    //[Authorize(Roles = "employee")]
    public class EnrollmentsController : ControllerBase
    {
        public readonly string conString = "Data Source=db-mssql;Initial Catalog=s19270;Integrated Security=True";
        IStudentDbService service = new SqlServerDbService();
        public IConfiguration Configuration { get; set; }
        public EnrollmentsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [HttpPost]
        public IActionResult CreateStudent(NewStudent student)
        {
            return Ok(service.AddStudent(student));
        }
        [HttpPost("promotions")]
        public IActionResult Promote(string studies, int semester)
        {
            return Ok((studies + ", " + semester));
            return Ok(service.PromoteStudents(studies, semester));
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest login)
        {
            if (!service.Logging(login.login, login.password)) return Unauthorized("Brak ucznia w bazie");
            var claims = new[]
{
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "doman"),
                new Claim(ClaimTypes.Role, "employee"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });
        }
    }
}