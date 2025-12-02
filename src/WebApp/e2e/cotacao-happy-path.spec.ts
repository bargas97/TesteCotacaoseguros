import { test, expect } from '@playwright/test';

async function esperarApiPronta() {
  for (let i = 0; i < 10; i++) {
    try {
      const res = await fetch('http://localhost:5138/health');
      if (res.ok) return; // saúde ok
    } catch {}
    await new Promise(r => setTimeout(r, 1500));
  }
  throw new Error('API não respondeu /health dentro do timeout');
}

test.describe('Cotação - Happy Path', () => {
  test.beforeAll(async () => {
    await esperarApiPronta();
  });

  test('fluxo completo de cotação via API', async ({ request }) => {
    // Teste direto na API (sem depender do frontend SSR problemático)
    
    // 1. Criar rascunho
    const criarResp = await request.post('http://localhost:5138/cotacoes', {
      data: {
        produtoId: 1,
        proponente: {
          nome: "Teste E2E",
          cpfCnpj: "52998224725",
          genero: 0,
          estadoCivil: "Solteiro",
          dtNascimento: "1990-05-10",
          cepResidencial: "06000000"
        },
        veiculo: {
          codigoFipeOuVeiculo: "FIPE123",
          anoModelo: 2024,
          anoFabricacao: 2024,
          cepPernoite: "06000000",
          tipoUtilizacao: 0,
          zeroKm: false,
          blindado: false,
          kitGas: false,
          placa: "ABC1234"
        },
        coberturas: [
          {
            coberturaId: 1,
            importanciaSegurada: 10000,
            contratada: true,
            franquiaSelecionada: "Média"
          }
        ]
      }
    });
    
    expect(criarResp.ok()).toBeTruthy();
    const { id } = await criarResp.json();
    expect(id).toBeGreaterThan(0);
    
    // 2. Calcular
    const calcularResp = await request.put(`http://localhost:5138/cotacoes/${id}/calcular`, { data: {} });
    expect(calcularResp.status()).toBe(204);
    
    // 3. Aprovar
    const aprovarResp = await request.put(`http://localhost:5138/cotacoes/${id}/aprovar`, { data: {} });
    expect(aprovarResp.status()).toBe(204);
    
    // 4. Obter e validar estado final
    const obterResp = await request.get(`http://localhost:5138/cotacoes/${id}`);
    expect(obterResp.ok()).toBeTruthy();
    const cotacao = await obterResp.json();
    
    expect(cotacao.status).toBe(2); // Aprovada
    expect(cotacao.premioTotal).toBeGreaterThan(0);
    expect(cotacao.numero).toMatch(/^\d{4}-\d+$/); // formato YYYY-SEQ
  });
});
