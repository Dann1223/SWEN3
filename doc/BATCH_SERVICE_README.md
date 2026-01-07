# Paperless Batch Processing Service

## Overview

The Paperless Batch Processing Service is a scheduled service that processes daily XML access log files from external systems. It reads XML files containing document access statistics and stores them in the PostgreSQL database for analytics and reporting.

## Features

- **Scheduled Processing**: Runs automatically based on configurable cron schedule (default: daily at 1:00 AM)
- **XML File Processing**: Parses structured XML files containing document access statistics
- **Duplicate Prevention**: Tracks processed files using checksums to prevent reprocessing
- **File Archiving**: Automatically moves processed files to archive or error folders
- **Comprehensive Logging**: Detailed logging with Serilog for monitoring and debugging
- **Database Integration**: Stores daily access statistics in PostgreSQL with proper entity relationships

## Architecture Integration

The batch service integrates with the existing Paperless DMS architecture:

- **Database**: Uses the same PostgreSQL database as the main API
- **Entities**: Adds new entities (`DailyDocumentAccess`, `BatchProcessingHistory`) via Entity Framework migrations
- **Configuration**: Follows the same configuration patterns as other services
- **Docker**: Deployed as a separate container with shared volumes for file processing

## XML File Format

### Input XML Structure

```xml
<?xml version="1.0" encoding="UTF-8"?>
<AccessLogReport version="1.0" date="2024-12-15" system="External-System-1">
  
  <DocumentAccess documentId="1" fileName="technical_report_2024.pdf">
    <ViewCount>15</ViewCount>
    <DownloadCount>8</DownloadCount>
    <SearchCount>3</SearchCount>
    <TotalAccess>26</TotalAccess>
    <AccessDetails>
      <AccessDetail time="09:15:30" action="view" userAgent="Mozilla/5.0" ipAddress="192.168.1.100" />
      <AccessDetail time="10:22:45" action="download" userAgent="Mozilla/5.0" ipAddress="192.168.1.101" />
    </AccessDetails>
  </DocumentAccess>
  
  <!-- Additional document access records -->
  
</AccessLogReport>
```

### XML Schema Description

- **AccessLogReport**: Root element with version, date, and source system attributes
- **DocumentAccess**: Individual document statistics with document ID and optional filename
- **ViewCount**: Number of document views
- **DownloadCount**: Number of document downloads  
- **SearchCount**: Number of times document appeared in search results
- **TotalAccess**: Total access count across all actions
- **AccessDetails**: Optional detailed access information (currently logged but not persisted)

## Configuration

### Environment Variables

```bash
# Database connection
ConnectionStrings__DefaultConnection=Host=paperless-db;Port=5432;Database=paperless;Username=paperless;Password=paperless123

# Batch processing settings
BatchProcessing__InputFolder=/app/batch/input
BatchProcessing__ArchiveFolder=/app/batch/archive
BatchProcessing__ErrorFolder=/app/batch/error
BatchProcessing__FilePattern=access_log_*.xml
BatchProcessing__CronSchedule=0 1 * * *
BatchProcessing__IsEnabled=true
BatchProcessing__MaxFilesPerBatch=100
BatchProcessing__MaxFileSizeBytes=52428800
```

### Cron Schedule Examples

- `0 1 * * *` - Daily at 1:00 AM
- `0 */6 * * *` - Every 6 hours
- `0 0 * * 0` - Weekly on Sunday at midnight
- `0 2 1 * *` - Monthly on the 1st at 2:00 AM

## File Processing Workflow

1. **File Discovery**: Service scans input folder for files matching the configured pattern
2. **Duplicate Check**: Verifies file hasn't been processed using filename and checksum
3. **XML Parsing**: Deserializes XML into strongly-typed DTOs
4. **Data Validation**: Validates date format and document IDs against existing documents
5. **Database Operations**: Upserts daily access statistics with transaction support
6. **History Recording**: Records processing success/failure with detailed metadata
7. **File Archiving**: Moves file to appropriate folder (archive/error) with timestamp

## Database Schema

### DailyDocumentAccess Table

```sql
CREATE TABLE "DailyDocumentAccesses" (
    "Id" SERIAL PRIMARY KEY,
    "DocumentId" INTEGER NOT NULL REFERENCES "Documents"("Id") ON DELETE CASCADE,
    "AccessDate" DATE NOT NULL,
    "ViewCount" INTEGER NOT NULL DEFAULT 0,
    "DownloadCount" INTEGER NOT NULL DEFAULT 0,
    "SearchCount" INTEGER NOT NULL DEFAULT 0,
    "TotalAccess" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    UNIQUE("DocumentId", "AccessDate")
);
```

### BatchProcessingHistory Table

```sql
CREATE TABLE "BatchProcessingHistories" (
    "Id" SERIAL PRIMARY KEY,
    "FileName" VARCHAR(500) NOT NULL,
    "FilePath" VARCHAR(1000) NOT NULL,
    "ProcessedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "IsSuccessful" BOOLEAN NOT NULL,
    "ErrorMessage" VARCHAR(2000),
    "RecordsProcessed" INTEGER NOT NULL DEFAULT 0,
    "FileSizeBytes" BIGINT NOT NULL,
    "FileChecksum" VARCHAR(64)
);
```

## Deployment

### Docker Compose

The service is included in the main docker-compose.yml:

```yaml
paperless-batch:
  build:
    context: .
    dockerfile: PaperlessBatchService/Dockerfile
  container_name: paperless-batch
  depends_on:
    postgresql:
      condition: service_healthy
  environment:
    - DOTNET_ENVIRONMENT=Production
    - ConnectionStrings__DefaultConnection=Host=paperless-db;Port=5432;Database=paperless;Username=paperless;Password=paperless123
  volumes:
    - batch_input:/app/batch/input
    - batch_archive:/app/batch/archive
    - batch_error:/app/batch/error
    - batch_logs:/app/logs
  restart: unless-stopped
```

### Manual Deployment

1. Build the service:
   ```bash
   cd PaperlessBatchService
   dotnet publish -c Release -o ./publish
   ```

2. Run with configuration:
   ```bash
   dotnet PaperlessBatchService.dll
   ```

## Monitoring and Troubleshooting

### Logs

Logs are written to both console and files:
- Console: Real-time monitoring
- Files: `logs/batch-service-YYYY-MM-DD.txt` with daily rotation

### Key Log Events

- Service startup/shutdown
- File discovery and processing
- Database operations
- Error conditions and stack traces
- Performance metrics (processing time, record counts)

### Health Checks

Monitor the service health by checking:
- Log files for recent activity
- Docker container status: `docker ps | grep paperless-batch`
- Processing history in database
- File counts in input/archive/error folders

### Common Issues

1. **Files not processing**: Check folder permissions and file pattern matching
2. **Database connection errors**: Verify connection string and database availability
3. **XML parsing errors**: Validate XML structure against schema
4. **Duplicate document IDs**: Ensure external system uses valid document IDs from the Paperless database

## Sample Files

Sample XML files are provided in the `sample_files/` directory:
- `access_log_20241215.xml` - Complete example with detailed access information
- `access_log_20241214.xml` - Simplified example with basic statistics

## API Integration

While the batch service operates independently, the processed data can be accessed via the main API:

- Daily statistics endpoints (to be implemented)
- Document access history queries
- Batch processing status monitoring

## Future Enhancements

- Web-based monitoring dashboard
- Email notifications for processing failures
- Support for additional XML schemas
- Real-time file processing via file system watchers
- Data aggregation and reporting APIs
