using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        [BindProperty]
        public string Terminal { get; set; } = string.Empty;

        public void OnGet(string terminal)
        {
            Terminal = terminal;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            // Aqui você cria o objeto para salvar no seu Banco de Dados
            // Certifique-se de que sua model se chama 'ListaEspera'
            var entrada = new ListaEspera
            {
                Id = Guid.NewGuid(),
                UsuarioId = userId,
                TerminalSaida = Terminal,
                DataSolicitacao = DateTime.Now
            };

            _context.ListaEspera.Add(entrada);
            await _context.SaveChangesAsync();

            return RedirectToPage("/App/ListaEsperaConfirmado");
        }
    }
}
