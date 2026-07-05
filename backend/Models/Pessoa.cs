namespace Models
{
  public class Pessoa
  {
    public int Id {get; set;}
    public string Nome {get; set;}
    public int Idade {get; set;}
    public bool Maioridade {get => Idade >= 18;}

    // Relacionamento 1:N
    public List<Transacao> Transacoes {get; set;} = new ();
  }
}
