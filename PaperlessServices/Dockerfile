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

# Switch to root user to install packages
USER root
WORKDIR /app

# Install Tesseract OCR and required packages
RUN apt-get update && apt-get install -y --allow-unauthenticated \
    tesseract-ocr \
    tesseract-ocr-eng \
    libtesseract-dev \
    libleptonica-dev \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Create symbolic links to fix Tesseract library loading issues
# Handle different architectures (x86_64 vs aarch64)
RUN if [ -f /usr/lib/x86_64-linux-gnu/libdl.so.2 ]; then \
        ln -sf /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so; \
    elif [ -f /usr/lib/aarch64-linux-gnu/libdl.so.2 ]; then \
        ln -sf /usr/lib/aarch64-linux-gnu/libdl.so.2 /usr/lib/aarch64-linux-gnu/libdl.so; \
    fi

# Create x64 directory for native libraries
RUN mkdir -p /app/x64

# Create symbolic links for Tesseract and Leptonica libraries (handle both architectures)
RUN if [ -f /usr/lib/x86_64-linux-gnu/liblept.so.5 ]; then \
        ln -sf /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so; \
    elif [ -f /usr/lib/aarch64-linux-gnu/liblept.so.5 ]; then \
        ln -sf /usr/lib/aarch64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so; \
    fi

RUN if [ -f /usr/lib/x86_64-linux-gnu/libtesseract.so.5 ]; then \
        ln -sf /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so; \
    elif [ -f /usr/lib/x86_64-linux-gnu/libtesseract.so.4 ]; then \
        ln -sf /usr/lib/x86_64-linux-gnu/libtesseract.so.4 /app/x64/libtesseract50.so; \
    elif [ -f /usr/lib/aarch64-linux-gnu/libtesseract.so.5 ]; then \
        ln -sf /usr/lib/aarch64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so; \
    elif [ -f /usr/lib/aarch64-linux-gnu/libtesseract.so.4 ]; then \
        ln -sf /usr/lib/aarch64-linux-gnu/libtesseract.so.4 /app/x64/libtesseract50.so; \
    fi

# Create tessdata directory and download additional language data if needed
RUN mkdir -p /app/tessdata
RUN cp -r /usr/share/tesseract-ocr/5/tessdata/* /app/tessdata/ || \
    cp -r /usr/share/tesseract-ocr/4.00/tessdata/* /app/tessdata/

# Clean up to reduce image size
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create directories for logs and uploads
RUN mkdir -p /app/logs
RUN mkdir -p /app/uploads

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV TESSDATA_PREFIX=/app/tessdata

# Switch back to non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "PaperlessServices.dll"]
