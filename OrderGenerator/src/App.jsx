import { useState } from "react";
import "./App.css";

const API_URL = import.meta.env.VITE_API_URL;

function App() {
  const [ativo, setAtivo] = useState("PETR4");
  const [lado, setLado] = useState("C");
  const [quantidade, setQuantidade] = useState("");
  const [preco, setPreco] = useState("");
  const [resultado, setResultado] = useState(null);
  const [loading, setLoading] = useState(false);

  async function enviarOrdem(event) {
    event.preventDefault();

    const ordem = {
      ativo,
      lado,
      quantidade: Number(quantidade),
      preco: Number(preco),
    };

    setLoading(true);
    setResultado(null);

    try {
      const response = await fetch(`${API_URL}/api/orders`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(ordem),
      });

      const data = await response.json();
      setResultado(data);
    } catch {
      setResultado({
        sucesso: false,
        msg_erro: "Não foi possível conectar ao servidor.",
      });
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="container">
      <h1>Ordens</h1>

      <form onSubmit={enviarOrdem}>
        <label>Ativo</label>
        <select value={ativo} onChange={(e) => setAtivo(e.target.value)}>
          <option value="PETR4">PETR4</option>
          <option value="VALE3">VALE3</option>
          <option value="VIIA4">VIIA4</option>
        </select>

        <label>Lado</label>
        <select value={lado} onChange={(e) => setLado(e.target.value)}>
          <option value="C">Compra</option>
          <option value="V">Venda</option>
        </select>

        <label>Quantidade</label>
        <input
          type="number"
          min="1"
          max="99999"
          step="1"
          required
          value={quantidade}
          onChange={(e) => setQuantidade(e.target.value)}
        />

        <label>Preço</label>
        <input
          type="number"
          min="0.01"
          max="999.99"
          step="0.01"
          required
          value={preco}
          onChange={(e) => setPreco(e.target.value)}
        />

        <button type="submit" disabled={loading}>
          {loading ? "Enviando..." : "Enviar ordem"}
        </button>
      </form>

      {resultado && (
        <div className={resultado.sucesso ? "sucesso" : "erro"}>
          <h3>{resultado.sucesso ? "Sucesso" : "Erro"}</h3>

          {resultado.sucesso ? (
            <p>Exposição atual: R$ {resultado.exposicao_atual?.toFixed(2)}</p>
          ) : (
            <p>{resultado.msg_erro}</p>
          )}
        </div>
      )}
    </div>
  );
}

export default App;
