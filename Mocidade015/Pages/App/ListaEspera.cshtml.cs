using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class ListaEsperaModel : PageModel
    {
        private readonly AppDbContext _context;

        public ListaEsperaModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Terminal { get; set; } = string.Empty;

        public string NomeUsuario { get; set; } = string.Empty;

        public void OnGet(string terminal)
        {
            Terminal = terminal;
            NomeUsuario = User.Identity?.Name ?? "Usuário";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            if (string.IsNullOrEmpty(Terminal)) return Page();

            var entrada = new ListaEspera
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                TerminalDesejado = Terminal, // Nome corrigido conforme sua Model
                DataSolicitacao = DateTime.Now
            };

            _context.ListaEspera.Add(entrada);
            await _context.SaveChangesAsync();

            return RedirectToPage("/App/ListaEsperaConfirmado", new { terminal = Terminal });
        }
    }
}
