# DemoWebApi

简洁启动说明（前后端分离，Vue + ASP.NET Core）。

## 环境

- .NET SDK 8.0
- Node.js 18+
- MySQL

## 1) 配置数据库

编辑 `E:\workspace_csharp\DemoWebApi\DemoWebApi\appsettings.json`：

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=learn;User=root;Password=adm;SslMode=None;"
}
```

## 2) 启动后端

```powershell
cd E:\workspace_csharp\DemoWebApi\DemoWebApi
dotnet restore
dotnet run
```

- API: `http://localhost:5159`
- Swagger: `http://localhost:5159/swagger`

## 3) 启动前端

```powershell
cd E:\workspace_csharp\DemoWebApi\DemoWebApi\frontend
npm install
npm run dev
```

- 前端: `http://localhost:5173`
- `/api` 默认代理到 `http://localhost:5159`

## 常见问题

- `Couldn't find a project to run`：你不在 `DemoWebApi\DemoWebApi` 目录，切过去再 `dotnet run`。
- 接口请求失败：先确认后端已启动、端口正确、MySQL 用户名密码正确。
