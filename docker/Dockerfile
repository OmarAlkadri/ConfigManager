# docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../ConfigManager.sln ./
COPY ../ConfigManager.API/*.csproj ./ConfigManager.API/
COPY ../ConfigManager.Application/*.csproj ./ConfigManager.Application/
COPY ../ConfigManager.Domain/*.csproj ./ConfigManager.Domain/
COPY ../ConfigManager.Infrastructure/*.csproj ./ConfigManager.Infrastructure/
COPY ../ConfigManager.Client/*.csproj ./ConfigManager.Client/

RUN dotnet restore ConfigManager.sln

COPY ../ConfigManager.API ./ConfigManager.API/
COPY ../ConfigManager.Application ./ConfigManager.Application/
COPY ../ConfigManager.Domain ./ConfigManager.Domain/
COPY ../ConfigManager.Infrastructure ./ConfigManager.Infrastructure/

WORKDIR /src/ConfigManager.API
RUN dotnet build ConfigManager.API.csproj -o /app/build

FROM build AS publish
RUN dotnet publish /src/ConfigManager.API/ConfigManager.API.csproj -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "ConfigManager.API.dll"]
