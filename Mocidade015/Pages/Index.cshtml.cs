using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mocidade015.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Se o cara entrar no site raiz (localhost:7191/) e já estiver logado, 
            // a gente corta caminho e manda ele direto pro painel de viagens.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/App/Dashboard");
            }

            // Se não estiver logado, mostra a página inicial normalmente.
            return Page();
        }

    }
}