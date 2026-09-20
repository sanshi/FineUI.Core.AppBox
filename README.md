# FineUI.Core.AppBox

FineUI.Core.AppBox 是 FineUI 官方应用系统样板，一个基于 FineUI.Core（社区版）的通用权限管理框架，包含用户、职称、部门、角色与角色权限管理等模块。本仓库是该项目的唯一真相源，欢迎通过 Issue 和 Pull Request 参与技术讨论与改进。

## 依赖方式

项目文件已声明从公共软件包仓库获取的 NuGet 包 `FineUI.Core`。正常联网构建时，包管理器会自动还原依赖；仓库不包含 FineUI.Core.dll、FineUI.Pro.dll、fineui-java.jar，也不包含 FineUI 框架源码。

## 构建

安装 .NET 8 SDK 后，在仓库根目录运行：

```powershell
dotnet restore FineUI.Core.AppBox.sln
dotnet build FineUI.Core.AppBox.sln -c Release --no-restore
```

## 运行

在仓库根目录启动（首次会先还原 NuGet 包并编译）：

```powershell
dotnet run --project FineUI.Core.AppBox/FineUI.Core.AppBox.csproj
```

启动后打开 <http://localhost:53001> —— 地址来自 `FineUI.Core.AppBox/Properties/launchSettings.json` 里的 `FineUI.Core.AppBox` 配置。

也可以用 Visual Studio 打开 `FineUI.Core.AppBox.sln`，把启动配置切成 `IIS Express`（<http://localhost:53221>）。

端口被占用时，改 `Properties/launchSettings.json` 里对应配置的 `applicationUrl` 即可。

数据库用 Entity Framework Core 的 Code First 模式，表结构在第一次运行时自动创建；连接串取自 `appsettings.json` 的 `ConnectionStrings:SQLServer`（`Startup.cs` 里的 `options.UseSqlServer(...)`）。默认管理员账号是 `admin`，密码 `admin`。

**不需要授权文件**：本仓库引用的是公共 NuGet 包 `FineUI.Core`（社区版），社区版不做授权校验，克隆下来就能直接跑。

## 发布历史

各版本的更新内容见 [CHANGELOG.md](CHANGELOG.md)。

## 许可边界

本仓库中由合肥三生石上软件有限公司拥有著作权的示例或应用项目源代码采用 [MIT 许可证](LICENSE)。FineUI 各端框架源码、二进制软件包、内嵌的 FineUI.js 运行时以及 FineUI 名称、标识和商标不属于 MIT 授权范围，仍适用各自的商业或社区版许可。具体边界见 [NOTICE.md](NOTICE.md)。

## 参与贡献

请先阅读 `CONTRIBUTING.md`。安全问题请按 `SECURITY.md` 私下报告。
