using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mocidade015.Models
{
    public class ListaEspera
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string TerminalDesejado { get; set; } = string.Empty;

        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

        // Propriedade de Navegação
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
}