using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace Mocidade015.Pages.App
{
    [Authorize]
    public class ValidadoModel : PageModel // <-- O "V" e o "M" maiúsculos são importantes
    {
        public void OnGet()
        {
        }
    }
}