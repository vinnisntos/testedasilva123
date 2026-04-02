using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Models;
using Mocidade015.Data;

namespace Mocidade015.Pages
{
    public class CadastroModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // REGRA DE NEGÓCIO: Pagamento deve ser confirmado até 15 dias antes (11/07/2026)
        // Se passar dessa data, as reservas "Pendentes" deixam de ocupar as 64 vagas.
        private readonly DateTime DataLimitePagamento = new DateTime(2026, 7, 11, 23, 59, 59, DateTimeKind.Utc);

        public CadastroModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Lote Lote { get; set; } = new Lote();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Lote.Passageiros == null || Lote.Passageiros.Count == 0)
            {
                ModelState.AddModelError("", "O sistema não conseguiu ler os dados dos passageiros.");
                return Page();
            }

            try
            {
                bool jaPassouDoPrazoGeral = DateTime.UtcNow > DataLimitePagamento;

                // 1. REGRA DAS 64 VAGAS
                // Conta quem já pagou OU quem está pendente mas ainda está dentro do prazo de 15 dias antes.
                int vagasOcupadas = await _context.Passageiros
                    .CountAsync(p => p.Lote.Status == "Pago" ||
                                    (!jaPassouDoPrazoGeral && p.Lote.Status == "Pendente"));

                if (vagasOcupadas + Lote.Passageiros.Count > 64)
                {
                    ModelState.AddModelError(string.Empty, "Infelizmente o ônibus atingiu a lotação máxima de 64 lugares.");
                    return Page();
                }

                // 2. REGRA DE DUPLICIDADE (CPF)
                var cpfsNovos = Lote.Passageiros.Select(p => p.Cpf).ToList();
                bool cadastroDuplicado = await _context.Passageiros
                    .AnyAsync(p => cpfsNovos.Contains(p.Cpf) &&
                                  (p.Lote.Status == "Pago" || (!jaPassouDoPrazoGeral && p.Lote.Status == "Pendente")));

                if (cadastroDuplicado)
                {
                    ModelState.AddModelError(string.Empty, "Um dos passageiros já possui uma reserva ativa (Paga ou Pendente no prazo).");
                    return Page();
                }

                // 3. PROCESSAMENTO DOS DADOS
                int quantidadePassagens = Lote.Passageiros.Count;
                Lote.ValorTotal = quantidadePassagens * 80.00m;
                Lote.Status = "Pendente";
                Lote.CriadoEm = DateTime.UtcNow;

                foreach (var passageiro in Lote.Passageiros)
                {
                    passageiro.DataNascimento = DateTime.SpecifyKind(passageiro.DataNascimento, DateTimeKind.Utc);
                }

                _context.Lotes.Add(Lote);
                await _context.SaveChangesAsync();

                // 4. DISPARO DO E-MAIL DE PRÉ-RESERVA
                EnviarEmailReserva(Lote.EmailTitular, Lote.Id);

                return RedirectToPage("/Pagamento", new { loteId = Lote.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao processar cadastro: " + ex.Message);
                return Page();
            }
        }

        private void EnviarEmailReserva(string emailDestino, Guid loteId)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("SEU_EMAIL@gmail.com", "uvxxajfaqrxqvsvi"),
                    EnableSsl = true,
                };

                string linkPagamento = $"https://mocidade015.azurewebsites.net/Pagamento?loteId={loteId}";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("SEU_EMAIL@gmail.com", "Mocidade 015 - Excursão"),
                    Subject = "Sua pré-reserva está garantida! 🚌",
                    Body = $@"
                        <div style='font-family: sans-serif; max-width: 600px; border: 1px solid #eee; padding: 20px;'>
                            <h2 style='color: #4CA3ED;'>Tudo certo com seu cadastro!</h2>
                            <p>Sua vaga está pré-reservada. Para garantir seu lugar no ônibus, o pagamento deve ser confirmado até o dia <strong>11/07/2026</strong>.</p>
                            <p><strong>Atenção:</strong> Após essa data, as vagas não pagas serão liberadas automaticamente para outras pessoas.</p>
                            <br>
                            <a href='{linkPagamento}' style='background-color: #4CA3ED; color: white; padding: 15px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>PAGAR AGORA (PIX OU CARTÃO)</a>
                            <br><br>
                            <p>Guarde este e-mail. Você pode usar o link acima para pagar quando quiser, desde que seja antes do prazo final.</p>
                            <p>Deus abençoe!</p>
                        </div>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(emailDestino);
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro e-mail: " + ex.Message);
            }
        }
    }
}