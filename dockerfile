# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Geology_Api.csproj", "./"]
RUN dotnet restore

# Copy the entire project and build the application
COPY . .
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Production-ready runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Copy the published app to the runtime container
COPY --from=publish /app/publish .

# Expose ports for HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# Set environment variables for URLs and HTTPS port
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_ENVIRONMENT="Production"

# Disable HTTPS certificate validation in container (use real certificates in production)
# ENV DOTNET_SYSTEM_NET_HTTP_USESSLSTREAM=false

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "Geology_Api.dll"]
