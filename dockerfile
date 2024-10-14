# Stage 1: Build the application
FROM bitnami/aspnet-core:latest AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/Geology_Api
COPY ["./Geology_Api.csproj", "./"]
RUN dotnet restore "./Geology_Api.csproj"

# Copy the rest of the application
COPY . .

# Build the application
RUN dotnet build "./Geology_Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Geology_Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Geology_Api.dll"]
