using System;
using System.IO;
using calculotrabalista.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;

namespace calculotrabalista.Services
{
    public class PdfService
    {   
        public byte[] GerarPdfRescisao(DadosTrabalhistaVM dados)
{
            // 1. Gera o Payload Real (caso não tenha vindo da tela, geramos aqui de novo)
            // Se você quiser centralizar a chave, pode colocar em uma constante, mas aqui repetimos por simplicidade
            string payloadPix = PixHelper.GerarPayload("c6482b29-fa1f-4f61-ba4a-f414bf306e26", "Daniel Natan", "Pitangueiras");
            
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payloadPix, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = qrCode.GetGraphic(20);

            // 2. Desenhar o PDF
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Cabeçalho
                    page.Header()
                        .Text("Cálculo de Rescisão Trabalhista")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    // Conteúdo
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Data do Cálculo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        x.Item().Text($"Motivo: {dados.MotivoDemissao}");
                        x.Item().Text("------------------------------------------------");

                        // Tabela de Valores
                        x.Item().Table(tabela =>
                        {
                            tabela.ColumnsDefinition(c => 
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            tabela.Header(h =>
                            {
                                h.Cell().Text("Descrição").Bold();
                                h.Cell().AlignRight().Text("Valor (R$)").Bold();
                            });

                            // Linhas da tabela
                            tabela.Cell().Text("Saldo de Salário");
                            tabela.Cell().AlignRight().Text($"{dados.SaldoSalario:N2}");

                            tabela.Cell().Text("Aviso Prévio");
                            tabela.Cell().AlignRight().Text($"{dados.AvisoPrevioValor:N2}");

                            tabela.Cell().Text("Férias Proporcionais + 1/3");
                            tabela.Cell().AlignRight().Text($"{(dados.FeriasProporcionais + dados.TercoFerias):N2}");

                            tabela.Cell().Text("13º Salário Proporcional");
                            tabela.Cell().AlignRight().Text($"{dados.DecimoTerceiro:N2}");

                            tabela.Cell().Text("FGTS (Depósito + Multa)");
                            tabela.Cell().AlignRight().Text($"{(dados.FGTS_Saldo + dados.FGTS_Multa):N2}");

                            tabela.Cell().PaddingTop(10).Text("TOTAL LÍQUIDO A RECEBER").Bold().FontSize(14);
                            tabela.Cell().PaddingTop(10).AlignRight().Text($"{dados.TotalLiquido:N2}").Bold().FontSize(14).FontColor(Colors.Green.Medium);
                        });

                        // Área do Pix
                        x.Item().PaddingTop(2, Unit.Centimetre).AlignCenter().Column(pix => 
                        {
                            pix.Item().Text("Gostou da ferramenta? Apoie com um Pix!").Italic();
                            pix.Item().Width(100).Image(qrCodeImage); // Insere a imagem do QR Code
                        });
                    });

                    // Rodapé
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return documento.GeneratePdf();
        }
    }
}