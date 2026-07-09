import { Routes, Route, Navigate } from 'react-router-dom'
import './App.css'

import Pessoas from '../pages/Pessoas'
import Transacoes from '../pages/Transacoes'
import TotalGeral from '../pages/TotalGeral'
import Sidebar from '../components/Sidebar'

function App() {
 

  return (
    <main className="min-h-screen bg-slate-100 text-slate-900 flex flex-row">
      {/* menu lateral */}
      <Sidebar />
      {/* Conteúdo principal de renderização, o Router vai renderizar o conteúdo desta parte da pagina sem a necessidade
      de carrregar a pagina inteira, apenas atualiza o link, permitindo o usuario voltar na mesma pagina */}
      <Routes>
      {/* /pessoas */}
        <Route 
          path="/" 
          element={<Navigate to="/pessoas" replace />} 
        />
        <Route 
          path="/pessoas" 
          element={<Pessoas />} 
          />
        <Route
          path="/transacoes"
          element={<Transacoes />}
        />
        <Route
          path="/totais"
          element={<TotalGeral />}
        />
      </Routes>
    </main>
  )
}

export default App