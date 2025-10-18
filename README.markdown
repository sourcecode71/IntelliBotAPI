# 🤖 IntelliBot API - Intelligent ChatGPT Integration Platform

<div align="center">
<img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8.0" />
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL" />
<img src="https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white" alt="Redis" />
<img src="https://img.shields.io/badge/Swagger-85EA2D?logo=swagger&logoColor=black" alt="Swagger" />
<img src="https://img.shields.io/badge/OpenAI-412991?logo=openai&logoColor=white" alt="OpenAI" />

Enterprise-grade ChatGPT API integration with conversation management, caching, and monitoring built with .NET 8

[Features](#-features) • [Quick Start](#-quick-start) • [API Docs](#-api-docs) • [Deployment](#-deployment)
</div>

## 🌟 Overview
IntelliBot API is a production-ready .NET 8 Web API for integrating OpenAI's ChatGPT into your applications. It provides advanced features like conversation memory, Redis caching, rate limiting, and real-time streaming for building intelligent chatbot applications and AI-powered solutions.

🔥 **Perfect for**: Chat applications, AI assistants, customer support bots, and enterprise AI solutions

## 🚀 Features

### 🤖 AI & ChatGPT Integration
- **Multi-Model Support**: GPT-3.5 Turbo, GPT-4, GPT-4 Turbo, GPT-4o
- **Conversation Management**: Persistent chat history with context awareness
- **Streaming Responses**: Real-time token streaming for better UX
- **Smart Caching**: Redis-powered response caching to reduce API costs

### 💼 Enterprise Ready
- **Rate Limiting**: Configurable request throttling per user/IP
- **Resilience Patterns**: Polly retry policies and circuit breakers
- **Structured Logging**: Serilog with JSON formatting for observability
- **API Documentation**: Auto-generated Swagger/OpenAPI with examples
- **Validation**: FluentValidation for robust request validation

### 📊 Analytics & Monitoring
- **Usage Statistics**: Token tracking and cost estimation
- **Performance Metrics**: Response times and error rate monitoring
- **Health Checks**: Comprehensive service health monitoring

## 🛠️ Tech Stack
| Category        | Technologies                              |
|-----------------|-------------------------------------------|
| Backend         | .NET 8, ASP.NET Core, Entity Framework Core |
| Database        | PostgreSQL, Redis                         |
| AI/ML           | OpenAI API, ChatGPT Integration           |
| DevOps          | Docker, Serilog, Polly, FluentValidation  |
| API             | RESTful, Swagger/OpenAPI, AutoMapper      |

## 🏗️ Architecture
```
IntelliBotAPI/
├── 🎯 IntelliBot.API/                 # Controllers, DTOs, Configuration
├── 🧠 IntelliBot.Application/         # Services, Validators, Business Logic
├── 💾 IntelliBot.Infrastructure/      # Repositories, External APIs, Caching
├── 📦 IntelliBot.Core/               # Entities, Interfaces, Enums
└── 🔧 IntelliBot.Shared/             # Utilities, Extensions, Constants
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL 14+
- Redis 6+
- OpenAI API Key

### Installation & Setup
1. **Clone and setup**
```bash
git clone https://github.com/yourusername/IntelliBotAPI.git
cd IntelliBotAPI/src/IntelliBot.API
```

2. **Configure environment**
Update `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "DefaultModel": "gpt-3.5-turbo"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IntelliBot;Username=postgres;Password=yourpassword",
    "Redis": "localhost:6379"
  }
}
```

3. **Run database migrations**
```bash
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

### Access the API
- API: https://localhost:7131
- Swagger UI: https://localhost:7131/swagger
- Health Check: https://localhost:7131/health

## 📚 API Docs

### Core Endpoints
| Method | Endpoint                          | Description                   |
|--------|-----------------------------------|-------------------------------|
| POST   | `/api/chat/message`              | Send chat message            |
| POST   | `/api/chat/message/stream`       | Stream chat response         |
| GET    | `/api/chat/conversations`        | List conversations           |
| GET    | `/api/chat/conversations/{id}`   | Get conversation             |
| POST   | `/api/chat/conversations`        | Create conversation          |
| DELETE | `/api/chat/conversations/{id}`   | Delete conversation          |
| GET    | `/api/chat/models`               | Get available AI models      |
| GET    | `/api/chat/usage`                | Get usage statistics         |

### Example Request
```bash
curl -X POST "https://localhost:7131/api/chat/message" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Explain quantum computing in simple terms",
    "model": 1,
    "temperature": 0.7
  }'
```

## ⚙️ Deployment
### Docker Support
```bash
docker-compose up -d
```

### Configuration
| Variable                          | Description                | Default              |
|-----------------------------------|----------------------------|----------------------|
| `OpenAI__ApiKey`                 | OpenAI API Key            | Required             |
| `OpenAI__DefaultModel`           | Default AI model          | gpt-3.5-turbo       |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | Required             |
| `ConnectionStrings__Redis`       | Redis connection          | localhost:6379      |
| `Serilog__MinimumLevel`          | Logging level             | Information         |

### Rate Limiting
Configure in `appsettings.json`:
```json
{
  "RateLimiting": {
    "RequestsPerMinute": 10,
    "RequestsPerHour": 100,
    "RequestsPerDay": 1000
  }
}
```

## 🛠️ Development
### Building
```bash
dotnet build
```

### Testing
```bash
dotnet test
```

### Code Style
- EditorConfig for consistent code style
- XML Documentation for public APIs
- Async/Await pattern throughout

## 📊 Monitoring & Logging
### Log Structure
```json
{
  "Timestamp": "2025-10-18T12:00:00Z",
  "Level": "Information",
  "Message": "Chat request processed",
  "Properties": {
    "ProcessingTimeMs": 1250,
    "TokensUsed": 150,
    "Model": "gpt-3.5-turbo",
    "ConversationId": "conv-123"
  }
}
```

### Health Checks
- Overall health: GET `/health`
- Database health: GET `/health/ready`
- External services: GET `/health/live`

## 🤝 Contributing
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

See our [Contributing Guide](CONTRIBUTING.md) for details.

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support
- 📚 [Documentation](docs/)
- 🐛 [Issue Tracker](https://github.com/yourusername/IntelliBotAPI/issues)
- 💬 [Discussions](https://github.com/yourusername/IntelliBotAPI/discussions)

## 🙏 Acknowledgments
- OpenAI for the ChatGPT API
- .NET Team for the excellent framework
- All contributors and users of this project

<div align="center">
Made with ❤️ and .NET 8

If you find this project helpful, please give it a ⭐
</div>
