using System;
using System.Collections.Generic;

namespace Mocidade015.Models
{
    public class Onibus
    {
        public Guid Id { get; set; }
        public int Numero { get; set; }
        public string TerminalSaida { get; set; } = string.Empty;
        public TimeSpan HorarioSaida { get; set; }
        public int LotacaoMaxima { get; set; } = 64;
        public DateTime DataViagem { get; set; }

        public ICollection<Assento> Assentos { get; set; } = new List<Assento>();
    }
}