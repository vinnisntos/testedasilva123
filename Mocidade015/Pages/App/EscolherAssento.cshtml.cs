using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Services;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class EscolherAssentoModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IReservaService _reservaService;

        public EscolherAssentoModel(AppDbContext context, IReservaService reservaService)
        {
            _context = context;
            _reservaService = reservaService;
        }

        public Onibus Onibus { get; set; } = null!;
        public List<Assento> Assentos { get; set; } = new();
        public Guid? AcompanhanteId { get; set; }
        public string NomePassageiro { get; set; } = "Titular";

        // Propriedade para destacar a poltrona atual
        public Guid? AssentoAtualId { get; set; }

        // Propriedade NOVA: Lista com os IDs de quem já pegou lugar
        public List<Guid> AssentosOcupadosIds { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid onibusId, Guid? acompanhanteId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            Onibus = await _context.Onibus.FirstOrDefaultAsync(o => o.Id == onibusId);
            if (Onibus == null) return RedirectToPage("/App/Dashboard");

            // VERIFICA SE O ÔNIBUS ESTÁ CHEIO (64/64 lugares)
            var assentosOcupadosCount = await _context.Assentos
                .Where(a => a.OnibusId == onibusId && a.Ocupado)
                .CountAsync();

            if (assentosOcupadosCount >= Onibus.LotacaoMaxima)
            {
                // Ônibus lotado (64/64) - redireciona para lista de espera
                return RedirectToPage("/App/ListaEspera", new { terminal = Onibus.TerminalSaida });
            }

            Assentos = await _context.Assentos
                .Where(a => a.OnibusId == onibusId)
                .OrderBy(a => a.Numero)
                .ToListAsync();

            AcompanhanteId = acompanhanteId;

            if (acompanhanteId.HasValue)
            {
                var acompanhante = await _context.Acompanhantes.FindAsync(acompanhanteId);
                NomePassageiro = acompanhante?.Nome ?? "Acompanhante";
            }
            else
            {
                NomePassageiro = User.Identity?.Name ?? "Você";
            }

            // Puxa a lista de TODOS os assentos ocupados neste ônibus
            AssentosOcupadosIds = await _context.Reservas
                .Where(r => r.Assento.OnibusId == onibusId)
                .Select(r => r.AssentoId)
                .ToListAsync();

            // Verifica se a pessoa logada já tem uma poltrona reservada
            var reservaExistente = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Assento.OnibusId == onibusId &&
                                          r.UsuarioId == userId &&
                                          r.AcompanhanteId == acompanhanteId);

            if (reservaExistente != null)
            {
                AssentoAtualId = reservaExistente.AssentoId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid onibusId, Guid assentoId, Guid? acompanhanteId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            // VERIFICAÇÃO DUPLA: Ônibus está cheio (64/64)?
            var onibus = await _context.Onibus.FindAsync(onibusId);
            if (onibus != null)
            {
                var assentosOcupadosCount = await _context.Assentos
                    .Where(a => a.OnibusId == onibusId && a.Ocupado)
                    .CountAsync();

                if (assentosOcupadosCount >= onibus.LotacaoMaxima)
                {
                    return RedirectToPage("/App/ListaEspera", new { terminal = onibus.TerminalSaida });
                }
            }

            // Defesa: Verifica se o assento clicado já tem dono
            bool assentoOcupado = await _context.Reservas
                .AnyAsync(r => r.AssentoId == assentoId);

            if (assentoOcupado)
            {
                TempData["Erro"] = "Alguém foi mais rápido no gatilho. Escolha outro assento.";
                return RedirectToPage(new { onibusId, acompanhanteId });
            }

            var reservaExistente = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Assento.OnibusId == onibusId &&
                                          r.UsuarioId == userId &&
                                          r.AcompanhanteId == acompanhanteId);

            if (reservaExistente != null)
            {
                // Altera o assento
                reservaExistente.AssentoId = assentoId;
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Assento alterado com sucesso!";
                return RedirectToPage("/App/Validado");
            }
            else
            {
                // Cria nova reserva
                bool sucesso = await _reservaService.ReservarAssentoAsync(userId, assentoId, acompanhanteId);

                if (sucesso)
                {
                    var o = await _context.Onibus.FindAsync(onibusId);
                    if (o != null) await _reservaService.VerificarEGerarNovoOnibusAsync(o.TerminalSaida);

                    return RedirectToPage("/App/Validado");
                }

                TempData["Erro"] = "Não foi possível realizar a reserva.";
                return RedirectToPage(new { onibusId, acompanhanteId });
            }
        }
    }
}