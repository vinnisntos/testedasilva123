using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using System.Security.Claims;

namespace Mocidade015.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        public LoginModel(AppDbContext context) { _context = context; }

        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Senha { get; set; } = "";

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                ModelState.AddModelError("", "E-mail e senha são obrigatórios.");
                return Page();
            }

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Senha, user.SenhaHash))
            {
                ModelState.AddModelError("", "E-mail ou senha inválidos.");
                return Page();
            }

            // CRIANDO AS CREDENCIAIS (CLAIMS)
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Email, user.Email),
                // ESSA LINHA É A CHAVE: Diz ao sistema qual a Role dele
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // Se for admin, já manda direto pro painel dele, se não, vai pro Dashboard normal
            if (user.Role == "Admin") return RedirectToPage("/Admin/Index");

            return RedirectToPage("/App/Dashboard");
        }
    }
}