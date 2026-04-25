using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Services;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class ListaEsperaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IReservaService _reservaService;

        public ListaEsperaModel(AppDbContext context, IReservaService reservaService)
        {
            _context = context;
            _reservaService = reservaService;
        }

        [BindProperty]
        public string Terminal { get; set; } = string.Empty;

        public bool JaEstaNaLista { get; set; }
        public int PosicaoNaLista { get; set; }
        public int PessoasNaFrente { get; set; }

        public async Task<IActionResult> OnGetAsync(string terminal)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            Terminal = terminal;

            // Verifica se usuário já tem reserva neste terminal - se sim, não faz sentido estar na lista de espera
            var jaTemReserva = await _context.Reservas
                .Include(r => r.Assento)
                .ThenInclude(a => a.Onibus)
                .AnyAsync(r => r.UsuarioId == userId &&
                               r.Assento.Onibus != null &&
                               r.Assento.Onibus.TerminalSaida == terminal);

            if (jaTemReserva)
            {
                TempData["Erro"] = "Você já possui uma reserva neste terminal.";
                return RedirectToPage("/App/Dashboard");
            }

            // Verifica se já está na lista de espera
            JaEstaNaLista = await _reservaService.JaEstaNaListaDeEsperaAsync(userId, terminal);

            if (JaEstaNaLista)
            {
                // Calcula a posição na lista
                var todosNaLista = await _context.ListaEspera
                    .Where(l => l.TerminalDesejado == terminal)
                    .OrderBy(l => l.DataSolicitacao)
                    .ToListAsync();

                PosicaoNaLista = todosNaLista.FindIndex(l => l.UsuarioId == userId) + 1;
            }

            // Conta pessoas na frente (para quem não está na lista)
            PessoasNaFrente = await _context.ListaEspera
                .CountAsync(l => l.TerminalDesejado == terminal);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string terminal)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            // Verifica duplicidades antes de adicionar
            var jaEstaNaLista = await _reservaService.JaEstaNaListaDeEsperaAsync(userId, terminal);
            if (jaEstaNaLista)
            {
                TempData["Erro"] = "Você já está na lista de espera para este terminal.";
                return RedirectToPage(new { terminal });
            }

            // Verifica se já tem reserva ativa neste terminal
            var jaTemReserva = await _context.Reservas
                .Include(r => r.Assento)
                .ThenInclude(a => a.Onibus)
                .AnyAsync(r => r.UsuarioId == userId &&
                               r.Assento.Onibus != null &&
                               r.Assento.Onibus.TerminalSaida == terminal);

            if (jaTemReserva)
            {
                TempData["Erro"] = "Você já possui uma reserva neste terminal.";
                return RedirectToPage(new { terminal });
            }

            var sucesso = await _reservaService.AdicionarNaListaDeEsperaAsync(userId, terminal);

            if (sucesso)
            {
                // Calcula a posição após entrar na lista
                var posicao = await _context.ListaEspera
                    .Where(l => l.TerminalDesejado == terminal)
                    .CountAsync();

                TempData["Sucesso"] = $"Você entrou na lista de espera! Posição: {posicao}º";
            }
            else
            {
                TempData["Erro"] = "Não foi possível entrar na lista de espera. Você já pode estar cadastrado.";
            }

            return RedirectToPage(new { terminal });
        }
    }
}
