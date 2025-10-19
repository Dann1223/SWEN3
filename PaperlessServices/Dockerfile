# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY PaperlessServices.csproj ./
RUN dotnet restore

# Copy source code
COPY . ./

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install Tesseract OCR and required packages
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    libtesseract-dev \
    libleptonica-dev \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Create tessdata directory and download additional language data if needed
RUN mkdir -p /app/tessdata
RUN cp -r /usr/share/tesseract-ocr/5/tessdata/* /app/tessdata/

# Copy published app
COPY --from=build /app/publish .

# Create directories for logs and uploads
RUN mkdir -p /app/logs
RUN mkdir -p /app/uploads

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TESSDATA_PREFIX=/app/tessdata

# Start the application
ENTRYPOINT ["dotnet", "PaperlessServices.dll"]
