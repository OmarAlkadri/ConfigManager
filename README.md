# ConfigManager - Configuration Management System

## 📌 Introduction  
ConfigManager is a dynamic configuration management system based on **.NET 8** with **Redis and RabbitMQ** to support real-time configuration updates.  
It provides a **REST API** for managing configurations and follows **DDD, Clean Code, and TDD** principles to ensure high code quality.

---

## 🛠️ Technologies Used  
✅ **.NET 8 Web API**  
✅ **Redis** for caching configurations  
✅ **RabbitMQ** for sending update notifications  
✅ **Docker & Docker-Compose** for environment setup  
✅ **xUnit and Moq** for testing  

---

## 🚀 Running the Project Locally  
### **1️⃣ Prerequisites**  
- **Docker & Docker Compose**  
- **.NET 8 SDK** (if you want to run it without Docker)  

### **2️⃣ Run the Project with Docker**  
```sh
docker-compose up --build
```
The ConfigManager API will be available at **http://localhost:5000/swagger**  
RabbitMQ management UI can be accessed at **http://localhost:15672**  
- **Username:** guest  
- **Password:** guest  

### **3️⃣ Run the Project Manually without Docker**  
```sh
dotnet run --project src/ConfigManager.API
```

## 📡 Using the API via Swagger  
You can explore the API through **http://localhost:5000/swagger**, where the following endpoints are available:

🔹 Retrieve all configurations for a specific application  
```bash
GET /api/configurations/{applicationName}
```
🔹 Retrieve a specific configuration by name  
```bash
GET /api/configurations/{applicationName}/{name}
```
🔹 Add or update a configuration  
```bash
POST /api/configurations
Content-Type: application/json
{
  "name": "SiteName",
  "type": "String",
  "value": "example.com",
  "isActive": true,
  "applicationName": "SERVICE-A"
}
```
🔹 Delete a specific configuration  
```bash
DELETE /api/configurations/{id}
```

## 🧪 Running Tests  
To execute tests, run the following command:  
```sh
dotnet test
```

## 📜 License  
This project is open-source and can be modified freely.

---

## **2️⃣ Share the Project on GitHub**  
### **🔹 Create a New Repository on GitHub**  
1. Go to [GitHub](https://github.com/) and create a new repository.  
2. Copy the repository URL.  

### **🔹 Push the Project to GitHub**  
```sh
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/ConfigManager.git
git push -u origin main
```
✅ Now, the project is available on GitHub!