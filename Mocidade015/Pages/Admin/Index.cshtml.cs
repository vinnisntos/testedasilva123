using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Services;

namespace Mocidade015.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IReservaService _reservaService;

    public IndexModel(AppDbContext context, IReservaService reservaService)
    {
        _context = context;
        _reservaService = reservaService;
    }

    public List<OnibusRelatorioDTO> RelatorioOnibus { get; set; } = new();
    public List<ListaEspera> FilaEspera { get; set; } = new();

    public async Task OnGetAsync()
    {
        // 1. Carrega os ônibus e as reservas vinculadas aos assentos
        RelatorioOnibus = await _context.Onibus
            .OrderBy(o => o.Numero)
            .Select(o => new OnibusRelatorioDTO
            {
                Id = o.Id,
                Numero = o.Numero,
                Terminal = o.TerminalSaida,
                LotacaoMaxima = o.LotacaoMaxima,
                Reservas = _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Acompanhante)
                    .Include(r => r.Assento)
                    .Where(r => r.Assento.OnibusId == o.Id)
                    .OrderBy(r => r.Assento.Numero)
                    .ToList()
            }).ToListAsync();

        // 2. Carrega a Lista de Espera (Propriedade do seu AppDbContext)
        FilaEspera = await _context.ListaEspera
            .Include(l => l.Usuario)
            .OrderBy(l => l.DataSolicitacao)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCancelarReservaAsync(Guid reservaId)
    {
        var sucesso = await _reservaService.CancelarReservaAsync(reservaId);

        if (sucesso) TempData["Mensagem"] = "Reserva cancelada com sucesso! O assento agora está vago.";
        else TempData["Erro"] = "Erro ao tentar cancelar a reserva.";

        return RedirectToPage();
    }
}

public class OnibusRelatorioDTO
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public string Terminal { get; set; } = "";
    public int LotacaoMaxima { get; set; }
    public List<Reserva> Reservas { get; set; } = new();
    public int TotalOcupados => Reservas.Count;
}