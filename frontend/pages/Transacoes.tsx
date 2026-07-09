import { Wallet2, ArrowDownCircle, ArrowUpCircle } from "lucide-react";
import type { Transacao, Pessoa } from "../src/types";

import { useEffect, useState } from 'react';

import { criarTransacao, listarTransacoes, listarPessoas } from '../src/api'
import { Users } from "lucide-react";

function Transacoes(){

  const [pessoas, setPessoas] = useState<Pessoa[]>([])
  const [pessoaId, setPessoaId] = useState<number | "">("")
  const [pessoaSelecionada, setPessoaSelecionada] = useState<Pessoa | null>(null)
  const [descricao, setDescricao] = useState('')
  const [tipo, setTipo] = useState<'receita' | 'despesa'>('despesa')
  const [valor, setValor] = useState('')
  const [transacoes, setTransacoes] = useState<Transacao[]>([])
  const [erro, setErro] = useState('')
  const [mensagem, setMensagem] = useState('')
  const [carregando, setCarregando] = useState(false)

  const formatoMoeda = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });

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

  async function handleCriarTransacao(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErro('')
    setMensagem('')

    try {
      await criarTransacao({
        pessoaId: Number(pessoaId),
        descricao,
        tipo,
        valor: Number(valor),
      })

      setPessoaId("")
      setDescricao('')
      setTipo('receita')
      setValor('')
      setMensagem('Transação cadastrada com sucesso.')
      await handleListarTransacoes()
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao cadastrar transação.')
    }
  }

  async function handleListarTransacoes(){
    try {
      setCarregando(true)
      const dados = await listarTransacoes()
      setTransacoes(dados)
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao listar transações.')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    handleListarTransacoes()
  }, [])

  return (
    <section className="flex-1 bg-slate-100">
        <div className="mx-auto max-w-5xl px-8 py-10 ">
          <h1 className="font-display text-3xl font-bold text-blue-950">Cadastrar Transação</h1>
          <p className="mt-1.5 text-sm text-blue-950">Adicione transações financeiras. Menores de idade só podem registrar despesas</p>
        </div>
        <div className="m-8 mr-8 ml-8 md:mx-auto max-w-5xl rounded-2xl bg-white p-6 shadow">
          <div className="flex items-center text-xl p-2 gap-2">
            <Wallet2 size={24} className="text-blue-950" />
            <h1 className="text-xl font-medium text-blue-950">Nova Transação</h1>
          </div>
          <div className="p-6 pt-0">
            <form className="mt-6 grid grid-cols-1 gap-4 md:grid-cols-2 md:flex-row md:items-end" onSubmit={handleCriarTransacao}>
              <div className="flex w-full flex-col md:flex-1">
                <label
                  htmlFor="pessoa"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Pessoa
                </label>

                <select
                  id="pessoa"
                  value={pessoaId}
                  onChange={(e) => {
                    setPessoaId(Number(e.target.value))
                    setPessoaSelecionada(pessoas.find((p) => p.id === Number(e.target.value)) || null)
                  }}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                >
                  <option value="">Selecione uma pessoa</option>
                  {pessoas.map((pessoa) => (
                    <option key={pessoa.id} value={pessoa.id}>
                      {pessoa.nome}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex w-full flex-col md:flex-1">
                <label
                  htmlFor="tipoTransacao"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Tipo
                </label>
                <select
                  id="tipoTransacao"
                  value={tipo}
                  onChange={(e) => setTipo(e.target.value as 'receita' | 'despesa')}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                >
                  {pessoaSelecionada?.maioridade ? (
                    <>
                      <option value="receita">Receita</option>
                      <option value="despesa">Despesa</option>
                    </>
                  ) :  <option value="despesa">Despesa</option>}
                </select>
              </div>
              <div className="flex w-full flex-col md:flex-1">
                <label
                  htmlFor="descricaoTransacao"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Descrição
                </label>
                <input
                  id="descricaoTransacao"
                  value={descricao}
                  onChange={(e) => setDescricao(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                  placeholder="Ex.: Compra de supermercado"
                >
                </input>
              </div>
              <div className="flex w-full flex-col md:flex-1">
                <label
                  htmlFor="valorTransacao"
                  className="mb-2 text-sm font-medium text-slate-700"
                >
                  Valor
                </label>

                <input
                  id="valorTransacao"
                  type="number"
                  min={0}
                  step="0.01"
                  value={valor}
                  onChange={(e) => setValor(e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                  placeholder="0,00"
                />
              </div>
              <button
                  type="submit"
                  className="md:justify-self-start items-end h-10 rounded-lg bg-blue-600 px-6 py-2 text-white text-sm font-semibold transition hover:bg-blue-700 cursor-pointer"
              >
                Registrar transação
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
              Transações cadastradas ({transacoes.length})
            </h2>
          </div>

          <div className="space-y-3">
            {carregando ? (
              <div className="rounded-xl border-2 border-dashed border-slate-300 p-8 text-center text-slate-500">
                Carregando transações...
              </div>
            ) : transacoes.length === 0 ? (
              <div className="rounded-xl border-2 border-dashed border-slate-300 p-8 text-center text-slate-500">
                Nenhuma transação cadastrada.
              </div>
            ) : (
              transacoes.map((transacao) => (
                <div
                  key={transacao.pessoaId}
                  className="flex items-center justify-between rounded-xl bg-white p-4 shadow-sm"
                >
                  {/* Esquerda */}
                  <div className="flex items-center gap-4">
                    <div className="flex items-center justify-center rounded-full bg-blue-100 text-lg font-bold text-blue-700">
                      {transacao.tipo == 'receita' ? (
                        <ArrowUpCircle size={30} className="text-green-500 bg-white"/>
                      ) : (
                        <ArrowDownCircle size={30} className="text-red-500 bg-white"/>
                      )}
                    </div>

                    <div>
                      <h3 className="font-medium text-slate-900">
                        {transacao.descricao}
                      </h3>

                      <p className="text-sm text-slate-500">
                       {transacao.tipo.toUpperCase()} . {transacao.pessoa.nome}
                      </p>
        
                    </div>
                    
                  </div>

                  {/* Direita */}
                  <div className="flex items-left gap-4">
                    <span
                      className={`rounded-full px-3 py-1 text-md font-semibold${
                        transacao.tipo == 'receita'
                          ? " text-green-500"
                          : " text-red-500"
                      }`}
                    >
                      {transacao.tipo == 'receita'
                        ? formatoMoeda.format(transacao.valor)
                        : `- ${formatoMoeda.format(transacao.valor)}`}
                    </span>
                  </div>
                </div>
              ))
            )}
          </div>
        </section>
      </section>
  )
}

export default Transacoes;
