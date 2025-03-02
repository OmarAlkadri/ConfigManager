# Client.Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build  

WORKDIR /src

COPY ../ConfigManager.Client/*.csproj ./ConfigManager.Client/
COPY ../ConfigManager.Domain/*.csproj ./ConfigManager.Domain/
COPY ../ConfigManager.Application/*.csproj ./ConfigManager.Application/
COPY ../ConfigManager.Infrastructure/*.csproj ./ConfigManager.Infrastructure/

RUN dotnet restore "./ConfigManager.Client/ConfigManager.Client.csproj"

COPY . . 

RUN dotnet publish "./ConfigManager.Client/ConfigManager.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS base
WORKDIR /app

COPY ./docker/nginx.conf /etc/nginx/nginx.conf

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

EXPOSE 80

FROM base AS final
WORKDIR /app

RUN echo "daemon off;" >> /etc/nginx/nginx.conf

CMD ["nginx"]
