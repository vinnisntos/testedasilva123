using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class Reserva
    {
        public Guid Id { get; set; }

        [Column("usuarioid")]
        public Guid UsuarioId { get; set; }

        [Column("assentoid")]
        public Guid AssentoId { get; set; }

        [Column("acompanhanteid")]
        public Guid? AcompanhanteId { get; set; }

        public decimal Valor { get; set; } = 80.00m;
        public DateTime DataReserva { get; set; } = DateTime.UtcNow;

        // Propriedades de Navegação
        public Usuario Usuario { get; set; } = null!;
        public Assento Assento { get; set; } = null!;
        public Acompanhante? Acompanhante { get; set; }
    }
}