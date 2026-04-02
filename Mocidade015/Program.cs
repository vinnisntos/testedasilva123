using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using MercadoPago.Config;
using MercadoPago.Client.Payment;
using System.Text.Json;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados (PostgreSQL/Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Adiciona suporte a Razor Pages (suas telas de Cadastro e Pagamento)
builder.Services.AddRazorPages();

var app = builder.Build();

// 3. Configura o Token do Mercado Pago (Sua Chave Privada)
// IMPORTANTE: Substitua pela sua Access Token real (aquela que começa com APP_USR-)
MercadoPagoConfig.AccessToken = "APP_USR-7192921073976488-032422-0221b3558397b961ac1cdb885f669209-2215635803";

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

// --- 4. O WEBHOOK (A Campainha do Pix) ---
// Esta rota fica "escutando" o Mercado Pago 24h por dia
app.MapPost("/api/webhook", async (HttpContext context, ApplicationDbContext db) =>
{
    try
    {
        // Lê o que o Mercado Pago enviou
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var json = JsonDocument.Parse(body);

        // O Mercado Pago envia o ID do pagamento dentro de data.id
        if (json.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var idProp))
        {
            var paymentId = long.Parse(idProp.GetString());

            // Pergunta para o Mercado Pago se esse pagamento é real
            var client = new PaymentClient();
            var payment = await client.GetAsync(paymentId);

            // Se o status for "approved" e tiver o ID do nosso lote (ExternalReference)
            if (payment.Status == "approved" && Guid.TryParse(payment.ExternalReference, out Guid loteId))
            {
                var lote = await db.Lotes.Include(l => l.Passageiros).FirstOrDefaultAsync(l => l.Id == loteId);

                // Se o lote existir e ainda não estiver como "Pago"
                if (lote != null && lote.Status != "Pago")
                {
                    lote.Status = "Pago";
                    await db.SaveChangesAsync();

                    // --- DISPARA O COMPROVANTE POR E-MAIL ---
                    EnviarEmailConfirmacao(lote.EmailTitular, lote.Passageiros.Count, lote.ValorTotal);

                    Console.WriteLine($"[WEBHOOK] Pagamento do Lote {loteId} confirmado com sucesso!");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WEBHOOK ERROR] {ex.Message}");
    }

    // Retorna sempre 200 OK para o Mercado Pago parar de "tocar a campainha"
    return Results.Ok();
});

app.Run();

// --- FUNÇÃO AUXILIAR DE E-MAIL (Mesma lógica do Gmail que usamos) ---
void EnviarEmailConfirmacao(string emailDestino, int qtd, decimal valor)
{
    try
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("noreplyprojetosvinni@gmail.com", "uvxxajfaqrxqvsvi"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("noreplyprojetosvinni@gmail.com", "Excursão Mocidade 015"),
            Subject = "✅ Pagamento Confirmado! Vaga Garantida",
            Body = $"<h2>Pagamento Aprovado!</h2><p>Recebemos o pagamento de <strong>R$ {valor:N2}</strong> referente a <strong>{qtd} passagem(ns)</strong>.</p><p>Suas vagas estão confirmadas para o dia 26 de Julho. Deus abençoe!</p>",
            IsBodyHtml = true,
        };
        mailMessage.To.Add(emailDestino);
        smtpClient.Send(mailMessage);
    }
    catch (Exception ex) { Console.WriteLine("Erro e-mail webhook: " + ex.Message); }
}