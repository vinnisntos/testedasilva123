using System;
using System.Threading.Tasks;

namespace Mocidade015.Services
{
    public interface IReservaService
    {
        Task<bool> ReservarAssentoAsync(Guid usuarioId, Guid assentoId, Guid? acompanhanteId);
        Task VerificarEGerarNovoOnibusAsync(string terminal);
        Task<bool> CancelarReservaAsync(Guid reservaId);
        Task<bool> AdicionarNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado);
        Task<bool> JaEstaNaListaDeEsperaAsync(Guid usuarioId, string terminalDesejado);
        Task<int> GetAssentosDisponiveisCountAsync(Guid onibusId);
    }
}