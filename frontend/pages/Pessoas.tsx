import { criarPessoa, deletarPessoa, listarPessoas } from '../src/api'
import type { Pessoa } from '../src/types'
import { useEffect, useState } from 'react'
import { Trash2, UserPlus, Users } from "lucide-react";


function Pessoas(){

   /* Chama API para listar pessoas, utiliza o tipo Pessoa[] definido em types.ts como forma de tipagem
  a constante é definida e espera um array de objetos do tipo Pessoa, setPessoas é definida dentro de carregarPessoas que é responsavel 
  pela chamada da API no backend. */
  const [pessoas, setPessoas] = useState<Pessoa[]>([])
  const [nome, setNome] = useState('')
  const [idade, setIdade] = useState('')
  const [erro, setErro] = useState('')
  const [mensagem, setMensagem] = useState('')
  const [carregando, setCarregando] = useState(false)

  async function carregarPessoas() {
    try {
      setCarregando(true)
      const dados = await listarPessoas() // Chama request da API em api.ts la no backend no app.MapGet(/pessoas)
      setPessoas(dados)
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao carregar pessoas.')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    carregarPessoas()
  }, [])

  async function handleCriarPessoa(event: React.SubmitEvent<HTMLFormElement>) {
    event.preventDefault()
    setErro('')
    setMensagem('')

    try {
      await criarPessoa({
        nome,
        idade: Number(idade),
      })

      setNome('')
      setIdade('')
      setMensagem('Pessoa cadastrada com sucesso.')
      await carregarPessoas()
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao cadastrar pessoa.')
    }
  }

  async function handleDeletarPessoa(id: number) {
    setErro('')
    setMensagem('')

    try {
      await deletarPessoa(id)
      setMensagem('Pessoa removida com sucesso.')
      await carregarPessoas()
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao remover pessoa.')
    }
  }

  return (
    <section className="flex-1 bg-slate-100">
        <div className="mx-auto max-w-5xl px-8 py-10 ">
          <h1 className="font-display text-3xl font-bold text-blue-950">Cadastrar Pessoa</h1>
          <p className="mt-1.5 text-sm text-blue-950">Adicione moradores da casa. Tenha total controle sobre suas finanças</p>
        </div>
        <div className="m-8 mr-8 ml-8 md:mx-auto max-w-5xl rounded-2xl bg-white p-6 shadow">
          <div className="flex items-center text-xl p-2 gap-2">
            < UserPlus size={24} className="text-blue-950" />
            <h1 className="text-xl font-medium text-blue-950">Nova Pessoa</h1>
          </div>
          <div className="p-6 pt-0">
            <form className="mt-6 flex flex-col gap-4 md:flex-row md:items-end" onSubmit={handleCriarPessoa}>
              <div className="flex w-full flex-col md:flex-1">
                <label
                  htmlFor="nomePessoa"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Nome
                </label>

                <input
                  id="nomePessoa"
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                  placeholder="Ex.: Maria Silva"
                />
              </div>
              <div className="flex w-full flex-col md:w-32">
                <label
                  htmlFor="idadePessoa"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Idade
                </label>

                <input
                  type="number"
                  value={idade}
                  onChange={(e) => setIdade(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                />
              </div>
              <button
                  type="submit"
                  className="w-full items-end h-10 rounded-lg bg-blue-600 px-6 py-2 text-white transition hover:bg-blue-700 md:w-auto cursor-pointer"
              >
                Cadastrar
              </button>
            </form>
            {erro && (
              <div className="mt-4 rounded-lg bg-red-100 px-4 py-2 text-sm text-red-700">
                {erro}
              </div>
            )}
            {mensagem && (
              <div className="mt-4 rounded-lg bg-green-100 px-4 py-2 text-sm text-green-700">
                {mensagem}
              </div>
            )}
          </div>
        </div>
        <section className="mt-8 m-8 mr-8 ml-8 md:mx-auto max-w-5xl">
          <div className="flex place-items-center gap-2 mb-4">
            <Users size={24} className="text-slate-600" />
            <h2 className="text-lg font-semibold uppercase tracking-wide text-slate-600">
              Pessoas cadastradas ({pessoas.length})
            </h2>
          </div>

          <div className="space-y-3">
            {pessoas.length === 0 ? (
              <div className="rounded-xl border-2 border-dashed border-slate-300 p-8 text-center text-slate-500">
                Nenhuma pessoa cadastrada.
              </div>
            ) : (
              pessoas.map((pessoa) => (
                <div
                  key={pessoa.id}
                  className="flex items-center justify-between rounded-xl bg-white p-4 shadow-sm"
                >
                  {/* Esquerda */}
                  <div className="flex items-center gap-4">
                    <div className="flex h-12 w-12 items-center justify-center rounded-full bg-blue-100 text-lg font-bold text-blue-700">
                      {pessoa.nome.charAt(0).toUpperCase()}
                    </div>

                    <div>
                      <h3 className="font-medium text-slate-900">
                        {pessoa.nome}
                      </h3>

                      <p className="text-sm text-slate-500">
                        {pessoa.idade} anos
                      </p>
                    </div>
                  </div>

                  {/* Direita */}
                  <div className="flex items-center gap-4">
                    <span
                      className={`rounded-full px-3 py-1 text-sm font-medium ${
                        pessoa.maioridade
                          ? "bg-green-100 text-green-700"
                          : "bg-yellow-100 text-yellow-700"
                      }`}
                    >
                      {pessoa.maioridade
                        ? "Maior de idade"
                        : "Menor de idade"}
                    </span>

                    <button
                      onClick={() => handleDeletarPessoa(pessoa.id)}
                      className="rounded-md p-2 text-slate-500 transition hover:bg-red-50 hover:text-red-600 cursor-pointer"
                    >
                      <Trash2 size={20}/>
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        </section>
      </section>
  )
}

export default Pessoas;