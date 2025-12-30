using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using calculotrabalista.Models;
using calculotrabalista.Services;
using calculotrabalista.ViewModels;

namespace calculotrabalista.Controllers;

public class HomeController : Controller
{
    private readonly CalculoService _calculoService;
    private readonly PdfService _pdfService;

    public HomeController(CalculoService calculoService, PdfService pdfService)
    {
        _calculoService = calculoService;
        _pdfService = pdfService;
    }

    public IActionResult Index()
    {
        // Inicia com datas preenchidas para evitar "Ano 0001"
        var modelo = new DadosTrabalhistaVM
        {
            DataAdmissao = DateTime.Now.AddYears(-1),
            DataDemissao = DateTime.Now
        };
        return View(modelo);
    }

    [HttpPost]
    public IActionResult Calcular(DadosTrabalhistaVM dados)
    {
        // 1. Limpeza do valor R$ (Tira o R$, tira pontos, troca vírgula por ponto decimal)
        if (!string.IsNullOrEmpty(dados.SalarioBrutoTexto))
        {
            try 
            {
                string valorLimpo = dados.SalarioBrutoTexto
                    .Replace("R$", "")
                    .Replace(" ", "")
                    .Replace(".", ""); // Tira ponto de milhar (1.000 -> 1000)
                
                // O C# usa cultura local, então o decimal.Parse entende a vírgula
                dados.SalarioBruto = decimal.Parse(valorLimpo);
            }
            catch
            {
                ModelState.AddModelError("SalarioBrutoTexto", "Valor inválido");
            }
        }

        if (!ModelState.IsValid)
        {
            return View("Index", dados);
        }

        // 2. Realiza o cálculo
        var resultado = _calculoService.Calcular(dados);
        
        // 3. Marca que o cálculo foi feito para mostrar a tabela
        resultado.CalculoRealizado = true; 
        resultado.SalarioBrutoTexto = dados.SalarioBrutoTexto; // Mantém o texto formatado

        // 4. GERA O CÓDIGO PIX (Payload Oficial)
        // ATENÇÃO: Troque "seuemail@exemplo.com" pela sua chave PIX real para funcionar
        string chavePix = "c6482b29-fa1f-4f61-ba4a-f414bf306e26"; 
        string nomeBeneficiario = "Calculadora Trabalhista";
        string cidadeBeneficiario = "Sao Paulo";

        resultado.PayloadPix = PixHelper.GerarPayload(chavePix, nomeBeneficiario, cidadeBeneficiario);

        return View("Index", resultado);
    }

    [HttpPost]
    public IActionResult BaixarPdf(DadosTrabalhistaVM dados)
    {
        // Repete a limpeza para garantir
         if (!string.IsNullOrEmpty(dados.SalarioBrutoTexto))
        {
            string valorLimpo = dados.SalarioBrutoTexto.Replace("R$", "").Replace(" ", "").Replace(".", "");
            if(decimal.TryParse(valorLimpo, out decimal valor)) dados.SalarioBruto = valor;
        }

        var resultado = _calculoService.Calcular(dados);
        
        // Gera o PDF
        var arquivoPdf = _pdfService.GerarPdfRescisao(resultado);
        
        return File(arquivoPdf, "application/pdf", $"rescisao_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}