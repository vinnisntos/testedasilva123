using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;

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

        public async Task OnGetAsync(string terminal)
        {
            Terminal = terminal;

            // Conta a posição baseada no campo TerminalDesejado
            PosicaoNaLista = await _context.ListaEspera
                .CountAsync(l => l.TerminalDesejado == terminal);
        }
    }
}
