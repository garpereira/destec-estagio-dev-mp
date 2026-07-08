/*
Aqui criamos os tipos equivalentes ao que criamos no backend como respostas das requisições, pois precisamos
receber a estrutura das APIs do backend de forma correta.
*/

export type Pessoa = {
  id: number
  nome: string
  idade: number
  maioridade: boolean
}

export type CriarPessoaRequest = {
  nome: string
  idade: number
}

export type Transacao = {
  id: number
  descricao: string
  valor: number
  tipo: 'receita' | 'despesa'
  pessoaId: number
  pessoa: Pessoa
}

export type CriarTransacaoRequest = {
  descricao: string
  valor: number
  tipo: 'receita' | 'despesa'
  pessoaId: number
}

export type TotalPessoa = {
  id: number
  nome: string
  idade: number
  totalReceitas: number
  totalDespesas: number
  saldo: number
}

export type ResumoGeral = {
  totalReceitas: number
  totalDespesas: number
  saldoLiquido: number
}

export type Totais = {
  pessoas: TotalPessoa[]
  totalGeral: ResumoGeral
}