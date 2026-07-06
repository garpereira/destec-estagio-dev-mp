namespace DTOs
{
  public class CriarTransacaoRequest
  {
    public string Descricao {get; set;} = string.Empty;
    public decimal Valor {get; set;}
    public string Tipo {get; set;} = string.Empty;
    public int PessoaId {get; set;}
  }
}