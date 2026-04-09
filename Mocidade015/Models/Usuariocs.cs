using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Mocidade015.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Rg { get; set; }
        public string SenhaHash { get; set; } = string.Empty;

        // No banco, para os 3 admins, mude de "Cliente" para "Admin"
        public string Role { get; set; } = "Cliente";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public ICollection<Acompanhante> Acompanhantes { get; set; } = new List<Acompanhante>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}