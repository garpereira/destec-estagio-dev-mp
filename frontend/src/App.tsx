import React from 'react'
import './App.css'

function App() {
  const [total, setTotal] = React.useState(0);

  function incrementar() {
    setTotal((total) => total + 1)
  }

  return (
    <div className="">
      <h1 className="text-3xl font-bold underline align-center">Controle de Gastos Residenciais</h1>
      <p className="text-lg">Sistema para cadastro de pessoas, transações e consulta de totais.</p>
      <div className="">
        <p>Total: {total}</p>
        <button className="bg-blue-600 hover:bg-blue-700 text-white font-medium p-2 rounded" onClick={incrementar}>Incrementar</button>
      </div>
    </div>
  )
}

export default App
