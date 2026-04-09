using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;

namespace Mocidade015.Pages
{
    public class CadastroModel : PageModel
    {
        private readonly AppDbContext _context;
        public CadastroModel(AppDbContext context) => _context = context;

        [BindProperty]
        public CadastroInput Input { get; set; } = new();

        public class CadastroInput
        {
            public string Nome { get; set; } = "";
            public string Email { get; set; } = "";
            public string Senha { get; set; } = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Verifica se o e-mail já existe
            var existe = await _context.Usuarios.AnyAsync(u => u.Email == Input.Email);
            if (existe)
            {
                ModelState.AddModelError(string.Empty, "Este e-mail já está cadastrado.");
                return Page();
            }

            // Cria o usuário com Hash de senha (Segurança Sênior)
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = Input.Nome,
                Email = Input.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(Input.Senha),
                Role = "Cliente"
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }
    }
}