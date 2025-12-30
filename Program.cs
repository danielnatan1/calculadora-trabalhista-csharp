var builder = WebApplication.CreateBuilder(args);

// --- Adicione estas linhas logo abaixo do 'CreateBuilder' ---

// Registra o serviço de cálculo para usarmos no site
builder.Services.AddScoped<calculotrabalista.Services.CalculoService>();
builder.Services.AddScoped<calculotrabalista.Services.PdfService>();

// Configura a licença do QuestPDF (Gratuita para comunidade)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// -----------------------------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
