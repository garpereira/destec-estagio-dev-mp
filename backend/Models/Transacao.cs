namespace Models
{
  public class Transacao
  {
    public int Id {get; set;}
    public string Descricao {get; set;} = string.Empty;
    public decimal Valor {get; set;} = default;
    public string Tipo {get; set;} = string.Empty;

    //FK
    public int PessoaId{get; set; }
    public Pessoa Pessoa{ get; set; } = null!;
  }

}