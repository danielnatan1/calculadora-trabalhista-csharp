using System;
using System.ComponentModel.DataAnnotations;

namespace calculotrabalista.ViewModels
{
    public class DadosTrabalhistaVM
    {
        public bool CalculoRealizado { get; set; } = false;

        // --- NOVO CAMPO ---
        [Display(Name = "Cumpriu o Aviso Prévio?")]
        public bool CumpriuAvisoPrevio { get; set; } = true; // Começa marcado como SIM
        // ------------------

        [Required(ErrorMessage = "Informe a data de admissão")]
        [DataType(DataType.Date)]
        public DateTime? DataAdmissao { get; set; }

        [Required(ErrorMessage = "Informe a data de demissão")]
        [DataType(DataType.Date)]
        public DateTime? DataDemissao { get; set; }

        [Required(ErrorMessage = "Informe o salário")]
        public string SalarioBrutoTexto { get; set; } = string.Empty;

        public decimal SalarioBruto { get; set; }

        public string MotivoDemissao { get; set; } = "SemJustaCausa";

        // SAÍDA DOS DADOS
        public decimal SaldoSalario { get; set; }
        
        public decimal AvisoPrevioValor { get; set; }
        public string AvisoPrevioDescricao { get; set; } = string.Empty;
        
        // Novo campo para o valor do desconto
        public decimal DescontoAvisoPrevio { get; set; } 

        public decimal FeriasVencidas { get; set; }
        public decimal FeriasProporcionais { get; set; }
        public decimal TercoFerias { get; set; }
        public decimal DecimoTerceiro { get; set; }
        
        public decimal FGTS_Saldo { get; set; }
        public decimal FGTS_Multa { get; set; }
        
        public decimal DescontoINSS { get; set; }
        public decimal DescontoIRRF { get; set; }

        public decimal TotalProventos { get; set; }
        public decimal TotalDescontos { get; set; }
        public decimal TotalLiquido { get; set; }
        
        public string PayloadPix { get; set; } = string.Empty; 
    }
}