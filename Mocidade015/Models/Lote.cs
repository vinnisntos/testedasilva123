namespace Mocidade015.Models
{
    public class Lote
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EmailTitular { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Status { get; set; } = "Em Análise";
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public List<Passageiro> Passageiros { get; set; } = new List<Passageiro>();
    }

    public class Passageiro
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LoteId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Rg { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string Telefone { get; set; } = string.Empty;
        public string NomeMae { get; set; } = string.Empty;
        public string? ContatoEmergencia1 { get; set; }
        public string? ContatoEmergencia2 { get; set; }
        public string ComumCongregacao { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public bool Batizado { get; set; }
    }
}