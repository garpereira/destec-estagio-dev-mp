using Data;
using Microsoft.EntityFrameworkCore;
using Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=desafio.db"));
var app = builder.Build();


app.MapGet("/", () => "API rodando!");
app.MapGet("/pessoas", async (AppDbContext db) => await db.Pessoas.ToListAsync());
app.MapPost("/pessoas", async (Pessoa pessoa, AppDbContext db) => { 
    db.Pessoas.Add(pessoa);
    await db.SaveChangesAsync();
    return Results.Created($"/pessoas/{pessoa.Id}", pessoa);

});
app.Run();