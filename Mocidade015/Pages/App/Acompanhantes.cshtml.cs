using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class AcompanhantesModel : PageModel
    {
        private readonly AppDbContext _context;
        public AcompanhantesModel(AppDbContext context) => _context = context;

        [BindProperty]
        public Acompanhante NovoAcompanhante { get; set; } = new();

        public List<Acompanhante> ListaAcompanhantes { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CarregarDados();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId)) return RedirectToPage("/Index");

            NovoAcompanhante.Id = Guid.NewGuid();
            NovoAcompanhante.UsuarioResponsavelId = userId;

            _context.Acompanhantes.Add(NovoAcompanhante);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var acompanhante = await _context.Acompanhantes.FindAsync(id);
            if (acompanhante != null)
            {
                _context.Acompanhantes.Remove(acompanhante);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        private async Task CarregarDados()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                ListaAcompanhantes = await _context.Acompanhantes
                    .Where(a => a.UsuarioResponsavelId == userId)
                    .OrderBy(a => a.Nome)
                    .ToListAsync();
            }
        }
    }
}