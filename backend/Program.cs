using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=desafio.db"));
var app = builder.Build();

// GETS
app.MapGet("/", () => "API rodando!");
app.MapGet("/pessoas", async (AppDbContext db) =>
{
    var pessoas = await db.Pessoas
        .AsNoTracking()
        .ToListAsync();
    
    return Results.Ok(pessoas);
});

app.MapGet("/pessoas/{id}", async (int id, AppDbContext db) =>
{
    var pessoa = await db.Pessoas
        .AsNoTracking()
        .Include(pessoa => pessoa.Transacoes)
        .FirstOrDefaultAsync(pessoa => pessoa.Id == id);
    
    if (pessoa is null) // null se elemento nao for encontrado
    {
        return Results.NotFound("Pessoa não encontrada!");
    }
    
    return Results.Ok(pessoa);
});

// POSTS
app.MapPost("/pessoas", async (Pessoa pessoa, AppDbContext db) => 
{
    if (string.IsNullOrEmpty(pessoa.Nome))
    {
        return Results.BadRequest("O campo nome é obrigatório.");
    }
    if (int.IsNegative(pessoa.Idade))
    {
        return Results.BadRequest("Idade não pode ser negativa (a nao ser que você seja um viajante no tempo).");
    }
    

    db.Pessoas.Add(pessoa);
    await db.SaveChangesAsync();
    return Results.Created($"/pessoas/{pessoa.Id}", pessoa);
});

// DELETES
app.MapDelete("/pessoas/{id}", async (int id, AppDbContext db) =>
{
    var pessoa = await db.Pessoas
        .FindAsync(id);
    
    if (pessoa is null)
    {
        return Results.NotFound("Pessoa não encontrada");
    }

    db.Pessoas.Remove(pessoa);
    await db.SaveChangesAsync();

    return Results.Accepted("Deletado com sucesso");
});
app.Run();