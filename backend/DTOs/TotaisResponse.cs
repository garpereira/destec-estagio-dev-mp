namespace DTOs
{
  public class TotaisResponse
  {
    public List<TotalPessoaResponse> Pessoas {get; set;} = new();
    public ResumoGeralResponse TotalGeral {get; set;} = new();
  }
}