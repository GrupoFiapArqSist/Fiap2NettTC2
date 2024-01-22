using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DurableFunction.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class DurableFunctionsTechChallenge
    {
        [FunctionName("DurableFunctionsTechChallenge_HttpStart")]
        public static async Task<IActionResult> HttpStart(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
         [DurableClient] IDurableOrchestrationClient starter,
         ILogger log)
        {
            string corpoRequisicao = await new StreamReader(req.Body).ReadToEndAsync();
            List<PedidoItemEntity> pedidos = JsonConvert.DeserializeObject<List<PedidoItemEntity>>(corpoRequisicao);

            string instanceId = await starter.StartNewAsync("DurableFunctionsTechChallenge", null, pedidos);

            log.LogInformation("Iniciada orquestração com ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("DurableFunctionsTechChallenge")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var itensPedido = context.GetInput<List<PedidoItemEntity>>();

            var pedido = new PedidoEntity
            {
                NumeroPedido = Guid.NewGuid().ToString().Split("-")[0],
                Status = "Iniciado",
                PedidoItens = itensPedido
            };

            foreach (var pedidoItens in pedido.PedidoItens)
                outputs.Add(await context.CallActivityAsync<string>("BuscarProduto", pedidoItens.Produto));

            bool aprovado = await context.CallActivityAsync<bool>("AprovarPedido", pedido);
            if (!aprovado) { outputs.Add($"Pedido {pedido.NumeroPedido} não foi aprovado."); return outputs; }

            outputs.Add(await context.CallActivityAsync<string>("EfetuarPagamento", pedido));
            outputs.Add(await context.CallActivityAsync<string>("Entregar", pedido));

            await context.CallActivityAsync("ArmazenarInformacoesNaTabela", pedido);
            return outputs;
        }

        [FunctionName("BuscarProduto")]
        public static string BuscarProduto([ActivityTrigger] string produto, ILogger log)
        {
            log.LogInformation("Executando a atividade Buscar Produto {produto}.", produto);
            return $"Buscando produto {produto}!";
        }

        [FunctionName("AprovarPedido")]
        public static bool AprovarPedido([ActivityTrigger] PedidoEntity pedido, ILogger log)
        {
            log.LogInformation($"Aprovando pedido {pedido}.");

            foreach (var pedidoItem in pedido.PedidoItens)
                log.LogInformation($"Item {pedidoItem.Produto} - {pedidoItem.Quantidade}.");

            Random random = new Random();
            return random.Next(0, 2) != 0;
        }

        [FunctionName("EfetuarPagamento")]
        public static string EfetuarPagamento([ActivityTrigger] PedidoEntity pedido, ILogger log)
        {
            log.LogInformation($"Executando a atividade Efetuar Pagamento {pedido.NumeroPedido}.");
            return $"Realizando pagamento {pedido.NumeroPedido}!";
        }

        [FunctionName("Entregar")]
        public static string Entregar([ActivityTrigger] PedidoEntity pedido, ILogger log)
        {
            log.LogInformation($"Executando a atividade Entregar {pedido.NumeroPedido}.");
            return $"Entregando produto {pedido.NumeroPedido}!";
        }      
    }
}