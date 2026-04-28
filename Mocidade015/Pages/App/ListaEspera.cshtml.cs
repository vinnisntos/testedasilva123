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

        public void OnGet(string terminal)
        {
            Terminal = terminal;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            if (string.IsNullOrEmpty(Terminal)) return Page();

            // Salva na tabela ListaEspera
            var entrada = new ListaEspera
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                TerminalSaida = Terminal, // Ajuste para o nome exato da sua coluna no banco
                DataSolicitacao = DateTime.Now
            };

            _context.ListaEspera.Add(entrada);
            await _context.SaveChangesAsync();

            // Redireciona passando o terminal como parâmetro de rota
            return RedirectToPage("/App/ListaEsperaConfirmado", new { terminal = Terminal });
        }
    }
}
