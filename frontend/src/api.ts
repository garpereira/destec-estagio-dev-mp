import type{
  CriarPessoaRequest,
  CriarTransacaoRequest,
  Pessoa,
  Totais,
  Transacao
} from './types'

const API_URL = "http://localhost:5109"

async function request<T>(path: string, options?: RequestInit): Promise<T>{
  const response = await fetch(`${API_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  })

  if(!response.ok){
    const message = await response.text()
    throw new Error(message || 'Erro ao realizar requisição.')
  }

  // Se retornar sem conteúdo, não tem como ser retornado algo concreto como promise
  if (response.status == 204){
    return undefined as T
  }

  return response.json()

}

export function listarPessoas() {
  return request<Pessoa[]>('/pessoas')
}

export function criarPessoa(data: CriarPessoaRequest){
  return request<Pessoa>('/pessoas', {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export function deletarPessoa(id: number){
  return request<void>(`/pessoas/${id}`, {
    method: 'DELETE',
  })
}

export function listarTransacoes(){
  return request<Transacao[]>('/transacoes')
}

export function criarTransacao(data: CriarTransacaoRequest){
  return request <Transacao>('/transacoes', {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export function buscarTotais(){
  return request<Totais>('/totais')
}