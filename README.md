# MixMedia MCP LexOffice

This project builds an **MCP (Model Context Protocol) server** for integrating with the [Lexoffice API](https://developers.lexoffice.io/). It enables LLMs and other clients to interact with Lexoffice resources (invoices, quotations, contacts, files, etc.) via a secure, tool-enabled MCP server.

## Features
- Secure MCP server with HTTP transport
- Lexoffice API integration (invoices, quotations, contacts, files, payments, vouchers, etc.)
- SSE security: Origin validation, localhost binding, and API key authentication
- Polly-based rate limiting and retry policies
- File download as LLM resources


## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- A valid Lexoffice API key ([get one here](https://app.lexoffice.de/))

### Configuration
1. **Set your Lexoffice API key**
   - For local development, set the environment variable `LEX_API_KEY` or use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).
   - Example for Windows PowerShell:
     ```powershell
     $env:LEX_API_KEY = "your-lexoffice-api-key"
     dotnet run
     ```
   - Or configure it in `Properties/launchSettings.json`:
     ```json
     "servers": {
            "lexoffice":{
                "type": "stdio",
                "command": "dotnet",
                "args": [
                    "run",
                    "--project",
                    "<path to .csproj>"
                ],
                "env": {}
            },
     }
     ```
2. **Run the server**
   - In your project directory, run:
     ```shell
     dotnet run
     ```
   - The server binds to `127.0.0.1:5000` by default for security.

### Authentication
- All HTTP requests must include the API key as a header:

  ```http
  X-Api-Key: <your-lexoffice-api-key>
  ```

- This header is required for all endpoints, including SSE connections.

### Example Request
```bash
curl http://localhost:5000/your-endpoint \
  -H "X-Api-Key: <your-lexoffice-api-key>"
```

### Example: List Invoices (MCP Tool)
```bash
curl http://localhost:5000/tools/lexoffice.listInvoices \
  -H "X-Api-Key: <your-lexoffice-api-key>"
```

### Example: Get Invoice Details
```bash
curl http://localhost:5000/tools/lexoffice.getInvoice \
  -H "X-Api-Key: <your-lexoffice-api-key>" \
  -d '{"invoiceId": "<invoice-guid>"}'
```

## Security Notes
- Only trusted origins are allowed for SSE (see `Program.cs` for details).
- The server is protected against DNS rebinding attacks and requires API key authentication for all SSE and API requests.

## Development
- All development should be done on the `main` branch.
- See the code for available tools and endpoints.

---
For more details, see the [Lexoffice API documentation](https://developers.lexoffice.io/).
