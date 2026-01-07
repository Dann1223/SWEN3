# Document Management System (DMS)

## Project Overview

A complete microservices-based Document Management System w### Development Mode

For frontend development with hot reload:

```bash
cd PaperlessWebUI
npm install
npm run dev
```

### Running Tests

```bash
# All tests (both API and Services)
dotnet test

# API tests only
cd PaperlessRESTAPI.Tests
dotnet test

# Services tests only  
cd PaperlessServices.Tests
dotnet test

# Run with coverage reporting
dotnet test --collect:"XPlat Code Coverage"
```

### OCR Processing

Upload documents through the Web UI or REST API to trigger automatic OCR processing:

1. **File Upload**: Documents are stored in MinIO object storage
2. **Queue Processing**: OCR jobs are queued via RabbitMQ  
3. **Text Extraction**: Tesseract OCR extracts text from images/PDFs
4. **Search Integration**: Extracted text enables full-text search
5. **Background Processing**: Services handle OCR asynchronouslyMinIO object storage, message queuing, and modern React frontend.

## 🚀 Sprint 4 Achievements ✅

### 1. OCR Integration with Tesseract 5.2.0
- ✅ **Real OCR Processing** - Tesseract 5.2.0 integration for text extraction
- ✅ **Multi-language Support** - English and Chinese text recognition
- ✅ **PDF & Image Processing** - Support for PDF, PNG, JPG, TIFF formats
- ✅ **Async Processing** - Background OCR worker with RabbitMQ messaging
- ✅ **Error Handling** - Robust error handling and retry mechanisms

### 2. MinIO Object Storage
- ✅ **File Storage** - MinIO 6.0.1 for scalable document storage
- ✅ **Bucket Management** - Automatic bucket creation and organization
- ✅ **Secure Access** - Pre-signed URLs for secure file access
- ✅ **Health Monitoring** - Storage service health checks
- ✅ **Docker Integration** - Containerized MinIO deployment

### 3. Message Queue Architecture
- ✅ **RabbitMQ Integration** - Reliable message passing between services
- ✅ **OCR Queue** - Dedicated queue for OCR processing tasks
- ✅ **GenAI Queue** - Ready for AI summarization features
- ✅ **Indexing Queue** - For search index updates
- ✅ **Connection Management** - Auto-recovery and health monitoring

### 4. Enhanced Testing Suite
- ✅ **67 Total Tests** - Comprehensive test coverage
- ✅ **PaperlessRESTAPI.Tests** - 45 tests covering all API components
- ✅ **PaperlessServices.Tests** - 22 tests for worker services
- ✅ **Mock Services** - Isolated testing without external dependencies
- ✅ **Code Coverage** - 20%+ coverage with detailed reporting
- ✅ **Continuous Integration** - Ready for CI/CD pipelines

### 5. Docker Orchestration
- ✅ **Complete Stack** - All services containerized
- ✅ **Health Checks** - Comprehensive health monitoring
- ✅ **Service Dependencies** - Proper startup order and dependencies
- ✅ **Volume Management** - Persistent storage for databases and files
- ✅ **Network Isolation** - Secure inter-service communication

## Previous Sprint Achievements

### Sprint 1 ✅ - Foundation
- Project structure, database, REST API, basic testing

### Sprint 2 ✅ - Frontend  
- React TypeScript frontend with modern UI components

### Sprint 3 ✅ - Integration

- ✅ **Modern React 18 + TypeScript** - Latest React with strict TypeScript
- ✅ **Vite Build System** - Fast development and optimized production builds
- ✅ **Ant Design UI Library** - Complete component library for professional UI
- ✅ **Redux Toolkit** - State management with document, search, and UI slices
- ✅ **React Router** - Client-side routing with protected routes

### 2. Core UI Components

- ✅ **Dashboard Page** - Document statistics and recent documents overview
- ✅ **Documents Management** - List/grid view with pagination and filtering
- ✅ **Document Upload** - Drag-and-drop file upload with progress tracking
- ✅ **Search Interface** - Debounced search with real-time results
- ✅ **Main Layout** - Responsive navigation with sidebar and header

### 3. Frontend Architecture

- ✅ **Component Architecture** - Reusable components with proper separation
- ✅ **Custom Hooks** - useDocuments, useSearch for business logic
- ✅ **API Services** - Axios-based API client with error handling
- ✅ **Type Safety** - Comprehensive TypeScript interfaces and types
- ✅ **Error Boundaries** - Global error handling and user feedback

### 4. Docker & Production Setup

- ✅ **Multi-stage Docker Build** - Optimized production image with Nginx
- ✅ **Nginx Configuration** - SPA routing and API proxy setup
- ✅ **Docker Compose Integration** - Full stack deployment
- ✅ **Production Ready** - Gzip compression, security headers, and logging

### 5. Performance & UX

- ✅ **Infinite Loop Bug Fix** - Resolved useEffect dependency issues
- ✅ **Debounced Search** - Optimized search performance
- ✅ **Loading States** - User feedback during API operations
- ✅ **Responsive Design** - Mobile-friendly interface with Ant Design

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop  
- Node.js 18+ (for frontend development)
- Tesseract OCR 5.2.0 (included in Docker images)

### Running the Complete Application

1. **Start All Services with Docker Compose:**

   ```bash
   docker-compose up --build
   ```

   This will start:
   - PostgreSQL Database with health checks
   - MinIO Object Storage for document files
   - RabbitMQ Message Queue for OCR processing
   - REST API with OCR integration
   - Background Services for async processing
   - React Frontend with modern UI

2. **Or Start Services Individually:**

   ```bash
   # Start core infrastructure
   docker-compose up -d postgresql minio rabbitmq
   
   # Start API services
   docker-compose up -d paperless-api paperless-services
   
   # Start WebUI
   docker-compose up -d paperless-webui
   ```

3. **Access Services:**

   - **Web Application**: http://localhost:8080
   - **REST API**: http://localhost:8081  
   - **API Documentation**: http://localhost:8081/swagger
   - **Health Check**: http://localhost:8081/health
   - **Database Admin**: http://localhost:9091 (Adminer)
   - **MinIO Console**: http://localhost:9001 (admin/minioadmin)
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Development Mode

For frontend development with hot reload:

```bash
cd PaperlessWebUI
npm install
npm run dev
```

### Testing

```bash
# Backend tests
cd PaperlessRESTAPI.Tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## API Endpoints

### Documents

- `GET /api/documents` - Get all documents
- `GET /api/documents/{id}` - Get document by ID
- `POST /api/documents` - Upload new document
- `PUT /api/documents/{id}` - Update document
- `DELETE /api/documents/{id}` - Delete document
- `GET /api/documents/search?query=text` - Search documents
- `GET /api/documents/recent?count=10` - Get recent documents

### Tags

- `GET /api/tags` - Get all tags
- `GET /api/tags/{id}` - Get tag by ID
- `POST /api/tags` - Create new tag
- `PUT /api/tags/{id}` - Update tag
- `DELETE /api/tags/{id}` - Delete tag

## Database Schema

### Documents Table

- Id, Title, FileName, FilePath
- UploadDate, LastModified, FileType, FileSize
- OcrText, Summary, IsProcessed, IsIndexed

### Tags Table

- Id, Name, Description, Color, CreatedDate

### DocumentTags Table (Many-to-Many)

- DocumentId, TagId

### DocumentAccesses Table

- Id, DocumentId, AccessDate, UserAgent, IpAddress, ActionType

## Technology Stack

### Frontend
- **React 18** - Modern UI library with hooks and concurrent features
- **TypeScript** - Type safety and enhanced developer experience
- **Vite** - Fast build tool and development server
- **Ant Design** - Professional UI component library
- **Redux Toolkit** - Predictable state management
- **React Router** - Client-side routing
- **Axios** - HTTP client for API communication

### Backend
- **.NET 8.0** - Latest LTS backend framework
- **ASP.NET Core** - High-performance web API
- **Entity Framework Core** - Object-relational mapping
- **PostgreSQL** - Robust relational database
- **FluentValidation** - Input validation
- **AutoMapper** - Object-to-object mapping
- **Swagger/OpenAPI** - API documentation

### DevOps & Infrastructure
- **Docker** - Containerization platform
- **Docker Compose** - Multi-container orchestration
- **Nginx** - High-performance web server and reverse proxy
- **Multi-stage Builds** - Optimized container images

### Testing
- **xUnit** - .NET testing framework
- **Moq** - Mocking framework for unit tests
- **FluentAssertions** - Expressive test assertions
- **In-Memory Database** - Isolated testing environment

## Next Steps (Sprint 3)

- 🔄 **RabbitMQ Integration** - Message queuing for async processing
- 🔄 **OCR Worker Service** - Document text extraction
- 🔄 **Background Processing** - Async document processing pipeline
- 🔄 **MinIO Integration** - Object storage for files
- 🔄 **Enhanced Error Handling** - Comprehensive exception management

## Code Quality Metrics

### Sprint 1 & 2 Combined
- ✅ **Test Coverage**: 22+ unit tests covering core functionality
- ✅ **SOLID Principles**: Applied throughout codebase
- ✅ **Clean Architecture**: Separated concerns with proper layering
- ✅ **Error Handling**: Comprehensive exception management
- ✅ **Logging**: Structured logging in all components
- ✅ **Validation**: Input validation on all endpoints
- ✅ **Type Safety**: Full TypeScript implementation
- ✅ **Performance**: Optimized builds and efficient state management
- ✅ **Security**: CORS configuration and security headers

## Architecture Highlights

### Frontend Architecture
```
src/
├── components/     # Reusable UI components
├── pages/         # Route-level page components  
├── hooks/         # Custom React hooks
├── services/      # API service layer
├── store/         # Redux state management
├── types/         # TypeScript definitions
└── utils/         # Helper functions
```

### Backend Architecture
```
PaperlessRESTAPI/
├── Controllers/   # API endpoints
├── Services/      # Business logic layer
├── Data/          # Data access layer
├── Models/        # DTOs and view models
├── Infrastructure/ # Cross-cutting concerns
└── Configuration/ # App configuration
```

### Container Architecture
```
Docker Compose Services:
├── paperless-webui (Port 8080)   # React + Nginx
├── paperless-api (Port 8081)     # .NET Web API  
├── postgresql (Port 5432)        # Database
└── adminer (Port 9091)           # DB Admin
```
