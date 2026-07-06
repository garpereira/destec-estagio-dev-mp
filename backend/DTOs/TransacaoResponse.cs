namespace DTOs
{
  public class TransacaoResponse
  {
    public int Id {get; set;}
    public string Descricao {get; set;} = string.Empty;
    public decimal Valor {get; set;}
    public string Tipo {get; set;} = string.Empty;
    public int PessoaId {get; set;}
    public PessoaResponse Pessoa {get; set;} = null!;
  }
}