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

        // O nome aqui TEM que ser igual ao que está no HTML
        public List<Onibus> ListaOnibus { get; set; } = new();

        public async Task OnGetAsync()
        {
            ListaOnibus = await _context.Onibus
                .Where(o => o.DataViagem.Date == new DateTime(2026, 7, 26).Date)
                .OrderBy(o => o.HorarioSaida)
                .ToListAsync();
        }
    }
}