FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS base
WORKDIR /app
EXPOSE 443
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .  

WORKDIR /src

RUN dotnet restore ConfigManager.sln

WORKDIR /src/ConfigManager.API
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish /src/ConfigManager.API/ConfigManager.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "ConfigManager.API.dll"]
