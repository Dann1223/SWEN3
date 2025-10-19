# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY PaperlessRESTAPI.csproj ./
RUN dotnet restore

# Copy source code and build application
COPY . ./
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copy published application from build stage
COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Expose port 8081
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8081

# Start the application
ENTRYPOINT ["dotnet", "PaperlessRESTAPI.dll"]
