using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;
using Mocidade015.Services;
using System.Text;

namespace Mocidade015.Pages
{
    public class TestListaEsperaModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IReservaService _reservaService;

        public string Resultado { get; set; } = string.Empty;

        public TestListaEsperaModel(AppDbContext context, IReservaService reservaService)
        {
            _context = context;
            _reservaService = reservaService;
        }

        public async Task OnGetAsync()
        {
            await RunTestsAsync();
        }

        public async Task OnPostAsync()
        {
            await RunTestsAsync();
        }

        private async Task RunTestsAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TESTE: Lista de Espera ===\n");

            try
            {
                // Teste 1: Adicionar usuário na lista de espera
                sb.AppendLine("Teste 1: Adicionar usuário na lista de espera...");
                var primeiroUsuario = await _context.Usuarios.FirstOrDefaultAsync();
                if (primeiroUsuario != null)
                {
                    var terminalTeste = "Santo Antônio";
                    var resultado = await _reservaService.AdicionarNaListaDeEsperaAsync(primeiroUsuario.Id, terminalTeste);
                    sb.AppendLine($"  Resultado: {(resultado ? "SUCESSO ✓" : "FALHOU ✗")}");

                    if (resultado)
                    {
                        // Verifica se está na lista
                        var naLista = await _reservaService.JaEstaNaListaDeEsperaAsync(primeiroUsuario.Id, terminalTeste);
                        sb.AppendLine($"  Verificação (JaEstaNaLista): {(naLista ? "SUCESSO ✓" : "FALHOU ✗")}");

                        // Conta registros na lista
                        var count = await _context.ListaEspera
                            .CountAsync(l => l.TerminalDesejado == terminalTeste);
                        sb.AppendLine($"  Total de pessoas na lista: {count}");
                    }
                }
                else
                {
                    sb.AppendLine("  NENHUM USUÁRIO ENCONTRADO");
                }

                // Teste 2: Tentar adicionar o mesmo usuário novamente (deve falhar)
                sb.AppendLine("\nTeste 2: Tentar duplicar entrada na lista (deve falhar)...");
                if (primeiroUsuario != null)
                {
                    var resultado = await _reservaService.AdicionarNaListaDeEsperaAsync(primeiroUsuario.Id, "Santo Antônio");
                    sb.AppendLine($"  Resultado: {(resultado ? "FALHOU - permitiu duplicar ✗" : "SUCESSO - bloqueou duplicidade ✓")}");
                }

                // Teste 3: Adicionar segundo usuário na lista
                sb.AppendLine("\nTeste 3: Adicionar segundo usuário na lista...");
                var segundoUsuario = await _context.Usuarios.Skip(1).FirstOrDefaultAsync();
                if (segundoUsuario != null && segundoUsuario.Id != primeiroUsuario.Id)
                {
                    var resultado = await _reservaService.AdicionarNaListaDeEsperaAsync(segundoUsuario.Id, "Santo Antônio");
                    sb.AppendLine($"  Resultado: {(resultado ? "SUCESSO ✓" : "FALHOU ✗")}");

                    if (resultado)
                    {
                        var count = await _context.ListaEspera
                            .CountAsync(l => l.TerminalDesejado == "Santo Antônio");
                        sb.AppendLine($"  Total de pessoas na lista: {count}");
                    }
                }

                // Teste 4: Listar todos na lista de espera
                sb.AppendLine("\nTeste 4: Listar todos na lista de espera:");
                var listaEspera = await _context.ListaEspera
                    .Include(l => l.Usuario)
                    .OrderBy(l => l.DataSolicitacao)
                    .ToListAsync();

                int posicao = 1;
                foreach (var item in listaEspera)
                {
                    sb.AppendLine($"  {posicao}. {item.Usuario?.Nome} - {item.TerminalDesejado} ({item.DataSolicitacao:dd/MM HH:mm})");
                    posicao++;
                }

                // Teste 5: Verifica estrutura do banco
                sb.AppendLine("\nTeste 5: Estrutura do banco de dados:");
                var totalUsuarios = await _context.Usuarios.CountAsync();
                var totalOnibus = await _context.Onibus.CountAsync();
                var totalAssentos = await _context.Assentos.CountAsync();
                var totalReservas = await _context.Reservas.CountAsync();
                var totalListaEspera = await _context.ListaEspera.CountAsync();

                sb.AppendLine($"  Usuarios: {totalUsuarios}");
                sb.AppendLine($"  Ônibus: {totalOnibus}");
                sb.AppendLine($"  Assentos: {totalAssentos}");
                sb.AppendLine($"  Reservas: {totalReservas}");
                sb.AppendLine($"  Lista de Espera: {totalListaEspera}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\nERRO: {ex.Message}");
                sb.AppendLine($"Detalhes: {ex.InnerException?.Message}");
            }

            sb.AppendLine("\n=== FIM DO TESTE ===");
            Resultado = sb.ToString();
        }
    }
}
