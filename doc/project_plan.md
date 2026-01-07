# Document Management System (DMS) - Project Implementation Plan

## Project Overview

This semester project implements a comprehensive Document Management System using microservices architecture with automatic OCR, AI-powered summarization, tagging, and full-text search capabilities.

### Core Architecture
- **PaperlessWebUI** (Port 80) - Frontend with Nginx
- **PaperlessRESTAPI** (Port 8081) - Backend REST API
- **PaperlessServices** (Port 8082) - OCR & GenAI Workers
- **Supporting Services**: RabbitMQ, PostgreSQL, MinIO, Elasticsearch, Adminer

## Project Structure (2 C# + 1 Frontend)

### 1. PaperlessWebUI (React + TypeScript Frontend)
```
PaperlessWebUI/
├── public/                     # Static assets
│   ├── index.html
│   ├── favicon.ico
│   └── images/
├── src/                        # Source code
│   ├── components/             # Reusable UI components
│   │   ├── common/
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── LoadingSpinner.tsx
│   │   │   └── ErrorBoundary.tsx
│   │   ├── document/
│   │   │   ├── DocumentCard.tsx
│   │   │   ├── DocumentList.tsx
│   │   │   ├── DocumentDetail.tsx
│   │   │   ├── DocumentUpload.tsx
│   │   │   └── DocumentSearch.tsx
│   │   └── collaboration/      # Additional use case components
│   │       ├── UserManagement.tsx
│   │       ├── CommentSection.tsx
│   │       └── VersionHistory.tsx
│   ├── pages/                  # Page components
│   │   ├── Dashboard.tsx
│   │   ├── DocumentsPage.tsx
│   │   ├── SearchPage.tsx
│   │   ├── UploadPage.tsx
│   │   └── CollaborationPage.tsx
│   ├── hooks/                  # Custom React hooks
│   │   ├── useDocuments.ts
│   │   ├── useSearch.ts
│   │   ├── useUpload.ts
│   │   └── useWebSocket.ts
│   ├── services/               # API service layer
│   │   ├── api.ts              # Axios configuration
│   │   ├── documentService.ts
│   │   ├── searchService.ts
│   │   ├── authService.ts
│   │   └── websocketService.ts
│   ├── store/                  # State management (Redux Toolkit)
│   │   ├── index.ts
│   │   ├── slices/
│   │   │   ├── documentSlice.ts
│   │   │   ├── searchSlice.ts
│   │   │   ├── authSlice.ts
│   │   │   └── uiSlice.ts
│   │   └── middleware/
│   │       └── apiMiddleware.ts
│   ├── types/                  # TypeScript type definitions
│   │   ├── document.types.ts
│   │   ├── search.types.ts
│   │   ├── user.types.ts
│   │   └── api.types.ts
│   ├── utils/                  # Utility functions
│   │   ├── formatters.ts
│   │   ├── validators.ts
│   │   ├── constants.ts
│   │   └── helpers.ts
│   ├── styles/                 # Styling
│   │   ├── globals.css
│   │   ├── variables.css
│   │   └── components/
│   ├── App.tsx                 # Main App component
│   ├── index.tsx               # Entry point
│   └── setupTests.ts          # Test configuration
├── nginx/                      # Nginx configuration
│   ├── nginx.conf
│   └── default.conf
├── package.json
├── tsconfig.json
├── tailwind.config.js
├── vite.config.ts             # Vite build configuration
├── Dockerfile                 # Multi-stage build
└── .env.example               # Environment variables template
```

### 2. PaperlessRESTAPI
```
PaperlessRESTAPI/
├── Controllers/                # REST API Controllers
│   ├── DocumentsController.cs
│   ├── SearchController.cs
│   └── HealthController.cs
├── Models/                     # DTOs and API Models
│   ├── DTOs/
│   │   ├── DocumentDto.cs
│   │   ├── SearchResultDto.cs
│   │   └── UploadDocumentDto.cs
│   └── Requests/
│       ├── CreateDocumentRequest.cs
│       └── SearchRequest.cs
├── Services/                   # Business Logic Layer
│   ├── Interfaces/
│   │   ├── IDocumentService.cs
│   │   ├── IQueueService.cs
│   │   └── IStorageService.cs
│   └── Implementations/
│       ├── DocumentService.cs
│       ├── RabbitMQService.cs
│       └── MinIOService.cs
├── Data/                       # Data Access Layer
│   ├── Entities/
│   │   ├── Document.cs
│   │   ├── DocumentAccess.cs
│   │   └── Tag.cs
│   ├── Repositories/
│   │   ├── IDocumentRepository.cs
│   │   ├── DocumentRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   └── AutoMapperProfile.cs
│   └── ApplicationDbContext.cs
├── Infrastructure/             # Cross-cutting concerns
│   ├── Exceptions/
│   │   ├── BusinessException.cs
│   │   ├── DataException.cs
│   │   └── ServiceException.cs
│   ├── Middleware/
│   │   ├── ExceptionMiddleware.cs
│   │   └── LoggingMiddleware.cs
│   └── Validation/
│       ├── DocumentValidator.cs
│       └── ValidationExtensions.cs
├── Configuration/
│   ├── DatabaseConfig.cs
│   ├── RabbitMQConfig.cs
│   └── MinIOConfig.cs
├── Program.cs
├── Dockerfile
└── PaperlessRESTAPI.csproj
```

### 3. PaperlessServices
```
PaperlessServices/
├── Workers/                    # Background Services
│   ├── OcrWorker.cs
│   ├── GenAIWorker.cs
│   ├── IndexingWorker.cs
│   └── BatchProcessingWorker.cs
├── Services/                   # Service Implementations
│   ├── Interfaces/
│   │   ├── IOcrService.cs
│   │   ├── IGenAIService.cs
│   │   ├── IElasticsearchService.cs
│   │   └── IBatchProcessingService.cs
│   └── Implementations/
│       ├── TesseractOcrService.cs
│       ├── GeminiAIService.cs
│       ├── ElasticsearchService.cs
│       └── XmlBatchProcessingService.cs
├── Models/                     # Message Models
│   ├── Messages/
│   │   ├── OcrMessage.cs
│   │   ├── GenAIMessage.cs
│   │   └── IndexingMessage.cs
│   └── DTOs/
│       ├── OcrResultDto.cs
│       ├── SummaryDto.cs
│       └── AccessLogDto.cs
├── Infrastructure/
│   ├── Exceptions/
│   │   ├── OcrException.cs
│   │   ├── GenAIException.cs
│   │   └── ElasticsearchException.cs
│   ├── Configuration/
│   │   ├── OcrConfig.cs
│   │   ├── GenAIConfig.cs
│   │   └── ElasticsearchConfig.cs
│   └── Utilities/
│       ├── FileHelper.cs
│       └── XmlParser.cs
├── Program.cs
├── Dockerfile
└── PaperlessServices.csproj
```

## Sprint Implementation Plan

### Sprint 1: Project Setup, REST API, DAL (Weeks 1-2)
**Objectives:**
- Set up C# project structure
- Implement REST API with basic CRUD operations
- Integrate ORM with PostgreSQL using Repository pattern
- Create docker-compose.yml

**Deliverables:**
- ✅ All three C# projects created with proper structure
- ✅ REST endpoints for document management
- ✅ Entity Framework Core with PostgreSQL integration
- ✅ Repository pattern implementation
- ✅ Unit tests with mocking (>70% coverage target)
- ✅ Docker containers for API and database

**Technical Implementation:**
```csharp
// Document Entity
public class Document
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }
    public string OcrText { get; set; }
    public string Summary { get; set; }
    public List<Tag> Tags { get; set; }
}

// Repository Pattern
public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> SearchByTextAsync(string searchTerm);
    Task<Document> GetByFileNameAsync(string fileName);
}
```

### Sprint 2: Web UI (Weeks 3-4)
**Objectives:**
- Implement modern React + TypeScript frontend
- Create responsive dashboard and detail pages
- Integrate frontend with REST API using Axios
- Setup Nginx web server with React SPA

**Deliverables:**
- ✅ Modern React 18 + TypeScript application
- ✅ Responsive UI with Tailwind CSS
- ✅ Document upload with drag-and-drop (react-dropzone)
- ✅ Document listing with pagination and filtering
- ✅ Basic search interface with debounced input
- ✅ Multi-stage Docker build with Nginx
- ✅ API integration with error handling

**UI Components:**
- Dashboard with document statistics
- Upload form with drag-and-drop
- Document grid/list view
- Document detail page
- Search interface
- Navigation and user feedback

### Sprint 3: RabbitMQ Integration (Weeks 5-6)
**Objectives:**
- Integrate message queuing system
- Implement async document processing
- Add comprehensive logging and exception handling

**Deliverables:**
- ✅ RabbitMQ container and configuration
- ✅ Message publishing on document upload
- ✅ OCR worker service foundation
- ✅ Layer-specific exception handling
- ✅ Structured logging implementation

**Message Flow:**
```
Document Upload → REST API → RabbitMQ (OCR Queue) → OCR Worker
                                    ↓
ElasticSearch ← GenAI Worker ← RabbitMQ (Result Queue)
```

### Sprint 4: OCR & MinIO Integration (Weeks 7-8)
**Objectives:**
- Implement OCR functionality using Tesseract
- Integrate MinIO for file storage
- Complete OCR worker service

**Deliverables:**
- ✅ Tesseract OCR integration
- ✅ MinIO file storage service
- ✅ OCR worker processing pipeline
- ✅ File retrieval and processing workflow
- ✅ Unit tests for OCR functionality

**OCR Pipeline:**
1. Document uploaded → stored in MinIO
2. OCR message sent to RabbitMQ
3. OCR Worker processes document
4. Extracted text stored in database
5. Success message sent to result queue

### Sprint 5: Generative AI Integration (Weeks 9-10)
**Objectives:**
- Integrate Google Gemini API
- Implement AI-powered document summarization
- Handle API failures and rate limiting

**Deliverables:**
- ✅ Google Gemini API integration
- ✅ GenAI worker service
- ✅ Document summarization pipeline
- ✅ API error handling and retry logic
- ✅ Summary storage and retrieval

**GenAI Workflow:**
```csharp
public class GeminiAIService : IGenAIService
{
    public async Task<string> GenerateSummaryAsync(string text)
    {
        // Implement Google Gemini API call
        // Handle rate limiting and errors
        // Return generated summary
    }
}
```

### Sprint 6: Elasticsearch & Additional Use Case (Weeks 11-12)
**Objectives:**
- Implement full-text search with Elasticsearch
- Create indexing worker service
- Develop additional use case with new entities

**Deliverables:**
- ✅ Elasticsearch integration
- ✅ Document indexing worker
- ✅ Advanced search functionality
- ✅ Additional feature: **Document Collaboration System**
  - User management entities
  - Document sharing and permissions
  - Comment system
  - Version control

**Additional Use Case: Document Collaboration**
```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<DocumentPermission> Permissions { get; set; }
}

public class DocumentComment
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int Version { get; set; }
    public string FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Sprint 7: Integration Tests & Batch Processing (Weeks 13-14)
**Objectives:**
- Implement end-to-end integration tests
- Create batch processing for access logs
- Project finalization

**Deliverables:**
- ✅ Integration test suite
- ✅ XML batch processing service
- ✅ Scheduled job for daily processing
- ✅ Final documentation and deployment

**Batch Processing Implementation:**
```csharp
public class BatchProcessingWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessDailyAccessLogs();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

## Technology Stack & Dependencies

### Frontend Technologies
- **React 18** - Modern UI library
- **TypeScript** - Type safety and better DX
- **Vite** - Fast build tool and dev server

### Backend Technologies (.NET)
- **.NET 8.0** - Latest LTS version
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - ORM
- **Docker & Docker Compose** - Containerization

### External Services
- **Nginx** - Web server and reverse proxy
- **PostgreSQL** - Primary database
- **RabbitMQ** - Message queuing
- **MinIO** - Object storage
- **Elasticsearch** - Search engine
- **Google Gemini** - AI services

### Frontend Dependencies (package.json)


### Backend NuGet Packages
```xml
<!-- Common packages for all projects -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />

<!-- REST API specific -->
<PackageReference Include="Microsoft.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="RabbitMQ.Client" Version="6.8.1" />
<PackageReference Include="Minio" Version="6.0.1" />

<!-- Services specific -->
<PackageReference Include="Tesseract" Version="5.2.0" />
<PackageReference Include="Google.Cloud.AIPlatform.V1" Version="3.1.0" />
<PackageReference Include="NEST" Version="7.17.5" />

<!-- Testing -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

## Code Quality & Architecture Requirements

### SOLID Principles Implementation
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable
- **Interface Segregation**: Clients depend only on methods they use
- **Dependency Inversion**: Depend on abstractions, not concretions

### Design Patterns
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Loose coupling
- **Factory Pattern**: Object creation
- **Observer Pattern**: Event handling
- **Strategy Pattern**: Algorithm selection

### Exception Handling Strategy
```csharp
// Layer-specific exceptions
public class BusinessException : Exception
{
    public string ErrorCode { get; }
    public BusinessException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}

// Global exception middleware
public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### Logging Strategy
- **Structured logging** with Serilog
- **Log levels**: Debug, Information, Warning, Error, Critical
- **Contextual information**: RequestId, UserId, CorrelationId
- **Performance logging**: Method execution times
- **Business event logging**: Document uploads, searches, etc.

## Testing Strategy

### Frontend Tests (React/TypeScript)
```typescript
// Component Testing with React Testing Library
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { DocumentUpload } from '../components/DocumentUpload';

describe('DocumentUpload Component', () => {
  test('renders upload area correctly', () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <DocumentUpload />
      </QueryClientProvider>
    );
    
    expect(screen.getByText(/drag.*drop.*files/i)).toBeInTheDocument();
  });
  
  test('handles file drop correctly', async () => {
    const file = new File(['test content'], 'test.pdf', { type: 'application/pdf' });
    const queryClient = new QueryClient();
    
    render(
      <QueryClientProvider client={queryClient}>
        <DocumentUpload />
      </QueryClientProvider>
    );
    
    const dropzone = screen.getByTestId('dropzone');
    fireEvent.drop(dropzone, { dataTransfer: { files: [file] } });
    
    await waitFor(() => {
      expect(screen.getByText('test.pdf')).toBeInTheDocument();
    });
  });
});

// Service Testing
import { documentService } from '../services/documentService';
import axios from 'axios';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('Document Service', () => {
  test('uploads document successfully', async () => {
    const mockResponse = { data: { id: 1, filename: 'test.pdf' } };
    mockedAxios.post.mockResolvedValue(mockResponse);
    
    const file = new File(['test'], 'test.pdf');
    const result = await documentService.upload(file);
    
    expect(result.id).toBe(1);
    expect(mockedAxios.post).toHaveBeenCalledWith('/api/documents', expect.any(FormData));
  });
});
```

### Backend Unit Tests (>70% Coverage)
```csharp
[Fact]
public async Task UploadDocument_ValidDocument_ReturnsSuccess()
{
    // Arrange
    var mockRepo = new Mock<IDocumentRepository>();
    var mockQueue = new Mock<IQueueService>();
    var service = new DocumentService(mockRepo.Object, mockQueue.Object);
    
    // Act
    var result = await service.UploadDocumentAsync(validDocument);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    mockQueue.Verify(q => q.SendMessageAsync(It.IsAny<OcrMessage>()), Times.Once);
}
```

### Integration Tests
```csharp
[Fact]
public async Task DocumentUploadWorkflow_EndToEnd_Success()
{
    // Arrange - Use TestServer and in-memory database
    
    // Act - Upload document through REST API
    
    // Assert - Verify document is processed through entire pipeline
}
```

## Docker Configuration

### docker-compose.yml Structure 简略版本
```yaml
version: '3.8'
services:
  paperless-webui:
    build: ./PaperlessWebUI
    ports:
      - "80:80"
    depends_on:
      - paperless-api
      
  paperless-api:
    build: ./PaperlessRESTAPI
    ports:
      - "8081:8081"
    depends_on:
      - postgresql
      - rabbitmq
      - minio
      
  paperless-services:
    build: ./PaperlessServices
    ports:
      - "8082:8082"
    depends_on:
      - rabbitmq
      - minio
      - elasticsearch
      
  postgresql:
    image: postgres:16
    environment:
      POSTGRES_DB: paperless
      POSTGRES_USER: paperless
      POSTGRES_PASSWORD: paperless
    ports:
      - "5432:5432"
      
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
      
  minio:
    image: minio/minio
    command: server /data --console-address ":9090"
    ports:
      - "9000:9000"
      - "9090:9090"
      
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    ports:
      - "9200:9200"
      
  adminer:
    image: adminer
    ports:
      - "9091:8080"
```

## Development Workflow

### Git Strategy
- **Main branch**: Production-ready code
- **Develop branch**: Integration branch
- **Feature branches**: Individual features
- **Release branches**: Preparing releases
- **Hotfix branches**: Critical fixes

### Branch Naming Convention
- `feature/sprint-X-feature-name`
- `bugfix/issue-description`
- `hotfix/critical-issue`
- `release/vX.X.X`

### Commit Message Format
```
type(scope): description

[optional body]

[optional footer(s)]
```

Example:
```
feat(api): add document upload endpoint

Implement POST /api/documents endpoint with file validation
and RabbitMQ message publishing

Closes #123
```

### Pull Request Process
1. Create feature branch from develop
2. Implement feature with tests
3. Ensure all tests pass and coverage >70%
4. Create pull request to develop
5. Code review by team members
6. Merge after approval

## CI/CD Pipeline (GitHub Actions)

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
...
```

## Documentation Requirements

### README.md
- Project overview and architecture
- Quick start guide
- Development setup instructions
- API documentation links
- Testing instructions

### API Documentation
- OpenAPI/Swagger integration
- Endpoint descriptions
- Request/response examples
- Authentication requirements

### Architecture Documentation
- System architecture diagrams
- Database schema
- Message flow diagrams
- Deployment architecture

## Risk Management

### Technical Risks
1. **OCR Accuracy**: Tesseract may not work well with all document types
   - *Mitigation*: Implement multiple OCR engines, add manual correction
2. **AI API Limits**: Google Gemini rate limiting
   - *Mitigation*: Implement retry logic, caching, fallback summaries
3. **Performance**: Large file processing may be slow
   - *Mitigation*: Async processing, file size limits, progress indicators

### Project Risks
1. **Scope Creep**: Additional features beyond requirements
   - *Mitigation*: Strict sprint planning, regular reviews
2. **Integration Complexity**: Multiple services coordination
   - *Mitigation*: Early integration testing, service contracts
3. **Team Coordination**: Multiple team members working on same codebase
   - *Mitigation*: Clear Git workflow, code reviews, daily standups

## Evaluation Criteria Mapping

### Functional Requirements (20%)
- ✅ **Use Cases (5%)**: All 4 use cases implemented and tested
- ✅ **REST API (5%)**: Complete API with proper service layers
- ✅ **Web Frontend (5%)**: Modern, responsive UI with full functionality
- ✅ **Additional Use Case (5%)**: Document collaboration system

### Non-Functional Requirements (30%)
- ✅ **Queues (4%)**: RabbitMQ async communication
- ✅ **Logging (2%)**: Comprehensive logging across all components
- ✅ **Validation (2%)**: Input validation in all layers
- ✅ **Stability Patterns (2%)**: Exception handling and resilience
- ✅ **Unit Tests (4%)**: >70% coverage with mocking
- ✅ **Integration Tests (4%)**: End-to-end workflow testing
- ✅ **Clean Code (2%)**: SOLID principles and code quality

### Software Architecture (30%)
- ✅ **Packaging (10%)**: Docker containerization and configuration
- ✅ **Loose Coupling (2%)**: Interface-based design
- ✅ **Mapper (2%)**: AutoMapper between layers
- ✅ **Dependency Injection (2%)**: Built-in DI container
- ✅ **DAL (2%)**: Repository pattern with EF Core
- ✅ **BL (2%)**: Clean business logic layer

### Software Development Workflow (20%)
- ✅ **GitFlow (5%)**: Proper branching and pull requests
- ✅ **Issue Tracking (5%)**: GitHub Issues and project boards
- ✅ **CI/CD Pipelines (5%)**: GitHub Actions automation
- ✅ **Documentation (5%)**: Comprehensive project documentation

## Timeline Summary

| Sprint | Duration | Focus Area | Key Deliverables |
|--------|----------|------------|------------------|
| 1 | Weeks 1-2 | Foundation | REST API, DAL, Docker setup |
| 2 | Weeks 3-4 | Frontend | Web UI, Nginx integration |
| 3 | Weeks 5-6 | Messaging | RabbitMQ, Exception handling |
| 4 | Weeks 7-8 | Processing | OCR, MinIO integration |
| 5 | Weeks 9-10 | AI | GenAI integration, Summarization |
| 6 | Weeks 11-12 | Search | Elasticsearch, Additional use case |
| 7 | Weeks 13-14 | Finalization | Integration tests, Batch processing |

## Success Metrics

- ✅ All sprint deliverables completed on time
- ✅ Code coverage >70% maintained throughout
- ✅ All containers start successfully with `docker-compose up`
- ✅ Complete document workflow: Upload → OCR → AI Summary → Search
- ✅ Additional collaboration features working
- ✅ Integration tests passing
- ✅ Documentation complete and up-to-date

---

*This project plan is designed to meet all evaluation criteria while maintaining high code quality and following software engineering best practices. Regular sprint reviews and adjustments will ensure successful delivery.*
