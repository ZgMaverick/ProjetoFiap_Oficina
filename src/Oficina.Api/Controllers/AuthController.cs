using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Oficina.Api.Controllers;

public record LoginRequest(string Usuario, string Senha);
public record LoginResponse(string Token, DateTime ExpiraEm);

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration config) : ControllerBase
{
    // Usuário de demonstração definido no appsettings (seção Admin).
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login(LoginRequest req)
    {
        if (req.Usuario != config["Admin:Usuario"] || req.Senha != config["Admin:Senha"])
            return Unauthorized();

        var jwt = config.GetSection("Jwt");
        var expiraEm = DateTime.UtcNow.AddHours(8);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Chave"]!));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Expires = expiraEm,
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, req.Usuario)]),
            SigningCredentials = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        });

        return new LoginResponse(token, expiraEm);
    }
}
