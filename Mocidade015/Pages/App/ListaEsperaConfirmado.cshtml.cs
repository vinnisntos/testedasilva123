using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class ListaEsperaConfirmadoModel : PageModel
    {
        private readonly AppDbContext _context;

        public ListaEsperaConfirmadoModel(AppDbContext context)
        {
            _context = context;
        }

        public string Terminal { get; set; } = string.Empty;
        public int PosicaoNaLista { get; set; }

        public async Task<IActionResult> OnGetAsync(string terminal)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            Terminal = terminal;

            // Verifica se o usuário está realmente na lista de espera
            var naLista = await _context.ListaEspera
                .AnyAsync(l => l.UsuarioId == userId && l.TerminalDesejado == terminal);

            if (!naLista)
            {
                TempData["Erro"] = "Você não está na lista de espera para este terminal.";
                return RedirectToPage("/App/Dashboard");
            }

            // Calcula a posição na lista
            var todosNaLista = await _context.ListaEspera
                .Where(l => l.TerminalDesejado == terminal)
                .OrderBy(l => l.DataSolicitacao)
                .ToListAsync();

            PosicaoNaLista = todosNaLista.FindIndex(l => l.UsuarioId == userId) + 1;

            return Page();
        }
    }
}
