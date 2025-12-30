using System;
using System.Text;

namespace calculotrabalista.Services
{
    public static class PixHelper
    {
        public static string GerarPayload(string chavePix, string nomeBeneficiario, string cidade)
        {
            // Limpa e prepara os dados
            string chave = chavePix.Trim();
            string nome = nomeBeneficiario.Length > 25 ? nomeBeneficiario.Substring(0, 25) : nomeBeneficiario;
            string cidadeFormatada = cidade.Length > 15 ? cidade.Substring(0, 15) : cidade;
            
            // Remove acentos (bancos preferem sem)
            nome = RemoverAcentos(nome);
            cidadeFormatada = RemoverAcentos(cidadeFormatada);

            // Monta a estrutura EMV (Padrão do Banco Central)
            var sb = new StringBuilder();
            
            sb.Append("000201"); // Payload Format Indicator
            sb.Append(MontarCampo("26", $"0014BR.GOV.BCB.PIX01{chave.Length:D2}{chave}")); // Merchant Account Information
            sb.Append("52040000"); // Merchant Category Code
            sb.Append("5303986"); // Transaction Currency (BRL)
            sb.Append("5802BR"); // Country Code
            sb.Append(MontarCampo("59", nome)); // Merchant Name
            sb.Append(MontarCampo("60", cidadeFormatada)); // Merchant City
            sb.Append("62070503***"); // Additional Data Field Template (TxID ***)
            sb.Append("6304"); // CRC16

            // Calcula o Checksum (CRC16)
            string payloadSemCrc = sb.ToString();
            string crc = CalcularCRC16(payloadSemCrc);
            
            return payloadSemCrc + crc;
        }

        private static string MontarCampo(string id, string valor)
        {
            return $"{id}{valor.Length:D2}{valor}";
        }

        private static string RemoverAcentos(string texto)
        {
            // Simplificado para exemplo
            return texto.Replace("ã", "a").Replace("á", "a").Replace("ç", "c").ToUpper(); 
        }

        private static string CalcularCRC16(string data)
        {
            int crc = 0xFFFF;
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            foreach (byte b in bytes)
            {
                crc ^= (b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (crc << 1) ^ 0x1021;
                    else
                        crc <<= 1;
                }
            }
            return (crc & 0xFFFF).ToString("X4"); // Retorna Hexadecimal
        }
    }
}