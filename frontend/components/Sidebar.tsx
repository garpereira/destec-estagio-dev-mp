import { NavLink } from 'react-router-dom'
import { Users, ArrowLeftRight, ChartPie } from "lucide-react";

function Sidebar(){

   const menu = [
  { id: "pessoas", label: "Pessoas", routes: "/pessoas", icone: <Users size={20} className="text-white" /> },
  { id: "transacoes", label: "Transações", routes: "/transacoes", icone: <ArrowLeftRight size={20} className="text-white" /> },
  { id: "totais", label: "Totais", routes: "/totais", icone: <ChartPie size={20} className="text-white" /> },
  ];

  return (
    <aside className="sticky top-0 flex h-screen w-64 shrink-0 flex-col bg-blue-950 ">
          <div className="flex items-center gap-3 px-6 py-6">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white">
                
            </div>
            <div className="">
              <p className="text-white text-2xl font-medium">Despesas</p>
              <p className="text-gray-400">Residenciais</p>
            </div>
          </div>
          <nav className="flex flex-1 flex-col gap-1 px-3 py-2">
            {/* optei por adicionar uma iteração para a construção do menu lateral, para evitar ficar editando e copiando diversos
            divs que teriam o mesmo atributo. De acordo com a necessidade de adição de novos atributos, basta atualizar a estrutura */}
            {menu.map((item) => (
              
              <NavLink
                key={item.id}
                to={item.routes}
                className={({ isActive }) => `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-white shadow-sm cursor-pointer transition-colors
                  ${
                    isActive
                      ? "bg-blue-600"
                      : "hover:bg-blue-600"
                  }`}
              > {item.icone}
                {item.label}
              </NavLink>
            ))}
          </nav>
          <div className="px-6 py-5 text-xs text-gray-400">Controle financeiro da casa</div>
          {/* menu lateral */}
      </aside>

  )
}

export default Sidebar;