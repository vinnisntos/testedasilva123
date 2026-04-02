using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;

namespace Mocidade015.Pages
{
    public class PagamentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // A MESMA DATA LIMITE
        private readonly DateTime DataLimitePagamento = new DateTime(2026, 4, 18, 23, 59, 59, DateTimeKind.Utc);

        public PagamentoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public Guid LoteId { get; set; }

        public decimal ValorPix { get; set; }
        public decimal ValorCartao { get; set; }
        public string? PixQrCodeBase64 { get; set; }
        public string? PixCopiaECola { get; set; }

        // Propriedades para travar a tela
        public bool PagamentoBloqueado { get; set; }
        public string MensagemBloqueio { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var lote = await _context.Lotes
                                     .Include(l => l.Passageiros)
                                     .FirstOrDefaultAsync(l => l.Id == LoteId);

            if (lote == null) return RedirectToPage("/Index");

            // 1. TRAVA: Já está pago?
            if (lote.Status == "Pago")
            {
                PagamentoBloqueado = true;
                MensagemBloqueio = "Este lote já está pago! Suas vagas estão garantidas.";
                return Page();
            }

            // 2. TRAVA: Passou do prazo?
            if (DateTime.UtcNow > DataLimitePagamento)
            {
                PagamentoBloqueado = true;
                MensagemBloqueio = "O prazo para pagamento desta reserva expirou. Infelizmente, a vaga foi liberada.";
                return Page();
            }

            int qtdPassageiros = lote.Passageiros.Count > 0 ? lote.Passageiros.Count : 1;
            ValorPix = 80.00m * qtdPassageiros;

            decimal taxaCartao = 0.0498m;
            ValorCartao = Math.Round(ValorPix / (1m - taxaCartao), 2, MidpointRounding.AwayFromZero);

            var request = new PaymentCreateRequest
            {
                TransactionAmount = ValorPix,
                Description = $"Excursão Mocidade 015 - {qtdPassageiros} Passagem(ns) (PIX)",
                PaymentMethodId = "pix",
                ExternalReference = lote.Id.ToString(), // Rastreador do Pix
                DateOfExpiration = DataLimitePagamento, // Validade do Pix
                Payer = new PaymentPayerRequest { Email = lote.EmailTitular }
            };

            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(request);

            if (payment.PointOfInteraction?.TransactionData != null)
            {
                PixQrCodeBase64 = payment.PointOfInteraction.TransactionData.QrCodeBase64;
                PixCopiaECola = payment.PointOfInteraction.TransactionData.QrCode;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessarCartaoAsync([FromBody] MpCardRequest data, [FromQuery] Guid loteId)
        {
            if (data == null) return BadRequest(new { erro = "Dados vazios" });

            var lote = await _context.Lotes.Include(l => l.Passageiros).FirstOrDefaultAsync(l => l.Id == loteId);
            if (lote == null) return BadRequest(new { erro = "Lote não encontrado" });

            // Travas de segurança no POST
            if (lote.Status == "Pago") return BadRequest(new { erro = "Lote já está pago." });
            if (DateTime.UtcNow > DataLimitePagamento) return BadRequest(new { erro = "Prazo expirado." });

            int qtdPassageiros = lote.Passageiros.Count > 0 ? lote.Passageiros.Count : 1;
            decimal valorEsperadoCartao = Math.Round((80.00m * qtdPassageiros) / (1m - 0.0498m), 2, MidpointRounding.AwayFromZero);

            var request = new PaymentCreateRequest
            {
                TransactionAmount = valorEsperadoCartao,
                Token = data.Token,
                Description = $"Excursão Mocidade 015 - {qtdPassageiros} Passagem(ns) (Cartão)",
                Installments = data.Installments,
                PaymentMethodId = data.PaymentMethodId,
                IssuerId = data.IssuerId,
                ExternalReference = lote.Id.ToString(),
                Payer = new PaymentPayerRequest
                {
                    Email = data.Payer.Email,
                    Identification = new IdentificationRequest
                    {
                        Type = data.Payer.Identification.Type,
                        Number = data.Payer.Identification.Number
                    }
                }
            };

            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(request);

            if (payment.Status == "approved" || payment.Status == "in_process")
            {
                lote.Status = "Pago";
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, status = payment.Status });
            }

            return BadRequest(new { success = false, status = payment.StatusDetail });
        }
    }

    public class MpCardRequest { public string Token { get; set; } public int Installments { get; set; } public string PaymentMethodId { get; set; } public string IssuerId { get; set; } public MpPayer Payer { get; set; } }
    public class MpPayer { public string Email { get; set; } public MpIdentification Identification { get; set; } }
    public class MpIdentification { public string Type { get; set; } public string Number { get; set; } }
}