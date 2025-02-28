# docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443

# استخدم .NET SDK للبناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات المشروع إلى الحاوية
COPY ../ConfigManager.sln ./
COPY ../ConfigManager.API/*.csproj ./ConfigManager.API/
COPY ../ConfigManager.Application/*.csproj ./ConfigManager.Application/
COPY ../ConfigManager.Domain/*.csproj ./ConfigManager.Domain/
COPY ../ConfigManager.Infrastructure/*.csproj ./ConfigManager.Infrastructure/
COPY ../ConfigManager.Client/*.csproj ./ConfigManager.Client/
# استعادة الحزم (يجب أن يتم تنفيذها من نفس المسار الذي يحتوي على الحلّ)
RUN dotnet restore ConfigManager.sln

# نسخ باقي الأكواد المصدرية بعد الاستعادة لتسريع عمليات الـ Docker caching
COPY ../ConfigManager.API ./ConfigManager.API/
COPY ../ConfigManager.Application ./ConfigManager.Application/
COPY ../ConfigManager.Domain ./ConfigManager.Domain/
COPY ../ConfigManager.Infrastructure ./ConfigManager.Infrastructure/

# بناء التطبيق
WORKDIR /src/ConfigManager.API
RUN dotnet build ConfigManager.API.csproj -o /app/build

# نشر التطبيق
FROM build AS publish
RUN dotnet publish /src/ConfigManager.API/ConfigManager.API.csproj -o /app/publish

# تشغيل التطبيق
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "ConfigManager.API.dll"]
