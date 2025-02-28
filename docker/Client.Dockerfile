# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ../ConfigManager.sln ./
COPY ../ConfigManager.Domain ./ConfigManager.Domain
COPY ../ConfigManager.Application ./ConfigManager.Application
COPY ../ConfigManager.Infrastructure ./ConfigManager.Infrastructure
COPY ../ConfigManager.Client ./ConfigManager.Client

WORKDIR /app/ConfigManager.Client
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# مرحلة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# تأكد من أن الملفات تنسخ بشكل صحيح
COPY --from=build /app/publish /app

# نقطة الدخول
ENTRYPOINT ["dotnet", "/app/ConfigManager.Client.dll"]
