using System.Security.Claims;

namespace yazlab1proje3UI.Services
{
    public class LoginService : ILoginService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public LoginService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor=contextAccessor;
        }

        public string GetUserId => _contextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

        public string GetUserName => _contextAccessor.HttpContext.User.FindFirst("Username")?.Value;
        public string GetCustType => _contextAccessor.HttpContext.User.FindFirst("Type")?.Value;
    }
}
