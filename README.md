# 💼 Calculadora Trabalhista Pro

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)

Aplicação web desenvolvida em **ASP.NET Core MVC** para realização de cálculos rescisórios trabalhistas com alta precisão jurídica, desenhada para evitar erros comuns de contagem de avos e tributação.

O sistema diferencia automaticamente as regras para **Pedido de Demissão**, **Dispensa Sem Justa Causa** e **Justa Causa**, aplicando as normas vigentes da CLT (Consolidação das Leis do Trabalho).


---

## 🎓 Guia para Iniciantes: Como Baixar e Rodar

Se você está começando agora e quer testar este projeto no seu computador, siga este passo a passo detalhado:

### 1. Baixando as Ferramentas (Grátis)
Você vai precisar do **Visual Studio Community**, que é o programa oficial da Microsoft para editar códigos em C#.

1.  Acesse o site oficial: [Download Visual Studio](https://visualstudio.microsoft.com/pt-br/vs/community/)
2.  Baixe a versão **Community 2022** (é gratuita).
3.  **Importante na Instalação:** Quando o instalador abrir, ele vai pedir para você marcar o que deseja instalar. Marque a caixinha:
    * ✅ **Desenvolvimento para ASP.NET e web**
4.  Clique em instalar e aguarde (pode demorar um pouquinho).

### 2. Abrindo o Projeto
1.  Nesta página do GitHub, clique no botão verde **<> Code** e depois em **Download ZIP**.
2.  Extraia (descompacte) a pasta no seu computador.
3.  Abra a pasta e procure um arquivo com extensão **.sln** (Solução) ou clique com o botão direito na pasta vazia e escolha "Abrir com Visual Studio".

### 3. Rodando a Calculadora
1.  Com o Visual Studio aberto, procure na parte de cima um botão verde com o símbolo de "Play" (▶) escrito **CalculadoraTrabalhista** ou **http**.
2.  Clique nele! O Visual Studio vai preparar tudo e abrirá o navegador automaticamente com a calculadora funcionando.


---

## 📸 Preview

![Preview do Sistema](img/resultado.png)

---

## 🚀 Diferencial Técnico: A "Regra dos 15 Dias"

O grande desafio de calculadoras trabalhistas é a contagem correta de meses trabalhados (avos) para 13º e Férias. Calculadoras simples apenas subtraem datas, gerando prejuízo ao trabalhador.

**Este projeto implementa a Lei 4.090/62 e Art. 130 da CLT:**
> "A fração igual ou superior a 15 (quinze) dias de trabalho será havida como mês integral."

### 💡 Exemplo Prático Implementado:
Se um funcionário é admitido em **15/01** e sai em **31/12**:
1.  **Cálculo Ingênuo:** Consideraria apenas 11 meses (fevereiro a dezembro).
2.  **Cálculo Deste Sistema:**
    * Janeiro: 17 dias trabalhados (15 a 31) -> **Conta como 1 mês**.
    * Fev a Dez: 11 meses.
    * **Resultado:** 12/12 avos (Pagamento Integral).

**Snippet da Lógica (`CalculoService.cs`):**
```csharp
// Algoritmo de precisão dia-a-dia
while (cursor <= dtDemissao)
{
    // ...logica de datas...
    int diasNoMes = (int)(fimMes - inicioMes).TotalDays + 1;
    
    // REGRA DE OURO: Garante o direito se trabalhou >= 15 dias
    if (diasNoMes >= 15) avosTrabalhados++;
    
    cursor = cursor.AddMonths(1);
}

