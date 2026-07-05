namespace Models
{
  public class Pessoa
  {
    public int Id {get; set;}
    public string Nome {get; set;} = string.Empty;
    public int Idade {get; set;} = default;
    public bool Maioridade {get => Idade >= 18;}

    // Relacionamento 1:N
    public List<Transacao> Transacoes {get; set;} = new ();
  }
}
