using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mocidade015.Models;
using Mocidade015.Data;

namespace Mocidade015.Pages
{
    public class CadastroModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CadastroModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Lote Lote { get; set; } = new Lote();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // O Dedo-duro: verifica se o C# recebeu a lista vazia
            if (Lote.Passageiros == null || Lote.Passageiros.Count == 0)
            {
                ModelState.AddModelError("", "O sistema não conseguiu ler os dados dos passageiros. Preencha todos os campos obrigatórios.");
                return Page();
            }

            try
            {
                int quantidadePassagens = Lote.Passageiros.Count;
                Lote.ValorTotal = quantidadePassagens * 30.00m;

                Lote.CriadoEm = DateTime.UtcNow;
                foreach (var passageiro in Lote.Passageiros)
                {
                    passageiro.DataNascimento = DateTime.SpecifyKind(passageiro.DataNascimento, DateTimeKind.Utc);
                }

                _context.Lotes.Add(Lote);
                _context.SaveChanges();

                return RedirectToPage("/Pagamento", new { loteId = Lote.Id });
            }
            catch (Exception ex)
            {
                // Se der erro no banco (ex: CPF duplicado), ele avisa aqui
                ModelState.AddModelError("", "Erro ao salvar no banco: " + ex.InnerException?.Message ?? ex.Message);
                return Page();
            }
        }
    }
}