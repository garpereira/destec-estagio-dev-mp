using Data;
using DTOs;
using Microsoft.EntityFrameworkCore;
using Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=desafio.db"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("Frontend");

Dictionary<string, string> listRoutes = new Dictionary<string, string> {
    ["/pessoas"] = "Listar todas as pessoas cadastradas",
    ["/pessoas/{id}"] = "Listar uma pessoa específica pelo ID, incluindo suas transações",
    ["/transacoes"] = "Listar todas as transações cadastradas",
    ["/transacoes/{id}"] = "Listar uma transação específica pelo ID, incluindo a pessoa associada",
    ["/totais"] = "Listar todas as pessoas com seus totais de receitas, despesas e saldo, além do total geral",
    ["/pessoas (POST)"] = "Cadastrar uma nova pessoa",
    ["/transacoes (POST)"] = "Cadastrar uma nova transação",
    ["/pessoas/{id} (DELETE)"] = "Deletar uma pessoa pelo ID"
};

// GETS
app.MapGet("/", () => $"API rodando!\n\nRotas disponíveis:\n{string.Join("\n", listRoutes.Select(route => $"{route.Key} - {route.Value}"))}");
app.MapGet("/pessoas", async (AppDbContext db) =>
{
    var pessoas = await db.Pessoas
        .AsNoTracking()
        .Select(pessoa => new
        {
            pessoa.Id,
            pessoa.Nome,
            pessoa.Idade,
            pessoa.Maioridade
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
    var pessoasResponse = pessoas
        .Select(pessoa => new TotalPessoaResponse
        {
           Id = pessoa.Id,
           Nome = pessoa.Nome,
           Idade = pessoa.Idade,
           TotalReceitas = pessoa.TotalReceitas,
           TotalDespesas = pessoa.TotalDespesas,
           Saldo = pessoa.TotalReceitas - pessoa.TotalDespesas
        })
        .ToList();

    var totalGeralReceitas = pessoas.Sum(pessoa => pessoa.TotalReceitas);
    var totalGeralDespesas = pessoas.Sum(pessoa => pessoa.TotalDespesas);

    var response = new TotaisResponse
    {
        Pessoas = pessoasResponse,
        TotalGeral = new ResumoGeralResponse
        {
            TotalReceitas = totalGeralReceitas,
            TotalDespesas = totalGeralDespesas,
            SaldoLiquido = totalGeralReceitas - totalGeralDespesas
        }
    };

    return Results.Ok(response);
});

// POSTS
app.MapPost("/pessoas", async (CriarpessoaRequest request, AppDbContext db) => 
{
    if (string.IsNullOrEmpty(request.Nome))
    {
        return Results.BadRequest("O campo nome é obrigatório.");
    }
    if (int.IsNegative(request.Idade))
    {
        return Results.BadRequest("Idade não pode ser negativa (a nao ser que você seja um viajante no tempo).");
    }
    if (request.Idade == 0)
    {
        return Results.BadRequest("Idade não pode ser zero.");
    }
    
    var pessoa = new Pessoa
    {
        Nome = request.Nome.Trim(),
        Idade = request.Idade
    };

    db.Pessoas.Add(pessoa);
    await db.SaveChangesAsync();

    var response = new PessoaResponse
    {
        Id = pessoa.Id,
        Nome = pessoa.Nome,
        Idade = pessoa.Idade,
        Maioridade = pessoa.Maioridade
    };
    return Results.Created($"/pessoas/{response.Id}", response);
});

app.MapPost("/transacoes", async (CriarTransacaoRequest request, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(request.Descricao.Trim()))
    {
        return Results.BadRequest("A descrição da transação é obrigatória.");
    }
    if (request.Valor <= 0)
    {
        return Results.BadRequest("O valor da transação deve ser maior que zero.");
    }
    if (string.IsNullOrEmpty(request.Tipo))
    {
        return Results.BadRequest("O tipo da transação deve ser 'receita' ou 'despesa'.");
    }

    // Verificar se PessoaId esta cadastrada pelo Id inserido na transacao
    var pessoa = await db.Pessoas.FindAsync(request.PessoaId);
        
    string tipo = request.Tipo.Trim().ToLower();
    
    if (pessoa is null)
    {
        return Results.BadRequest("A pessoa informada não existe.");
    }

    if (!tipo.Equals("receita") && !tipo.Equals("despesa"))
    {
        return Results.BadRequest("O tipo da transação deve ser 'receita' ou 'despesa'.");
    }

    // Se a pessoa for menor de idade, então o tipo tem que ser Despesa
    if (!pessoa.Maioridade && !tipo.Equals("despesa"))
    {
        return Results.BadRequest("Menores de idade podem inserir apenas despesas.");
    }

    var transacao = new Transacao
    {
        Descricao = request.Descricao.Trim(),
        Valor = request.Valor,
        Tipo = tipo,
        PessoaId = request.PessoaId
    };

    db.Transacoes.Add(transacao);
    await db.SaveChangesAsync();

    var response = new TransacaoResponse
    {
        Id = transacao.Id,
        Descricao = transacao.Descricao,
        Valor = transacao.Valor,
        Tipo = transacao.Tipo,
        PessoaId = transacao.PessoaId,
        Pessoa = new PessoaResponse
        {
            Id = pessoa.Id,
            Nome = pessoa.Nome,
            Idade = pessoa.Idade,
            Maioridade = pessoa.Maioridade
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

    return Results.NoContent();
});
app.Run();