using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Company.Function
{
    // public class PedidoEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    // {
    //     public string Status { get; set; }
    //     public string NumeroDoPedido { get; set; }

    //     public PedidoEntity(string partitionKey, string rowKey)
    //         : base(partitionKey, rowKey)
    //     {
    //     }

    //     public PedidoEntity()
    //     {
    //     }
    // }

    public static class DurableFunctionsTechChallenge
    {            
        [FunctionName("DurableFunctionsTechChallenge")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            string produto = "Celular";

            outputs.Add(await context.CallActivityAsync<string>("BuscarProduto", produto));

            bool aprovado = await context.CallActivityAsync<bool>("AprovarPedido", produto);

            if (aprovado)
            {
                outputs.Add(await context.CallActivityAsync<string>("EfetuarPagamento", produto));
                outputs.Add(await context.CallActivityAsync<string>("Entregar", produto));

                string informacao = $"Pedido aprovado para o produto {produto}.";
                await context.CallActivityAsync("ArmazenarInformacoesNoStorage", informacao);
            }
            else
            {
                outputs.Add($"Pedido para o produto {produto} não foi aprovado.");
            }

            return outputs;
        }

        [FunctionName("BuscarProduto")]
        public static string BuscarProduto([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Executando a atividade Buscar Produto {name}.", name);
            return $"Buscando produto {name}!";
        }

        [FunctionName("AprovarPedido")]
        public static bool AprovarPedido([ActivityTrigger] string pedido, ILogger log)
        {
            log.LogInformation($"Aprovando pedido {pedido}.");

            bool aprovado = true;
            
            return aprovado;
        }

        [FunctionName("EfetuarPagamento")]
        public static string EfetuarPagamento([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Executando a atividade Efetuar Pagamento {name}.", name);
            return $"Realizando pagamento {name}!";
        }

        [FunctionName("Entregar")]
        public static string Entregar([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Executando a atividade Entregar {name}.", name);
            return $"Entregando produto {name}!";
        }

        // Ex para adicionar na table da azure

        // [FunctionName("ArmazenarInformacoesNaTabela")]
        // public static async Task ArmazenarInformacoesNaTabela(
        //     [ActivityTrigger] string informacao,
        //     [Table("pedidos")] CloudTable pedidosTable,
        //     ILogger log)
        // {
        //     var pedidoEntity = new PedidoEntity("ParticaoPadrao", Guid.NewGuid().ToString())
        //     {
        //         Status = "Aprovado",
        //         NumeroDoPedido = "1"
        //     };

        //     TableOperation insertOperation = TableOperation.Insert(pedidoEntity);
        //     await pedidosTable.ExecuteAsync(insertOperation);

        //     log.LogInformation($"Armazenando informações na tabela: {informacao}");
        // }

        // Ex para adicionar no blob

        // [FunctionName("ArmazenarInformacoesNoStorage")]
        // public static void ArmazenarInformacoesNoStorage([ActivityTrigger] string informacao, [Blob("test-durable-function/{rand-guid}.txt", FileAccess.Write)] TextWriter writer, ILogger log)
        // {
        //     log.LogInformation($"Armazenando informações no Storage: {informacao}");
        //     writer.Write(informacao);
        // }

        [FunctionName("DurableFunctionsTechChallenge_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("DurableFunctionsTechChallenge", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}