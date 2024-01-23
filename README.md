# Durable Function Orchestration

Segunda parte do Tech Challenge - Fase 2

## Integrantes

[Lucas Hanke](https://github.com/lucasbagrt)
[João Gasparini](https://github.com/joaogasparini)
[Victoria Pacheco](https://github.com/vickypacheco)
[Rafael Araujo](https://github.com/RafAraujo)
[Cristian Kulessa](https://github.com/Kulessa)

## Build

Se estiver utilizando o Visual Studio Code, verifique se possui as extensões necessárias para rodar as Azure Functions e o C#.

Criar um arquivo local.settings.json, neste modelo:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "sua_connection_string",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}
```

**Para ter acesso a sua connection string primeiro crie um Storage Account no portal da Azure, acesse a aba Acess Keys e copie ela.**

## Retornos

Após rodar o projeto, obtemos a seguinte url local: http://localhost:7071/api/DurableFunctionsTechChallenge_HttpStart

No Postman, obtemos a seguinte resposta para quando o pedido é Aprovado:

E para quando é Negado:



