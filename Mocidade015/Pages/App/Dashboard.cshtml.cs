using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;

namespace Mocidade015.Pages.App
{
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        public DashboardModel(AppDbContext context) => _context = context;

        public List<OnibusViewModel> ListaOnibus { get; set; } = new();

        public async Task OnGetAsync()
        {
            var onibusList = await _context.Onibus
                .Where(o => o.DataViagem.Date == new DateTime(2026, 7, 26).Date)
                .OrderBy(o => o.HorarioSaida)
                .ToListAsync();

            ListaOnibus = new List<OnibusViewModel>();
            foreach (var onibus in onibusList)
            {
                var ocupados = await _context.Assentos
                    .Where(a => a.OnibusId == onibus.Id && a.Ocupado)
                    .CountAsync();

                ListaOnibus.Add(new OnibusViewModel
                {
                    Onibus = onibus,
                    Ocupados = ocupados,
                    LotacaoMaxima = onibus.LotacaoMaxima,
                    EstaLotado = ocupados >= onibus.LotacaoMaxima
                });
            }
        }
    }

    public class OnibusViewModel
    {
        public Onibus Onibus { get; set; } = null!;
        public int Ocupados { get; set; }
        public int LotacaoMaxima { get; set; }
        public bool EstaLotado { get; set; }
    }
}