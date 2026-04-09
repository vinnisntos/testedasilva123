using System;

namespace Mocidade015.Models
{
    public class Assento
    {
        public Guid Id { get; set; }
        public Guid OnibusId { get; set; }
        public int Numero { get; set; }
        public bool Ocupado { get; set; } = false;

        // Propriedade de navegação para o Ônibus (Relacionamento Pai)
        public Onibus Onibus { get; set; } = null!;

        // REMOVEMOS: public Reserva? Reserva { get; set; }
        // Deixar essa linha aqui é o que faz o Postgres procurar pelo "AssentoId1"
    }
}