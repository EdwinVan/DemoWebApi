# DemoWebApi

前后端分离示例项目（Vue 3 + ASP.NET Core 8 + MySQL）。

## 系统功能介绍

- 登录与鉴权
  - 支持 `JWT` 登录
  - 未登录仅可查看 `Overview` 和 `Quick Stats`
  - 登录后可访问 `Rooms / Things / Users`
- 角色权限
  - `SuperAdmin`：用户管理、房间管理、物品管理（增删改查）
  - `NormalUser`：只读访问（按后端接口授权）
- 用户系统
  - 用户创建、编辑、删除
  - 支持设置/修改密码
  - 密码强度规则：8-64 位，包含大小写字母、数字、特殊字符，且不能包含空格
- 房间与物品
  - `Rooms` 与 `Things` 的增删改查
  - 可查看某个房间关联的物品信息

## 超级管理员信息（开发环境）

- 用户名：`superadmin`
- 密码：`Admin@123456`
- 配置位置：`DemoWebApi/appsettings.json` -> `SuperAdmin`

说明：
- 应用启动时会自动确保超级管理员账号存在，并同步超级管理员密码。
- 上述账号仅用于本地开发演示，生产环境请务必修改。

## 环境要求

- .NET SDK 8.0
- Node.js 18+
- MySQL 8.x

## 1) 配置数据库

编辑 `E:\workspace_csharp\DemoWebApi\DemoWebApi\appsettings.json`：

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=learn;User=root;Password=admin;SslMode=None;AllowPublicKeyRetrieval=True;"
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

- 前端地址：`http://localhost:5173`
- 开发代理：`/api` -> `https://localhost:7159`

## 4) 首次使用建议流程

1. 启动后端和前端
2. 打开 `http://localhost:5173`
3. 使用超级管理员账号登录
4. 进入 `Users` 创建普通用户
5. 进入 `Rooms / Things` 进行数据维护

## 常见问题

- `Couldn't find a project to run`
  - 需要在 `E:\workspace_csharp\DemoWebApi\DemoWebApi` 目录执行 `dotnet run`
- `Access denied for user 'root'@'localhost'`
  - MySQL 账号或密码不正确，请核对连接字符串
- `address already in use`
  - 端口被占用，先结束旧进程再启动
