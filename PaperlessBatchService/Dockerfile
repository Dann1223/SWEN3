# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY PaperlessBatchService/PaperlessBatchService.csproj PaperlessBatchService/

# Restore dependencies for batch service
RUN dotnet restore PaperlessBatchService/PaperlessBatchService.csproj

# Copy batch service source code
COPY PaperlessBatchService/ PaperlessBatchService/

# Build application only for batch service
WORKDIR /src/PaperlessBatchService
RUN dotnet build -c Release -o /app/build

# Publish application
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create batch processing directories
RUN mkdir -p /app/batch/input /app/batch/archive /app/batch/error /app/logs

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PaperlessBatchService.dll"]
