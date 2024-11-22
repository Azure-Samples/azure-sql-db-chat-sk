# Azure SQL Chat with your data BOT

This is a simple example of a chatbot that uses Azure SQL to store and retrieve data using both RAG and Natural-Language-to-SQL (NL2QL) to allow chat on both structured and non-structured data. The bot is built using the Microsoft Semantic Kernel Framework and the newly added support for vectors in Azure SQL.

> [!NOTE]
> If you are looking for the sample using *insurance* data, please use the `insurance-chatbot-demo` branch. 

## Architecture

![Architecture](./_assets/azure-sql-sk-bot.png)

## Solution

The solution is composed of three main Azure components:

- [Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview?view=azuresql): The database that stores the data.
- [Azure Open AI](https://learn.microsoft.com/azure/ai-services/openai/): The language model that generates the text and the embeddings.
- [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/): The library used to orchestrate calls to LLM to do RAG and NL2SQL and to store long-term memories in the database.

### Azure Open AI

Make sure to have two models deployed, one for generating embeddings (*text-embedding-3-small* model recommended) and one for handling the chat (*gpt-4 turbo* recommended). You can use the Azure OpenAI service to deploy the models. Make sure to have the endpoint and the API key ready. The two models are assumed to be deployed with the following names:

- Embedding model: `text-embedding-3-small`
- Chat model: `gpt-4`

### Configure environment 

Create a `.env` file starting from the `.env.sample` file:

- `OPENAI_URL`: specify the URL of your Azure OpenAI endpoint, eg: 'https://my-open-ai.openai.azure.com/'
- `OPENAI_KEY`: specify the API key of your Azure OpenAI endpoint
- `OPENAI_MODEL`: specify the deployment name of your Azure OpenAI embedding endpoint, eg: 'text-embedding-3-small'

- `MSSQL`: the connection string to the Azure SQL database where you want to deploy the database objects and sample data
- `MSSQL_TABLE_NAME`: the name of the table where the chatbot will store long-term memories

### Database

> [!NOTE]  
> Vector Functions are in Public Preview. Learn the details about vectors in Azure SQL here: https://aka.ms/azure-sql-vector-public-preview

To deploy the database, you can just use the `deploy` option of the chatbot application. Make sure you have created the `.env` file as explained in the previoud section, and then run the following command:

```bash
dotnet run deploy
```

That will connect to Azure SQL and deploy the needed database objects and some sample data.

## Application

To run the application, make sure you have created the `.env` file and deployed the database as explained in the previous section, and then run the following command:

```bash
dotnet run chat
```

The chatbot will start and you can start chatting with it. Use the `/ch` command to clear the chat history and `/h` to see the chat history. End the chat with `ctrl-c`.

The prompt will look like this:

```bash 
(H: 1) Question: 
```

`H` indicates the chat memory size. The chatbot will remember the last `H` interactions. 

You can now start to chat with your own data. Have fun!

## F.A.Q.

### How can I quickly generate the embeddings for my data already stored in Azure SQL?

Take a look at the Azure SQL Vectorizer repository: 

https://github.com/Azure-Samples/azure-sql-db-vectorizer

It does exactly what you are looking for.
