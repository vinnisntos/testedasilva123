using System;

namespace Mocidade015.Models
{
    public class Acompanhante
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UsuarioResponsavelId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? RgCpf { get; set; }

        public Usuario UsuarioResponsavel { get; set; } = null!;
    }
}