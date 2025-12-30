using System;
using calculotrabalista.ViewModels;

namespace calculotrabalista.Services
{
    public class CalculoService
    {
        public DadosTrabalhistaVM Calcular(DadosTrabalhistaVM dados)
        {
            DateTime dtAdmissao = dados.DataAdmissao ?? DateTime.Now; 
            DateTime dtDemissao = dados.DataDemissao ?? DateTime.Now;

            // --- 1. CONTAGEM DE AVOS ---
            int avosTrabalhados = 0;
            DateTime cursor = new DateTime(dtAdmissao.Year, dtAdmissao.Month, 1);
            
            while (cursor <= new DateTime(dtDemissao.Year, dtDemissao.Month, 1))
            {
                DateTime inicioMes = cursor < dtAdmissao ? dtAdmissao : cursor;
                DateTime fimMes = cursor.AddMonths(1).AddDays(-1);
                if (fimMes > dtDemissao) fimMes = dtDemissao;

                int diasNoMes = (int)(fimMes - inicioMes).TotalDays + 1;
                // REGRA DE OURO: >= 15 dias conta como mês cheio
                if (diasNoMes >= 15) avosTrabalhados++;
                
                cursor = cursor.AddMonths(1);
            }

            // Ajuste para 13º (limitado ao ano corrente ou proporcional se < 1 ano)
            int avos13 = avosTrabalhados;
            if (avos13 > 12) avos13 = 12; // Trava em 12 se for mais de um ano


            // --- 2. SALDO DE SALÁRIO ---
            int diasUltimoMes = dtDemissao.Day > 30 ? 30 : dtDemissao.Day;
            // Se trabalhou 31 dias, paga-se 30 na rescisão comercial padrão
            if (dtDemissao.Day == 31) diasUltimoMes = 30; 
            
            dados.SaldoSalario = (dados.SalarioBruto / 30) * diasUltimoMes;


            // --- 3. AVISO PRÉVIO ---
            dados.AvisoPrevioValor = 0;
            dados.DescontoAvisoPrevio = 0; 

            if (dados.MotivoDemissao == "SemJustaCausa")
            {
                int anosCompletos = (int)((dtDemissao - dtAdmissao).TotalDays / 365.25);
                int diasAvisoTotal = 30 + (anosCompletos * 3);
                if (diasAvisoTotal > 90) diasAvisoTotal = 90;

                if (dados.CumpriuAvisoPrevio)
                {
                    int diasProjecao = diasAvisoTotal - 30;
                    dados.AvisoPrevioDescricao = $"Trabalhado (+{diasProjecao} dias indeniz.)";
                    dados.AvisoPrevioValor = (dados.SalarioBruto / 30) * diasProjecao;
                }
                else
                {
                    dados.AvisoPrevioDescricao = $"Indenizado ({diasAvisoTotal} dias)";
                    dados.AvisoPrevioValor = (dados.SalarioBruto / 30) * diasAvisoTotal;
                }
            }
            else if (dados.MotivoDemissao == "PedidoDemissao")
            {
                if (dados.CumpriuAvisoPrevio)
                {
                    dados.AvisoPrevioDescricao = "Trabalhado";
                    dados.AvisoPrevioValor = 0;
                }
                else
                {
                    dados.AvisoPrevioDescricao = "Não Cumprido (Descontado)";
                    dados.AvisoPrevioValor = 0;
                    dados.DescontoAvisoPrevio = dados.SalarioBruto; 
                }
            }
            else 
            {
                dados.AvisoPrevioDescricao = "Perdido (Justa Causa)";
                dados.AvisoPrevioValor = 0;
            }


            // --- 4. FÉRIAS ---
            dados.FeriasVencidas = 0;
            dados.FeriasProporcionais = 0;
            dados.TercoFerias = 0;

            if (dados.MotivoDemissao != "JustaCausa")
            {
                int avosFeriasTotal = avosTrabalhados;
                
                // Se aviso indenizado, ganha +1 avo de férias
                if (dados.MotivoDemissao == "SemJustaCausa" && !dados.CumpriuAvisoPrevio)
                {
                    avosFeriasTotal++;
                }

                // LÓGICA: Só é "Vencida" se completou 1 ano de data a data
                bool completouUmAno = (dtDemissao - dtAdmissao).TotalDays >= 365;

                if (completouUmAno && avosFeriasTotal >= 12)
                {
                    dados.FeriasVencidas = dados.SalarioBruto;
                    avosFeriasTotal -= 12; 
                }
                
                // O resto é proporcional
                dados.FeriasProporcionais = (dados.SalarioBruto / 12) * avosFeriasTotal;
                
                dados.TercoFerias = (dados.FeriasVencidas + dados.FeriasProporcionais) / 3;
            }


            // --- 5. 13º SALÁRIO ---
            if (dados.MotivoDemissao == "JustaCausa") 
            {
                dados.DecimoTerceiro = 0;
            }
            else 
            {
                // Se aviso indenizado, projeta 13º
                if (dados.MotivoDemissao == "SemJustaCausa" && !dados.CumpriuAvisoPrevio)
                {
                     avos13++;
                }
                if (avos13 > 12) avos13 = 12; 

                dados.DecimoTerceiro = (dados.SalarioBruto / 12) * avos13;
            }


            // --- 6. FGTS (CORREÇÃO DE VISUALIZAÇÃO) ---
            dados.FGTS_Saldo = (dados.SalarioBruto * avosTrabalhados) * 0.08m;
            dados.FGTS_Multa = 0;

            if (dados.MotivoDemissao == "SemJustaCausa")
            {
                dados.FGTS_Multa = dados.FGTS_Saldo * 0.40m;
            }
            else
            {
                // Se for Pedido de Demissão ou Justa Causa, o funcionário NÃO SACA.
                // Zeramos para não confundir o usuário.
                dados.FGTS_Saldo = 0; 
                dados.FGTS_Multa = 0;
            }


            // --- 7. TOTAIS ---
            dados.TotalProventos = dados.SaldoSalario + dados.AvisoPrevioValor + 
                                   dados.FeriasVencidas + dados.FeriasProporcionais + 
                                   dados.TercoFerias + dados.DecimoTerceiro;

            // IMPOSTOS
            decimal baseTributavel = dados.TotalProventos; 
            
            // INSS (Simplificado para exemplo - 9% médio)
            dados.DescontoINSS = baseTributavel * 0.09m; 
            if (dados.DescontoINSS > 908.85m) dados.DescontoINSS = 908.85m; 

            // IRRF
            decimal baseIR = baseTributavel - dados.DescontoINSS;
            dados.DescontoIRRF = 0;
            
            // Tabela IRRF Simplificada
            if (baseIR > 2259.20m) 
            {
                dados.DescontoIRRF = (baseIR * 0.075m) - 169.44m; // Faixa 1 exemplo
                if (dados.DescontoIRRF < 0) dados.DescontoIRRF = 0;
            }

            dados.TotalDescontos = dados.DescontoINSS + dados.DescontoIRRF + dados.DescontoAvisoPrevio;

            dados.TotalLiquido = dados.TotalProventos - dados.TotalDescontos;

            // Só soma multa do FGTS no líquido se for Sem Justa Causa (pois é pago na guia rescisória em alguns casos ou saque imediato)
            // Mas para o sistema, como o FGTS vai para a conta vinculada, o ideal é não somar no líquido de rescisão em dinheiro vivo,
            // EXCETO a multa de 40% que em algumas situações antigas caía na conta, mas hoje tudo é GRRF.
            // Para manter a coerência visual:
            if (dados.MotivoDemissao == "SemJustaCausa")
            {
                dados.TotalLiquido += dados.FGTS_Multa;
            }

            return dados;
        }
    }
}