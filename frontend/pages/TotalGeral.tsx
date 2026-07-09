import {Users, Wallet2, ArrowDownCircle, ArrowUpCircle } from "lucide-react";
import type { Totais, Transacao } from "../src/types";

import { useEffect, useState } from 'react';

import { buscarTotais, listarTransacoes} from '../src/api'

function TotalGeral(){

  const [totais, setTotais] = useState<Totais | null>(null)
  const [transacao, setTransacao] = useState<Transacao[] | null>([])
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  const formatoMoeda = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });

  async function handleBuscarTotais(){
    try {
      setCarregando(true)
      const dados = await buscarTotais()
      setTotais(dados)
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao buscar totais.')
    } finally {
      setCarregando(false)
    }
  }

  useEffect(() => {
    handleBuscarTotais()
  }, [])

async function handleListarTransacoes(){
    try {
      setCarregando(true)
      const dados = await listarTransacoes()
      setTransacao(dados)
    } catch (error) {
      setErro(error instanceof Error ? error.message : 'Erro ao buscar transações.')
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
          <h1 className="font-display text-3xl font-bold text-blue-950">Totais</h1>
          <p className="mt-1.5 text-sm text-blue-950">Resumo financeiro geral e por pessoa da casa.</p>
        </div>
        <div className="grid grid-cols-3 mx-auto max-w-5xl px-8 md:px-1 gap-4">
          <div className="grid grid-rows-3 items-center text-xl bg-white p-6 shadow rounded-2xl">
            <div className="grid h-10 w-10 rounded-xl bg-green-200/40 place-items-center">
              <ArrowUpCircle size={20} className="text-green-600" />
            </div>
            <h1 className="text-xl text-green-950/70">Receitas</h1>
            <h1 className="text-xl font-medium text-green-600">{formatoMoeda.format(totais?.totalGeral.totalReceitas || 0)}</h1>
          </div>
          <div className="grid grid-rows-3 items-center text-xl gap-2 bg-white p-6 shadow rounded-2xl">
            <div className="grid h-10 w-10 rounded-xl bg-red-200/40 place-items-center">
              <ArrowDownCircle size={20} className="text-red-600" />
            </div>
            <h1 className="text-xl  text-red-950/70">Despesas</h1>
            <h1 className="text-xl font-medium text-red-600">{formatoMoeda.format(totais?.totalGeral.totalDespesas || 0)}</h1>
          </div>
          <div className="grid grid-rows-3 items-center text-xl gap-2 bg-white p-6 shadow rounded-2xl">
            <div className="grid h-10 w-10 rounded-xl bg-blue-200/40 place-items-center">
              <Wallet2 size={20} className="text-blue-950" />
            </div>
            <h1 className="text-xl  text-blue-950/70">Saldo</h1>
            <h1 className="text-xl font-medium text-blue-950">{formatoMoeda.format(totais?.totalGeral.saldoLiquido || 0)}</h1>
          </div>
        </div>
        <section className="mt-8 m-8 mr-8 ml-8 md:mx-auto max-w-5xl">
          {erro && (
            <div className="mb-4 rounded-lg bg-red-100 px-4 py-2 text-sm text-red-700">
              {erro}
            </div>
          )}
          <div className="flex place-items-center gap-2 mb-4">
            <Users size={24} className="text-slate-600" />
            <h2 className="text-lg font-semibold uppercase tracking-wide text-slate-600">
              Por pessoa
            </h2>
          </div>

          <div className="overflow-hidden rounded-2xl bg-white shadow">
              {/* Se não tem transações, logo não há dados para exibir */}
              {carregando ? (
                <div className="rounded-xl border-2 border-dashed border-slate-300 p-8 text-center text-slate-500">
                  Carregando totais...
                </div>
              ) : transacao?.length === 0 ? (
                <div className="rounded-xl border-2 border-dashed border-slate-300 p-8 text-center text-slate-500">
                  Nenhuma transação cadastrada.
                </div>
              ) : (
                
                <table className="w-full border-collapse">
                  <thead className="bg-slate-100">
                    <tr>
                      <th className="p-4 text-left text-sm font-semibold text-slate-600">
                        Pessoa
                      </th>
                      <th className="p-4 text-left text-sm font-semibold text-slate-600">
                        Receitas
                      </th>
                      <th className="p-4 text-left text-sm font-semibold text-slate-600">
                        Despesas
                      </th>
                      <th className="p-4 text-left text-sm font-semibold text-slate-600">
                        Saldo
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {totais?.pessoas.map((pessoa) => (
                      <tr key={pessoa.id} className="border-t border-slate-100">
                        <td className="p-4 text-sm text-yellow-600">{pessoa.nome}</td>
                        <td className="p-4 text-sm text-green-600">{formatoMoeda.format(pessoa.totalReceitas)}</td>
                        <td className="p-4 text-sm text-red-600">{formatoMoeda.format(pessoa.totalDespesas)}</td>
                        <td className="p-4 text-sm text-blue-900">{formatoMoeda.format(pessoa.saldo)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                )}
              </div>
        </section>
      </section>
  )
}

export default TotalGeral;
