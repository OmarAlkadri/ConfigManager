# Build stage: Build the Blazor app using .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build  
# Set the working directory for the build
WORKDIR /src

# Copy the project files and restore dependencies
COPY ../ConfigManager.Client/*.csproj ./ConfigManager.Client/

# Restore the dependencies (restore packages)
RUN dotnet restore "./ConfigManager.Client/ConfigManager.Client.csproj"

# Copy the rest of the source code
COPY . . 

# Build and publish the Blazor client app to the /publish folder
RUN dotnet publish "./ConfigManager.Client/ConfigManager.Client.csproj" -c Release -o /app/publish

# Base stage: Setting up the base container with Nginx
FROM nginx:alpine AS base
WORKDIR /app

# Copy the custom Nginx configuration file into the container
COPY ./docker/nginx.conf /etc/nginx/nginx.conf

# Copy the published Blazor client app from the build stage to Nginx folder in the container
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Expose port 80 for Nginx
EXPOSE 80

# Final stage: Running Nginx to serve the Blazor app
FROM base AS final
WORKDIR /app

# Set Nginx to serve static files (HTML, CSS, JS, etc.)
RUN echo "daemon off;" >> /etc/nginx/nginx.conf

# Run Nginx in the background
CMD ["nginx"]
