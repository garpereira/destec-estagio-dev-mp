namespace DTOs
{
  public class PessoaResponse
  {
    public int Id {get; set;}
    public string Nome {get; set;} = string.Empty;
    public int Idade {get; set;}
    public bool Maioridade {get; set;}
  }
}