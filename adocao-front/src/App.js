import React, { useState, useEffect } from "react";
import axios from "axios";
import "./App.css";

function App() {
  const [animais, setAnimais] = useState([]);
  const [adotantes, setAdotantes] = useState([]);
  const [novoAnimal, setNovoAnimal] = useState({ nome: "", especie: "", idade: "", status: "" });
  const [novoAdotante, setNovoAdotante] = useState({ nome: "", email: "" });
  const [buscaId, setBuscaId] = useState("");
  const [resultadoBusca, setResultadoBusca] = useState(null);
  const [editandoId, setEditandoId] = useState(null);

  const apiUrl = "http://localhost:5279";

  useEffect(() => {
    fetchAnimais();
    fetchAdotantes();
  }, []);

  const fetchAnimais = async () => {
    try {
      const res = await axios.get(`${apiUrl}/Animais`);
      setAnimais(res.data);
    } catch (err) {
      console.error(err);
      alert("Erro ao carregar animais");
    }
  };

  const fetchAdotantes = async () => {
    try {
      const res = await axios.get(`${apiUrl}/Adotantes`);
      setAdotantes(res.data);
    } catch (err) {
      console.error(err);
      alert("Erro ao carregar adotantes");
    }
  };

  const adicionarOuEditarAnimal = async () => {
    try {
      if (editandoId) {
        await axios.put(`${apiUrl}/Animais/${editandoId}`, novoAnimal);
        alert("Animal atualizado com sucesso!");
      } else {
        await axios.post(`${apiUrl}/Animais`, novoAnimal);
        alert("Animal cadastrado com sucesso!");
      }
      fetchAnimais();
      setNovoAnimal({ nome: "", especie: "", idade: "", status: "" });
      setEditandoId(null);
    } catch (err) {
      console.error(err);
      alert("Erro ao salvar animal");
    }
  };

  const editarAnimal = (animal) => {
    setNovoAnimal({
      nome: animal.nome,
      especie: animal.especie,
      idade: animal.idade,
      status: animal.status,
    });
    setEditandoId(animal.id);
  };

  const excluirAnimal = async (id) => {
    if (!window.confirm("Deseja realmente excluir este animal?")) return;

    try {
      await axios.delete(`${apiUrl}/Animais/${id}`);
      fetchAnimais();
      alert("Animal excluído com sucesso!");
    } catch (err) {
      console.error(err);
      alert("Erro ao excluir animal");
    }
  };

  const adicionarAdotante = async () => {
    try {
      await axios.post(`${apiUrl}/Adotantes`, novoAdotante);
      fetchAdotantes();
      setNovoAdotante({ nome: "", email: "" });
      alert("Adotante cadastrado com sucesso!");
    } catch (err) {
      console.error(err);
      alert("Erro ao adicionar adotante");
    }
  };

  const buscarPorId = async () => {
    try {
      const animal = await axios.get(`${apiUrl}/Animais/${buscaId}`);
      setResultadoBusca(animal.data);
    } catch {
      setResultadoBusca(null);
      alert("Animal não encontrado");
    }
  };

  return (
    <div className="container">
      <h1>🐾 Sistema de Adoção</h1>

      <div className="form-section">
        <h2>{editandoId ? "Editar Animal" : "Adicionar Animal"}</h2>
        <input
          type="text"
          placeholder="Nome"
          value={novoAnimal.nome}
          onChange={(e) => setNovoAnimal({ ...novoAnimal, nome: e.target.value })}
        />
        <input
          type="text"
          placeholder="Espécie"
          value={novoAnimal.especie}
          onChange={(e) => setNovoAnimal({ ...novoAnimal, especie: e.target.value })}
        />
        <input
          type="number"
          placeholder="Idade"
          value={novoAnimal.idade}
          onChange={(e) => setNovoAnimal({ ...novoAnimal, idade: e.target.value })}
        />
        <input
          type="text"
          placeholder="Status"
          value={novoAnimal.status}
          onChange={(e) => setNovoAnimal({ ...novoAnimal, status: e.target.value })}
        />
        <button onClick={adicionarOuEditarAnimal}>
          {editandoId ? "Salvar Alterações" : "Cadastrar Animal"}
        </button>
        {editandoId && (
          <button
            onClick={() => {
              setEditandoId(null);
              setNovoAnimal({ nome: "", especie: "", idade: "", status: "" });
            }}
          >
            Cancelar
          </button>
        )}
      </div>

      <div className="form-section">
        <h2>Adicionar Adotante</h2>
        <input
          type="text"
          placeholder="Nome"
          value={novoAdotante.nome}
          onChange={(e) => setNovoAdotante({ ...novoAdotante, nome: e.target.value })}
        />
        <input
          type="email"
          placeholder="Email"
          value={novoAdotante.email}
          onChange={(e) => setNovoAdotante({ ...novoAdotante, email: e.target.value })}
        />
        <button onClick={adicionarAdotante}>Salvar Adotante</button>
      </div>

      <div className="form-section">
        <h2>Buscar Animal por ID</h2>
        <input
          type="number"
          placeholder="ID"
          value={buscaId}
          onChange={(e) => setBuscaId(e.target.value)}
        />
        <button onClick={buscarPorId}>Buscar</button>

        {resultadoBusca && (
          <div className="card">
            <p><strong>ID:</strong> {resultadoBusca.id}</p>
            <p><strong>Nome:</strong> {resultadoBusca.nome}</p>
            <p><strong>Espécie:</strong> {resultadoBusca.especie}</p>
            <p><strong>Status:</strong> {resultadoBusca.status}</p>
          </div>
        )}
      </div>

      <div className="list-section">
        <h2>🐶 Animais Cadastrados</h2>
        {animais.map((a) => (
          <div key={a.id} className="card">
            <p><strong>{a.nome}</strong> ({a.especie})</p>
            <p>Idade: {a.idade} anos</p>
            <p>Status: {a.status}</p>
            <button onClick={() => editarAnimal(a)}>Editar</button>
            <button onClick={() => excluirAnimal(a.id)}>Excluir</button>
          </div>
        ))}
      </div>

      <div className="list-section">
        <h2>👤 Adotantes Cadastrados</h2>
        {adotantes.map((a) => (
          <div key={a.id} className="card">
            <p><strong>{a.nome}</strong></p>
            <p>{a.email}</p>
          </div>
        ))}
      </div>
    </div>
  );
}

export default App;