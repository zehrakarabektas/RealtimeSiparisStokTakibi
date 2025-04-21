using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace yazlab1proje3webapi.Tools
{
    public class JwtTokenGenerator
    {
        public static TokenResponseViewModel GenerateToken(GetCheckAppUserViewModel model)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrWhiteSpace(model.Role))
                claims.Add(new Claim(ClaimTypes.Role, model.Role));

            claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()));

            if(!string.IsNullOrWhiteSpace(model.UserName))
                claims.Add(new Claim("Username",model.UserName));

            if (!string.IsNullOrWhiteSpace(model.Type))
                claims.Add(new Claim("Type", model.Type));

            var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenDefault.Key));
            var cred=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var expireDate=DateTime.UtcNow.AddDays(JwtTokenDefault.Expire);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: JwtTokenDefault.ValidIssuer,
                claims: claims,
                audience:JwtTokenDefault.ValidAudience,
                notBefore:DateTime.UnixEpoch,
                expires:expireDate,
                signingCredentials:cred);

            JwtSecurityTokenHandler tokenHandler= new JwtSecurityTokenHandler();

            return new TokenResponseViewModel(tokenHandler.WriteToken(token), expireDate);
        }
    }
}
