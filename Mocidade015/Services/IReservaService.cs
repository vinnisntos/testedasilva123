using System;
using System.Threading.Tasks;

namespace Mocidade015.Services
{
    public interface IReservaService
    {
        Task<bool> ReservarAssentoAsync(Guid usuarioId, Guid assentoId, Guid? acompanhanteId);
        Task VerificarEGerarNovoOnibusAsync(string terminal);

        Task<bool> CancelarReservaAsync(Guid reservaId);
    }
}