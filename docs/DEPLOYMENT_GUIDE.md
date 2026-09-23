# MarketLink - Production Deployment Guide

This guide covers deployment options for **MarketLink** (ASP.NET Core 8 / 10 MVC + EF Core + SQL Server).

---

## 1. Prerequisites
- **.NET 8.0 SDK / Runtime** or higher installed on host.
- **Microsoft SQL Server 2019+** (or Azure SQL Database / AWS RDS SQL Server).
- Web Server: **IIS (Windows)**, **Kestrel + Nginx/Apache (Linux)**, or **Docker Container**.

---

## 2. Configuration Settings (`appsettings.Production.json`)

Create an `appsettings.Production.json` or use Environment Variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=MarketLinkDb;Persist Security Info=False;User ID=your_user;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Smtp": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "Username": "apikey",
    "Password": "YOUR_SENDGRID_API_KEY",
    "UseSsl": true,
    "FromEmail": "notifications@marketlink.com",
    "FromName": "MarketLink"
  },
  "AllowedHosts": "*"
}
```

---

## 3. Deployment Methods

### Option A: SmarterASP.NET / MonsterASP Hosting (Shared Windows Hosting)
1. **Publish Locally**:
   ```bash
   dotnet publish MarketLink.Web/MarketLink.Web.csproj -c Release -o ./publish
   ```
2. **Setup Database**:
   - Create a SQL Server database via the hosting control panel.
   - Update `appsettings.json` connection string with the host's SQL credentials.
3. **Upload Files**:
   - Connect via FTP (e.g. FileZilla) and upload the contents of `./publish` to the `site/wwwroot` folder.
4. **Configure IIS / .NET Version**:
   - In control panel, verify the Application Pool is configured for **No Managed Code** (CoreCLR handles runtime execution).
5. **Initial Run**:
   - The first request executes `db.Database.MigrateAsync()` and `DatabaseSeeder.SeedAsync()` automatically, creating all schemas, default categories, markets, and accounts.

---

### Option B: Windows Server IIS
1. Install **.NET Core Hosting Bundle**:
   - Download and install the ASP.NET Core Hosting Bundle on the Windows Server.
2. In **IIS Manager**:
   - Add a new Website pointing to the published folder `C:\inetpub\wwwroot\marketlink`.
   - Set Application Pool **.NET CLR Version** to **No Managed Code**, **Pipeline mode** to **Integrated**.
3. Grant `IIS_IUSRS` Read & Execute permissions on the web folder, and Write permissions on `wwwroot/uploads` for images.

---

### Option C: Azure App Service (Linux or Windows)
1. Create a **Web App** (Runtime: .NET 8) and an **Azure SQL Database**.
2. Set Environment Variables / App Settings:
   - `ConnectionStrings__DefaultConnection`: `<Azure SQL Connection String>`
3. Deploy via GitHub Actions or Azure CLI:
   ```bash
   dotnet publish MarketLink.Web/MarketLink.Web.csproj -c Release -o ./publish
   az webapp deploy --resource-group MarketLinkRG --name marketlink-app --src-path ./publish.zip --type zip
   ```

---

### Option D: Docker Deployment
Create a `Dockerfile` in the root directory:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MarketLink.Core/MarketLink.Core.csproj", "MarketLink.Core/"]
COPY ["MarketLink.Infrastructure/MarketLink.Infrastructure.csproj", "MarketLink.Infrastructure/"]
COPY ["MarketLink.Web/MarketLink.Web.csproj", "MarketLink.Web/"]
RUN dotnet restore "MarketLink.Web/MarketLink.Web.csproj"
COPY . .
WORKDIR "/src/MarketLink.Web"
RUN dotnet build "MarketLink.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketLink.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketLink.Web.dll"]
```

Run container:
```bash
docker build -t marketlink-web .
docker run -d -p 5000:8080 -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=MarketLinkDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True" marketlink-web
```

---

## 4. Post-Deployment Checklist
- [x] Verify SSL / HTTPS certificate installation.
- [x] Navigate to `/swagger` to confirm REST API readiness.
- [x] Log in using Admin credentials to verify database seeding.
- [x] Test placing a pre-order with market slot selection.
- [x] Ensure `wwwroot/uploads` has write permissions for image file uploads.
