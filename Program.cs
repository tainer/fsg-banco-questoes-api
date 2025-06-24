using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BancoQuestoes.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços necessários para Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// Configurar Entity Framework
builder.Services.AddDbContext<BancoQuestoesContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=banco_questoes.db"));

// Configurar Swagger/OpenAPI  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banco de Questões API",
        Version = "v1",
        Description = "API para gerenciamento de banco de questões e composição de provas",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "dev@bancoquestoes.com"
        }
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configurar CORS para permitir qualquer origem
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banco de Questões API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableValidator();
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Mapear controllers
app.MapControllers();

// Criar banco de dados se não existir
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BancoQuestoesContext>();
    context.Database.EnsureCreated();
}

app.Run();