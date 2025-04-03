# Azure SQL Chat with Semantic Kernel

This project demonstrates a chat application that uses Azure OpenAI and Azure SQL Database to answer questions about data. The application is built using:

- ASP.NET Core Web API for the backend
- Vue.js for the frontend
- .NET Aspire for orchestration
- Semantic Kernel for AI integration
- Azure SQL Database for data storage

## Prerequisites

- .NET 8.0 SDK
- Node.js 18.x or later
- Azure OpenAI API key
- Azure SQL Database connection string

## Project Structure

- `AzureSqlSk.Api` - ASP.NET Core Web API project containing the chat and Semantic Kernel logic
- `AzureSqlSk.Web` - Vue.js frontend project with a modern chat interface
- `AzureSqlSk.Shared` - Shared library containing common models and interfaces
- `AzureSqlSk.AppHost` - .NET Aspire host project that orchestrates the API and frontend
- `AzureSqlSk.Console` - Console application for database deployment and chat interaction

## Configuration

### Console Application

1. Copy `.env.sample` to `.env` in the `AzureSqlSk.Console` directory and update the following values:
   ```
   OPENAI_API_KEY=your_azure_openai_api_key
   OPENAI_MODEL_ID=gpt-4
   SQL_SERVER_CONNECTION_STRING=your_azure_sql_connection_string
   ```

### API Application

1. Copy `appsettings.Development.Sample.json` to `appsettings.Development.json` in the `AzureSqlSk.Api` directory
2. Update the following values in `appsettings.Development.json`:
   ```json
   {
     "AzureOpenAI": {
       "ApiKey": "your_azure_openai_api_key",
       "ModelId": "gpt-4"
     },
     "ConnectionStrings": {
       "DefaultConnection": "your_azure_sql_connection_string"
     }
   }
   ```

## Database Deployment

1. Navigate to the console project directory:
   ```bash
   cd AzureSqlSk.Console
   ```

2. Run the deployment command:
   ```bash
   dotnet run deploy
   ```

This will create the necessary database tables and initial data.

## Running the Application

### Web Interface (Recommended)

1. Navigate to the Aspire host project:
   ```bash
   cd AzureSqlSk.AppHost
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

This will start both the API and frontend applications. The frontend will be available at `http://localhost:5173` and the API at `http://localhost:5000`.

### Console Interface

1. Navigate to the console project directory:
   ```bash
   cd AzureSqlSk.Console
   ```

2. Run the chat application:
   ```bash
   dotnet run chat
   ```

### Running Components Individually

#### API

1. Navigate to the API project:
   ```bash
   cd AzureSqlSk.Api
   ```

2. Run the API:
   ```bash
   dotnet run
   ```

#### Frontend

1. Navigate to the frontend project:
   ```bash
   cd AzureSqlSk.Web
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

## Development in GitHub Codespaces

This project includes DevContainer configuration for development in GitHub Codespaces. The container includes:

- .NET 8.0 SDK
- Node.js 18.x
- Required VS Code extensions
- Development tools and utilities

To use GitHub Codespaces:

1. Open this repository in GitHub Codespaces
2. The DevContainer will automatically build and configure the environment
3. Run the application using the Aspire host project as described above

## Deployment

The application can be deployed to Azure using the provided deployment scripts. See the deployment documentation for more details.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
