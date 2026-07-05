namespace Models
{
  public class Transacao
  {
    public int Id {get; set;}
    public string Descricao {get; set;}
    public decimal Valor {get; set;}
    public string Tipo {get; set;}

    //FK
    public int PessoaId{get; set; }
    public Pessoa Pessoa{ get; set; }
  }

}