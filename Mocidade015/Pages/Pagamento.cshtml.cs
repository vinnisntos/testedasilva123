using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mocidade015.Pages
{
    public class PagamentoModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid LoteId { get; set; }

        public decimal ValorTotalReal { get; set; }
        public int QuantidadePessoas { get; set; }

        public void OnGet()
        {
            // AQUI ENTRA O ENTITY FRAMEWORK DEPOIS
            // var lote = _context.Lotes.Include(l => l.Passageiros).FirstOrDefault(l => l.Id == LoteId);

            // Por enquanto, pra tela não quebrar enquanto não temos banco, vamos simular que ele achou no banco:
            QuantidadePessoas = 1; // Fictício até o banco rodar
            ValorTotalReal = QuantidadePessoas * 30.00m;
        }
    }
}