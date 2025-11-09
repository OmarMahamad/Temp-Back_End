# 🚀 Temp-Back_End - Production Ready API Template

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Status](https://img.shields.io/badge/Status-Production%20Ready-success?style=for-the-badge)
![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)

**✨ A complete, production-ready .NET 9.0 Web API template with Clean Architecture**

**Ready to use out of the box! Just clone, configure, and deploy.**

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [API Endpoints](#-api-endpoints)

</div>

---

## ✅ Project Status

**🟢 PRODUCTION READY** - This project is fully functional and ready for immediate use.

- ✅ Complete authentication system
- ✅ Email verification workflow
- ✅ Password reset with OTP
- ✅ JWT token management
- ✅ File upload integration
- ✅ Database migrations included
- ✅ Error handling implemented
- ✅ Logging configured
- ✅ API documentation (Swagger)

---

## 🎯 What is This?

**Temp-Back_End** is a professional backend API template built with **Clean Architecture** principles. It's designed to be a **starting point** for building enterprise-level applications, saving you weeks of development time.

### Why Use This Template?

- 🚀 **Save Time** - Start with a complete, working solution
- 🏗️ **Best Practices** - Follows industry-standard architecture patterns
- 🔒 **Secure by Default** - Built-in security features
- 📚 **Well Documented** - Comprehensive API documentation
- 🔧 **Easy to Customize** - Clean, maintainable code structure
- ⚡ **Production Ready** - Tested and ready for deployment

---

## ✨ Features

### 🔐 Authentication & Authorization
- ✅ User registration with email verification
- ✅ JWT-based authentication with refresh tokens
- ✅ Token validation and refresh
- ✅ Multi-session management
- ✅ Secure logout (single & all sessions)
- ✅ Role-based access control (User, Admin, Devo)

### 👤 User Management
- ✅ Complete user registration flow
- ✅ Profile image upload (Cloudinary integration)
- ✅ Email verification via secure tokens
- ✅ Password reset with OTP codes
- ✅ User profile management

### 🛡️ Security
- ✅ PBKDF2 password hashing (100,000 iterations)
- ✅ Secure token generation
- ✅ Email domain validation
- ✅ Input validation with Value Objects
- ✅ SQL injection protection (EF Core)
- ✅ CORS configuration

### 📧 Email Services
- ✅ Email verification
- ✅ Password reset emails
- ✅ OTP code delivery
- ✅ HTML email templates

### 📁 File Management
- ✅ Cloudinary integration
- ✅ Image upload with optimization
- ✅ Default image fallback
- ✅ Image deletion

### 🛠️ Development Features
- ✅ API Versioning
- ✅ Swagger/OpenAPI documentation
- ✅ Structured logging (Serilog)
- ✅ Global exception handling
- ✅ Database migrations
- ✅ Clean Architecture pattern

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation Steps

#### 1. Clone the Repository

```bash
git clone https://github.com/OmarMahamad/Temp-Back_End.git
cd Temp-Back_End
```

#### 2. Configure Database Connection

Edit `sorc/BackEnd.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YourDatabase;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### 3. Run Database Migrations

```bash
cd sorc/BackEnd.Api
dotnet ef database update --project ../BackEnd.Infrastructure
```

#### 4. Configure Application Settings

Update `sorc/BackEnd.Api/appsettings.json` with your settings:

```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARACTERS_LONG",
    "Issuer": "YourApp",
    "Audience": "YourFrontend",
    "DurationInMinutes": 40
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "Folder": "generated_images",
    "DeliveryFormat": "webp"
  }
}
```

> ⚠️ **Important**: For production, use Environment Variables instead of storing secrets in `appsettings.json`.

#### 5. Run the Application

```bash
dotnet run --project sorc/BackEnd.Api
```

#### 6. Access Swagger Documentation

Open your browser and navigate to:
- **Swagger UI**: `https://localhost:5001/swagger`
- **API Base URL**: `https://localhost:5001/api/v1`

---

## 📚 Documentation

### API Endpoints

#### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/Authorization/Register` | Register new user | ❌ |
| POST | `/api/v1/Authorization/Login` | User login | ❌ |
| POST | `/api/v1/Authorization/emailverify/{token}` | Verify email | ❌ |
| POST | `/api/v1/Authorization/resend-verification-email` | Resend verification email | ❌ |
| POST | `/api/v1/Authorization/ForgotPasswordAsync` | Request password reset | ❌ |
| POST | `/api/v1/Authorization/Check-Otp-Code` | Verify OTP code | ❌ |
| POST | `/api/v1/Authorization/Reset-Password` | Reset password | ❌ |
| POST | `/api/v1/Authentication/Refresh-AccessToken` | Refresh access token | ✅ |
| POST | `/api/v1/Authentication/ValidateToken` | Validate token | ✅ |
| POST | `/api/v1/Authentication/Logout` | Logout (single session) | ✅ |
| POST | `/api/v1/Authentication/Logout-FromAllSessions/{userid}` | Logout all sessions | ✅ |

### Example: Register User

```http
POST /api/v1/Authorization/Register
Content-Type: multipart/form-data

{
  "Name": "John Doe",
  "Email": "john@example.com",
  "Password": "SecurePassword123!",
  "Phone": "+1234567890",
  "verify_email_url": "https://yourfrontend.com/verify-email",
  "file": "[Image File - Optional]",
  "Address": {
    "Street": "123 Main St",
    "City": "New York"
  }
}
```

### Example: Login

```http
POST /api/v1/Authorization/Login
Content-Type: application/json

{
  "Email": "john@example.com",
  "Password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Success",
  "code": 100,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64-encoded-token..."
  }
}
```

### Using Swagger UI

1. Navigate to `https://localhost:5001/swagger`
2. Click **Authorize** button
3. Enter: `Bearer {your-token}`
4. Test all endpoints interactively

---

## 🏗️ Architecture

This project follows **Clean Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────┐
│      BackEnd.Api (Presentation)     │
│  Controllers, Middleware, Config    │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   BackEnd.Application (Business)     │
│  Services, DTOs, Application Logic   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│    BackEnd.Domin (Domain)            │
│  Entities, Value Objects, Domain     │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│ BackEnd.Infrastructure (Data)        │
│  Database, External Services         │
└──────────────────────────────────────┘
```

### Design Patterns

- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Factory Pattern** - Entity creation
- **Value Objects (DDD)** - Domain modeling
- **Response Pattern** - Unified API responses

---

## 🔒 Security Features

- ✅ **Password Hashing**: PBKDF2 with 100,000 iterations
- ✅ **JWT Tokens**: Secure token-based authentication
- ✅ **Refresh Token Rotation**: Enhanced security
- ✅ **Email Validation**: Domain whitelist and validation
- ✅ **Input Validation**: Value Objects pattern
- ✅ **SQL Injection Protection**: EF Core parameterized queries
- ✅ **CORS**: Configurable cross-origin policies

---

## 📊 Response Format

All API responses follow a consistent format:

**Success:**
```json
{
  "isSuccess": true,
  "message": "Operation completed successfully",
  "code": 100,
  "data": { /* response data */ }
}
```

**Error:**
```json
{
  "isSuccess": false,
  "message": "Error message",
  "code": 401,
  "errors": { /* validation errors */ }
}
```

---

## 🗄️ Database

### Running Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName --project sorc/BackEnd.Infrastructure

# Apply migrations
dotnet ef database update --project sorc/BackEnd.Infrastructure
```

### Database Schema

- **Users** - User accounts and profiles
- **Addresses** - User addresses
- **AuthoRepositories** - Refresh tokens
- **OtpCodes** - OTP codes for password reset
- **EmailVerificationTokens** - Email verification tokens

---

## 📝 Configuration

### Environment Variables (Recommended for Production)

```bash
JWT__Key=your-secret-key
JWT__Issuer=YourApp
JWT__Audience=YourFrontend
ConnectionStrings__DefaultConnection=Server=...;Database=...
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpUser=your-email@gmail.com
EmailSettings__SmtpPassword=your-app-password
Cloudinary__CloudName=your-cloud-name
Cloudinary__ApiKey=your-api-key
Cloudinary__ApiSecret=your-api-secret
```

---

## 🚀 Deployment

### Deploy to Azure

1. Create Azure App Service
2. Configure connection strings
3. Set environment variables
4. Deploy using Visual Studio or Azure CLI

### Deploy to Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "BackEnd.Api.dll"]
```

---

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Database
- **JWT Bearer** - Authentication
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation
- **Cloudinary** - Image hosting
- **MailKit** - Email delivery

---

## 📖 Project Structure

```
Temp-Back_End/
├── sorc/
│   ├── BackEnd.Api/              # API Layer
│   ├── BackEnd.Application/      # Business Logic
│   ├── BackEnd.Domin/            # Domain Models
│   └── BackEnd.Infrastructure/   # Data Access
└── README.md
```

---

## ⚠️ Important Notes

### Before Production

- [ ] Move JWT key to environment variables
- [ ] Configure CORS for specific origins
- [ ] Enable HTTPS only
- [ ] Set up proper logging
- [ ] Configure email service
- [ ] Set up Cloudinary account
- [ ] Review security settings

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

## 📞 Support

- 📧 **Issues**: [GitHub Issues](https://github.com/OmarMahamad/Temp-Back_End/issues)
- 📖 **Documentation**: Check Swagger UI at `/swagger`

---

## ⭐ Show Your Support

If you find this project helpful, please give it a ⭐ star on GitHub!

---

<div align="center">

**Made with ❤️ using .NET 9.0**

**Ready to use. Ready to deploy. Ready to scale.**

</div>


### Design Patterns

- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Factory Pattern** - Entity creation
- **Value Objects (DDD)** - Domain modeling
- **Response Pattern** - Unified API responses

---

## 🔒 Security Features

- ✅ **Password Hashing**: PBKDF2 with 100,000 iterations
- ✅ **JWT Tokens**: Secure token-based authentication
- ✅ **Refresh Token Rotation**: Enhanced security
- ✅ **Email Validation**: Domain whitelist and validation
- ✅ **Input Validation**: Value Objects pattern
- ✅ **SQL Injection Protection**: EF Core parameterized queries
- ✅ **CORS**: Configurable cross-origin policies

---

## 📊 Response Format

All API responses follow a consistent format:

**Success:**
```json
{
  "isSuccess": true,
  "message": "Operation completed successfully",
  "code": 100,
  "data": { /* response data */ }
}
```

**Error:**
```json
{
  "isSuccess": false,
  "message": "Error message",
  "code": 401,
  "errors": { /* validation errors */ }
}
```

---

## 🗄️ Database

### Running Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName --project sorc/BackEnd.Infrastructure

# Apply migrations
dotnet ef database update --project sorc/BackEnd.Infrastructure
```

### Database Schema

- **Users** - User accounts and profiles
- **Addresses** - User addresses
- **AuthoRepositories** - Refresh tokens
- **OtpCodes** - OTP codes for password reset
- **EmailVerificationTokens** - Email verification tokens

---

## 📝 Configuration

### Environment Variables (Recommended for Production)

```bash
JWT__Key=your-secret-key
JWT__Issuer=YourApp
JWT__Audience=YourFrontend
ConnectionStrings__DefaultConnection=Server=...;Database=...
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpUser=your-email@gmail.com
EmailSettings__SmtpPassword=your-app-password
Cloudinary__CloudName=your-cloud-name
Cloudinary__ApiKey=your-api-key
Cloudinary__ApiSecret=your-api-secret
```

---

## 🚀 Deployment

### Deploy to Azure

1. Create Azure App Service
2. Configure connection strings
3. Set environment variables
4. Deploy using Visual Studio or Azure CLI

### Deploy to Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "BackEnd.Api.dll"]
```

---

## 🛠️ Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Database
- **JWT Bearer** - Authentication
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation
- **Cloudinary** - Image hosting
- **MailKit** - Email delivery

---

## 📖 Project Structure
Temp-Back_End/
├── sorc/
│ ├── BackEnd.Api/ # API Layer
│ ├── BackEnd.Application/ # Business Logic
│ ├── BackEnd.Domin/ # Domain Models
│ └── BackEnd.Infrastructure/ # Data Access
└── README.md


---

## ⚠️ Important Notes

### Before Production

- [ ] Move JWT key to environment variables
- [ ] Configure CORS for specific origins
- [ ] Enable HTTPS only
- [ ] Set up proper logging
- [ ] Configure email service
- [ ] Set up Cloudinary account
- [ ] Review security settings

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

## 📞 Support

- 📧 **Issues**: [GitHub Issues](https://github.com/OmarMahamad/Temp-Back_End/issues)
- 📖 **Documentation**: Check Swagger UI at `/swagger`

---

## ⭐ Show Your Support

If you find this project helpful, please give it a ⭐ star on GitHub!

---

<div align="center">

**Made with ❤️ using .NET 9.0**

**Ready to use. Ready to deploy. Ready to scale.**

</div>