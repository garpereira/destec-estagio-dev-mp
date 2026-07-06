using Data;
using Microsoft.EntityFrameworkCore;
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
        return Results.NotFound("Pessoa não encontrada.");
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

app.MapGet("/totais", async (AppDbContext db) =>
{
   // Queremos listar todas as pessoas cadastradas, exibir total de receitas, despesas, saldo
   // Exibir total geral

   /* A logica para somar os valores de cada pessoa, foi pesquisar no banco de dados
   tabela Pessoas e Transacoes utilizando LINQ, que é como se estivessemos fazendo uma query
   dentro do SQL. Portanto, busco as informacoes de cada pessoa, enquanto 
   faço o somatório dos valores de receita e despesa onde seus tipos são equivalentes.
   */
    var pessoas = await db.Pessoas
        .AsNoTracking()
        .Select(pessoa => new
        {
            pessoa.Id,
            pessoa.Nome,
            pessoa.Idade,
            pessoa.Maioridade,

            TotalReceitas = db.Transacoes
                .Where(transacao => transacao.Tipo == "receita" && transacao.PessoaId == pessoa.Id)
                .Sum(transacao => transacao.Valor),
            
            TotalDespesas = pessoa.Transacoes
            .Where(transacao => transacao.Tipo == "despesa" && transacao.PessoaId == pessoa.Id)
            .Sum(transacao => transacao.Valor)
        })
        .ToListAsync();

    // Aqui eu só quis dividir um pouco a tarefa das variaveis para deixar mais legível
    var pessoasComSaldo = pessoas
        .Select(pessoa => new
        {
           pessoa.Id,
           pessoa.Nome,
           pessoa.Idade,
           pessoa.TotalReceitas,
           pessoa.TotalDespesas,
           Saldo = pessoa.TotalReceitas - pessoa.TotalDespesas 
        })
        .ToList();

    var totalGeralReceitas = pessoas.Sum(pessoa => pessoa.TotalReceitas);
    var totalGeralDespesas = pessoas.Sum(pessoa => pessoa.TotalDespesas);

    var response = new
    {
        Pessoas = pessoas,
        TotalGeral = new
        {
            TotalReceitas = totalGeralReceitas,
            TotalDespesas = totalGeralDespesas,
            SaldoLiquido = totalGeralReceitas - totalGeralDespesas
        }
    };

    return Results.Ok(response);
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

    var response = new
    {
        pessoa.Id,
        pessoa.Nome,
        pessoa.Idade
    };
    return Results.Created($"/pessoas/{response.Id}", response);
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

    var response = new
    {
        transacao.Id,
        transacao.Descricao,
        transacao.Valor,
        transacao.Tipo,
        transacao.PessoaId,
        Pessoa = new
        {
            pessoa.Id,
            pessoa.Nome,
            pessoa.Idade
        }
    };

    return Results.Created($"/transacoes/{response.Id}", response);
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