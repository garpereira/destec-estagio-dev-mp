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
        .Select(pessoa => new
        {
            pessoa.Id,
            pessoa.Nome,
            pessoa.Idade
        })
        .ToListAsync();
    
    return Results.Ok(pessoas);
});

app.MapGet("/pessoas/{id}", async (int id, AppDbContext db) =>
{
    var pessoa = await db.Pessoas
        .AsNoTracking()
        .Include(pessoa => pessoa.Transacoes)
        .Where(pessoa => pessoa.Id == id)
        .Select(pessoa => new
        {   
            pessoa.Nome,
            pessoa.Idade,
            pessoa.Maioridade,
            Transacao = pessoa.Transacoes.Select(transacao => new
            {
              transacao.Id,
              transacao.Descricao,
              transacao.Tipo,
              transacao.Valor,
            })
        })
        .FirstOrDefaultAsync();
    
    if (pessoa is null) // null se elemento nao for encontrado
    {
        return Results.NotFound("Pessoa não encontrada!");
    }
    
    return Results.Ok(pessoa);
});

app.MapGet("/transacoes", async (AppDbContext db) =>
{
    var transacao = await db.Transacoes
        .AsNoTracking()
        // havia feito anteriormente dessa forma a inserção de dependencia de outra entidade
        // mas ocasionou erro ciclico, porque pessoas tambem estava fazendo essa chamada
        // para transacoes. Então pesquisei e vi o formato DTO é mais indicado para retornos
        // em APIs 
        //.Include(transacao => transacao.Pessoa)
        .Include(transacao => transacao.Pessoa)
        .Select(transacao => new
        {
            transacao.Id,
            transacao.Descricao,
            transacao.Valor,
            transacao.Tipo,
            transacao.PessoaId,
            Pessoa = new
            {
                transacao.Pessoa.Id,
                transacao.Pessoa.Nome,
                transacao.Pessoa.Idade
            }
        })
        .ToListAsync();
    
    return Results.Ok(transacao);
});

app.MapGet("/transacoes/{id}", async (int id, AppDbContext db) =>
{
    var transacao = await db.Transacoes
    .AsNoTracking()
    // Igualemnte a /transacoes, tambem ocasionava serialização ciclica
    // .Include(transacao => transacao.Pessoa)
    // .FirstOrDefaultAsync(transacao => transacao.Id == id);
    .Include(transacao => transacao.Pessoa)
    .Where(transacao => transacao.Id == id)
    .Select(transacao => new
    {
        transacao.Id,
        transacao.Descricao,
        transacao.Valor,
        transacao.Tipo,
        transacao.PessoaId,
        Pessoa = new
        {
            transacao.Pessoa.Id,
            transacao.Pessoa.Nome,
            transacao.Pessoa.Idade,
            transacao.Pessoa.Maioridade
        }
    })
    .FirstOrDefaultAsync();

    if (transacao is null)
    {
        return Results.NotFound("Transação não encontrada.");
    }

    return Results.Ok(transacao);
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

app.MapPost("/transacoes", async (Transacao transacao, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(transacao.Descricao.Trim()))
    {
        return Results.BadRequest("A descrição da transação é obrigatória.");
    }
    if (transacao.Valor <= 0)
    {
        return Results.BadRequest("O valor da transação deve ser maior que zero.");
    }
    if (string.IsNullOrEmpty(transacao.Tipo))
    {
        return Results.BadRequest("O tipo da transação deve ser 'receita' ou 'despesa'.");
    }

    // Verificar se PessoaId esta cadastrada pelo Id inserido na transacao
    var pessoa = await db.Pessoas
        .FirstOrDefaultAsync(pessoa => pessoa.Id == transacao.PessoaId);
    
    if (pessoa is null)
    {
        return Results.BadRequest("A pessoa informada não existe.");
    }

    string tipo = transacao.Tipo.Trim().ToLower();

    if (!tipo.Equals("receita") && !tipo.Equals("despesa"))
    {
        return Results.BadRequest("O tipo da transação deve ser 'receita' ou 'despesa'.");
    }

    // Se a pessoa for menor de idade, então o tipo tem que ser Despesa
    if (!pessoa.Maioridade && !tipo.Equals("despesa"))
    {
        return Results.BadRequest("Menores de idade podem inserir apenas despesas.");
    }

    transacao.Tipo = tipo;
    db.Transacoes.Add(transacao);
    await db.SaveChangesAsync();

    return Results.Created($"/transacoes/{transacao.Id}", transacao);
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