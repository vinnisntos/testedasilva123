using Microsoft.EntityFrameworkCore;
using Mocidade015.Data;
using Mocidade015.Models;

namespace Mocidade015.Services
{
    public class ReservaService : IReservaService
    {
        private readonly AppDbContext _context;

        public ReservaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ReservarAssentoAsync(Guid usuarioId, Guid assentoId, Guid? acompanhanteId)
        {
            // 1. Pega o ônibus vinculado a esse assento
            var assento = await _context.Assentos
                .Include(a => a.Onibus)
                .FirstOrDefaultAsync(a => a.Id == assentoId);

            if (assento == null || assento.Ocupado) return false;

            // 2. REGRA: Verifica se este passageiro JÁ está no ônibus
            // Se acompanhanteId for nulo, o passageiro é o próprio Usuário.
            bool jaEstaNoOnibus = await _context.Reservas
                .AnyAsync(r => r.Assento.OnibusId == assento.OnibusId &&
                               ((acompanhanteId != null && r.AcompanhanteId == acompanhanteId) ||
                                (acompanhanteId == null && r.UsuarioId == usuarioId && r.AcompanhanteId == null)));

            if (jaEstaNoOnibus) return false; // Bloqueia a reserva duplicada

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                assento.Ocupado = true;

                var reserva = new Reserva
                {
                    UsuarioId = usuarioId,
                    AcompanhanteId = acompanhanteId,
                    AssentoId = assentoId,
                    Valor = 80.00m,
                    DataReserva = DateTime.UtcNow
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> CancelarReservaAsync(Guid reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Assento)
                .FirstOrDefaultAsync(r => r.Id == reservaId);

            if (reserva == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Libera o assento
                reserva.Assento.Ocupado = false;

                // 2. Remove a reserva
                _context.Reservas.Remove(reserva);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task VerificarEGerarNovoOnibusAsync(string terminal)
        {
            var assentosLivres = await _context.Assentos
                .Include(a => a.Onibus)
                .Where(a => a.Onibus != null && a.Onibus.TerminalSaida == terminal && !a.Ocupado)
                .CountAsync();

            if (assentosLivres == 0)
            {
                var numOnibusAtual = await _context.Onibus
                    .Where(o => o.TerminalSaida == terminal).CountAsync() + 1;

                var novoOnibus = new Onibus
                {
                    Id = Guid.NewGuid(),
                    Numero = numOnibusAtual,
                    TerminalSaida = terminal,
                    HorarioSaida = terminal == "Santo Antônio" ? new TimeSpan(11, 40, 0) : new TimeSpan(12, 00, 0),
                    DataViagem = new DateTime(2026, 07, 26, 0, 0, 0, DateTimeKind.Utc),
                    LotacaoMaxima = 48
                };

                _context.Onibus.Add(novoOnibus);
                await _context.SaveChangesAsync();

                for (int i = 1; i <= 48; i++)
                {
                    _context.Assentos.Add(new Assento { Id = Guid.NewGuid(), OnibusId = novoOnibus.Id, Numero = i, Ocupado = false });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AdicionarNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado)
        {
            // Verifica se já está na lista de espera para este terminal
            var jaEstaNaLista = await _context.ListaEspera
                .AnyAsync(l => l.UsuarioId == usuarioId && l.TerminalDesejado == terminalDesejado);

            if (jaEstaNaLista) return false;

            // Verifica se já tem reserva em algum assento deste terminal
            var jaTemReserva = await _context.Reservas
                .Include(r => r.Assento)
                .ThenInclude(a => a.Onibus)
                .AnyAsync(r => r.UsuarioId == usuarioId &&
                               r.Assento.Onibus != null &&
                               r.Assento.Onibus.TerminalSaida == terminalDesejado);

            if (jaTemReserva) return false;

            var entradaNaLista = new ListaEspera
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                TerminalDesejado = terminalDesejado,
                DataSolicitacao = DateTime.UtcNow
            };

            _context.ListaEspera.Add(entradaNaLista);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> JaEstaNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado)
        {
            return await _context.ListaEspera
                .AnyAsync(l => l.UsuarioId == usuarioId && l.TerminalDesejado == terminalDesejado);
        }

        public async Task<int> GetAssentosDisponiveisCountAsync(Guid onibusId)
        {
            return await _context.Assentos
                .Include(a => a.Onibus)
                .Where(a => a.OnibusId == onibusId && !a.Ocupado)
                .CountAsync();
        }
    }
}