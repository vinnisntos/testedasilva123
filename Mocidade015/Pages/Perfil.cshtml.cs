using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class PerfilModel : PageModel
    {
        private readonly AppDbContext _context;
        public PerfilModel(AppDbContext context) => _context = context;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public string Nome { get; set; } = "";
            public string Email { get; set; } = "";
            public string Rg { get; set; } = "";
            public string? NovaSenha { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Login");

            var usuario = await _context.Usuarios.FindAsync(Guid.Parse(userId));
            if (usuario == null) return RedirectToPage("/Login");

            Input.Nome = usuario.Nome;
            Input.Email = usuario.Email;
            Input.Rg = usuario.Rg ?? "";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _context.Usuarios.FindAsync(Guid.Parse(userId!));

            if (usuario == null) return RedirectToPage("/Login");

            // Atualiza os dados básicos
            usuario.Nome = Input.Nome;
            usuario.Email = Input.Email;
            usuario.Rg = Input.Rg;


            // Se digitou senha nova, faz o Hash
            if (!string.IsNullOrWhiteSpace(Input.NovaSenha))
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(Input.NovaSenha);
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // Pulo do Gato: Atualiza a sessão do usuário com o novo nome
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Mensagem"] = "Perfil atualizado com sucesso!";
            return RedirectToPage();
        }
    }
}