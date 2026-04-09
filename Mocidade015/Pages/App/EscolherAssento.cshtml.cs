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

        public async Task<IActionResult> OnGetAsync(Guid onibusId, Guid? acompanhanteId)
        {
            Onibus = await _context.Onibus.FirstOrDefaultAsync(o => o.Id == onibusId);
            if (Onibus == null) return RedirectToPage("/App/Dashboard");

            Assentos = await _context.Assentos
                .Where(a => a.OnibusId == onibusId)
                .OrderBy(a => a.Numero)
                .ToListAsync();

            AcompanhanteId = acompanhanteId;

            // Busca o nome para mostrar na tela (Perfume de UI)
            if (acompanhanteId.HasValue)
            {
                var acompanhante = await _context.Acompanhantes.FindAsync(acompanhanteId);
                NomePassageiro = acompanhante?.Nome ?? "Acompanhante";
            }
            else
            {
                NomePassageiro = User.Identity?.Name ?? "Você";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid onibusId, Guid assentoId, Guid? acompanhanteId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            // CHAMA O SERVIÇO COM A NOVA REGRA
            bool sucesso = await _reservaService.ReservarAssentoAsync(userId, assentoId, acompanhanteId);

            if (sucesso)
            {
                // Se o ônibus lotou, o serviço já gerou o próximo automaticamente
                var o = await _context.Onibus.FindAsync(onibusId);
                if (o != null) await _reservaService.VerificarEGerarNovoOnibusAsync(o.TerminalSaida);

                return RedirectToPage("/App/Validado");
            }

            // Se chegou aqui, é porque a poltrona foi pega ou a pessoa já tem reserva no ônibus
            TempData["Erro"] = "Não foi possível realizar a reserva. Verifique se este passageiro já possui um lugar neste ônibus ou se o assento foi ocupado agora.";
            return RedirectToPage(new { onibusId, acompanhanteId });
        }
    }
}