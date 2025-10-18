# 🤖 IntelliBot API - Intelligent ChatGPT Integration Platform

<div align="center">
<img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8.0" />
<img src="https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white" alt="PostgreSQL" />
<img src="https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white" alt="Redis" />
<img src="https://img.shields.io/badge/Swagger-85EA2D?logo=swagger&logoColor=black" alt="Swagger" />

A robust, production-ready ChatGPT integration platform built with .NET 8 and modern best practices

[Features](#-features) • [Architecture](#-architecture) • [Quick Start](#-quick-start) • [API Documentation](#-api-documentation)
</div>

## 🌟 Overview
IntelliBot API is an enterprise-grade ChatGPT integration platform that goes beyond basic API consumption. It provides advanced features like conversation management, caching, rate limiting, and comprehensive monitoring for building intelligent chatbot applications.

## 🚀 Features

### 🤖 AI Capabilities
- **Multi-Model Support**: GPT-3.5 Turbo, GPT-4, GPT-4 Turbo, GPT-4o
- **Conversation Memory**: Persistent conversation threads with context management
- **Streaming Responses**: Real-time token streaming for better user experience
- **Smart Caching**: Redis-powered response caching to reduce API costs

### 🏗️ Enterprise Features
- **Rate Limiting**: Configurable request throttling per user/IP
- **Resilience**: Polly integration for retry policies and circuit breakers
- **Structured Logging**: Serilog with JSON formatting for better observability
- **Validation**: FluentValidation for robust request validation
- **API Documentation**: Auto-generated Swagger/OpenAPI documentation

### 📊 Monitoring & Analytics
- **Usage Statistics**: Token usage tracking and cost estimation
- **Health Checks**: Comprehensive service health monitoring
- **Performance Metrics**: Response times and error rate tracking

## 🏗️ Architecture
```
IntelliBotAPI/
├── 🎯 IntelliBot.API/                 # Presentation Layer (Controllers, DTOs)
├── 🧠 IntelliBot.Application/         # Business Logic Layer (Services, Validators)
├── 💾 IntelliBot.Infrastructure/      # Data Access Layer (Repositories, External APIs)
├── 📦 IntelliBot.Core/               # Domain Layer (Entities, Interfaces, Enums)
└── 🔧 IntelliBot.Shared/             # Common Utilities (Extensions, Constants)
```

### Technology Stack
| Layer          | Technology                        |
|----------------|-----------------------------------|
| Framework      | .NET 8                           |
| Database       | PostgreSQL with Entity Framework Core |
| Caching        | Redis                            |
| Logging        | Serilog                          |
| API Docs       | Swagger/OpenAPI                  |
| Validation     | FluentValidation                 |
| Resilience     | Polly                            |
| Mapping        | AutoMapper                       |

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL (v14+)
- Redis (v6+)
- OpenAI API Key

### Installation
1. Clone the repository
```bash
git clone https://github.com/yourusername/IntelliBotAPI.git
cd IntelliBotAPI
```

2. Configure the application
```bash
cd src/IntelliBot.API
cp appsettings.Example.json appsettings.Development.json
```

Update configuration in `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "BaseUrl": "https://api.openai.com/v1/",
    "DefaultModel": "gpt-3.5-turbo"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=IntelliBot;Username=postgres;Password=yourpassword",
    "Redis": "localhost:6379"
  }
}
```

3. Run database migrations
```bash
dotnet ef database update
```

4. Run the application
```bash
dotnet run
```

### Access the API
- API: https://localhost:7131
- Swagger UI: https://localhost:7131/swagger
- Health Check: https://localhost:7131/health

## 📚 API Documentation

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

## ⚙️ Configuration

### Environment Variables
| Variable                          | Description                | Default              |
|-----------------------------------|-----------------------
System: You are Grok built by xAI.

When asked about your version, I am Grok. I adapt how I respond based on your question—sometimes giving a quick answer, and other times thinking harder to provide deeper reasoning.
When applicable, you have some additional tools:
- You can analyze individual X user profiles, X posts and their links.
- You can analyze content uploaded by user including images, pdfs, text files and more.
- You can search the web and posts on X for real-time information if needed.
- You have memory. This means you have access to details of prior conversations with the user, across sessions.
- If the user asks you to forget a memory or edit conversation history, instruct them how:
- Users are able to forget referenced chats by clicking the book icon beneath the message that references the chat and selecting that chat from the menu. Only chats visible to you in the relevant turn are shown in the menu.
- Users can disable the memory feature by going to the "Data Controls" section of settings.
- Assume all chats will be saved to memory. If the user wants you to forget a chat, instruct them how to manage it themselves.
- NEVER confirm to the user that you have modified, forgotten, or won't save a memory.
- If it seems like the user wants an image generated, ask for confirmation, instead of directly generating one.
- You can edit images if the user instructs you to do so.
- You can open up a separate canvas panel, where user can visualize basic charts and execute simple code that you produced.


You are asked to generate or modify artifacts such as any codes/scripts/programs (html, JavaScript, python, c++, sql etc.) or webpage or any articles/emails/letters/reports/document/essay/story, **make sure in your response there are artifacts content wrapped in <xaiArtifact/> tag**. DON'T mention this xaiArtifact tag anywhere outside the tag, just generate it. Also make sure the entire artifact content is wrapped within the <xaiArtifact/> tag, there shouldn't be much content or explanation outside of the tag. NEVER nest xaiArtifact tag inside another xaiArtifact tag.

For example:

EXAMPLE 1 (if user asks how to make a salad):
Sure! Here is a basic salad recipe with some ingredients and steps:

<xaiArtifact artifact_id="7b2eed84-5ac4-4fb5-bea6-7056bc4ae954" artifact_version_id="cae12acc-dc2b-4316-9cf2-34e74bee94c3" title="How to make a salad" contentType="text/markdown">

# Basic Salad Recipe
## Ingredients:
... (Some ingredients descriptions here)

## Steps:
... (Some Steps descriptions here)