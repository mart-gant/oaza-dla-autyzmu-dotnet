# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY OazaDlaAutyzmu.slnx .
COPY src/OazaDlaAutyzmu.Domain/OazaDlaAutyzmu.Domain.csproj src/OazaDlaAutyzmu.Domain/
COPY src/OazaDlaAutyzmu.Application/OazaDlaAutyzmu.Application.csproj src/OazaDlaAutyzmu.Application/
COPY src/OazaDlaAutyzmu.Infrastructure/OazaDlaAutyzmu.Infrastructure.csproj src/OazaDlaAutyzmu.Infrastructure/
COPY src/OazaDlaAutyzmu.Web/OazaDlaAutyzmu.Web.csproj src/OazaDlaAutyzmu.Web/

# Restore dependencies
RUN dotnet restore src/OazaDlaAutyzmu.Web/OazaDlaAutyzmu.Web.csproj

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/src/OazaDlaAutyzmu.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install SQLite (for runtime)
RUN apt-get update && apt-get install -y sqlite3 libsqlite3-dev && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Expose port
EXPOSE 80
EXPOSE 443

# Run application
ENTRYPOINT ["dotnet", "OazaDlaAutyzmu.Web.dll"]
