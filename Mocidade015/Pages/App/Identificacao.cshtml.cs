using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using System.Security.Claims;

namespace Mocidade015.Pages.App
{
    public class IdentificacaoModel : PageModel
    {
        private readonly AppDbContext _context;
        public IdentificacaoModel(AppDbContext context) => _context = context;

        public List<Acompanhante> Acompanhantes { get; set; } = new();

        [BindProperty]
        public string PassageiroSelecionado { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                Acompanhantes = await _context.Acompanhantes
                    .Where(a => a.UsuarioResponsavelId == userId)
                    .ToListAsync();
            }
        }

        public IActionResult OnPost(Guid onibusId)
        {
            if (PassageiroSelecionado == "titular")
            {
                // Manda sem acompanhanteId (será o titular)
                return RedirectToPage("/App/EscolherAssento", new { onibusId });
            }
            else
            {
                // Manda com o ID do acompanhante
                return RedirectToPage("/App/EscolherAssento", new { onibusId, acompanhanteId = PassageiroSelecionado });
            }
        }
    }
}