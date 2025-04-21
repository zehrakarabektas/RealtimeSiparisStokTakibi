using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yazlab1proje3webapi.Classes;
using yazlab1proje3webapi.Dtos.AdminDtos;
using yazlab1proje3webapi.Dtos.Customer;
using yazlab1proje3webapi.Dtos.LoginDtos;
using yazlab1proje3webapi.Models.Context;
using yazlab1proje3webapi.Tools;

namespace yazlab1proje3webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly Context _context;
        private readonly Hash _sifreleme;
        public LoginController(Context context, Hash sifreleme) 
        {
            _context=context;
            _sifreleme=sifreleme;
        }

        [HttpPost("CustomerLogin")]
        public async Task<IActionResult> CustomerLogin(CustomerLoginDto loginDto)
        {
            string query = "Select * From Customers Where CustomerEmail=@CustomerEmail and  Password=@Password";
            var paramaters = new DynamicParameters();
            paramaters.Add("@CustomerEmail", loginDto.CustomerMail);
            paramaters.Add("@Password",_sifreleme.Sifrele(loginDto.Password));

            using (var connection = _context.CreateConnection()) 
            { 
                var value=await connection.QueryFirstOrDefaultAsync<ResultCustomerDto>(query,paramaters);
                if (value != null)
                {
                    GetCheckAppUserViewModel model= new GetCheckAppUserViewModel();
                    model.Id=value.CustomerID;
                    model.UserName=value.CustomerName;
                    model.Role="Customer";
                    model.Type=value.CustomerType;

                    var token=JwtTokenGenerator.GenerateToken(model);
                    return Ok(token);
                
                }
                else
                {
                    return Ok("Başarısız");
                }
            }
        }
        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin(AdminLoginDto loginDto)
        {
            string query = "Select * From Admin Where AdminEmail=@AdminMail and  Password=@Password";
            var paramaters = new DynamicParameters();
            paramaters.Add("@AdminMail", loginDto.AdminMail);
            paramaters.Add("@Password", _sifreleme.Sifrele(loginDto.Password));

            using (var connection = _context.CreateConnection())
            {
                var value = await connection.QueryFirstOrDefaultAsync<ResultAdminDto>(query, paramaters);
                if (value != null)
                {
                    GetCheckAppUserViewModel model = new GetCheckAppUserViewModel();
                    model.Id=value.AdminId;
                    model.UserName=value.AdminName;
                    model.Role="Admin";
                    model.Type="Admin";
                    var token = JwtTokenGenerator.GenerateToken(model);
                    return Ok(token);

                }
                else
                {
                    return Ok("Başarısız");
                }
            }
        }
    }
}
