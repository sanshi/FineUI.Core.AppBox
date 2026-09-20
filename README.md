# FineUI.Core.AppBox

FineUI.Core.AppBox 是 FineUI 官方应用系统样板。本仓库是该项目的唯一真相源，欢迎通过 Issue 和 Pull Request 参与技术讨论与改进。

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

**不需要授权文件**：本仓库引用的是公共 NuGet 包 `FineUI.Core`（社区版），社区版不做授权校验，克隆下来就能直接跑。

## 项目说明

FineUI.Core.AppBox 是基于 FineUI.Core（社区版） 的通用权限管理框架，包括用户管理、职称管理、部门管理、角色管理、角色权限管理等模块。

更新下载：https://fineui.com/fans/

### 注意

1. FineUI.Core.AppBox作为演示程序，请不要直接用于真实项目。
2. FineUI.Core.AppBox作为演示程序，版本之间不兼容，也不支持版本升级。

### 使用步骤

1. 用 VS2022 打开项目工程文件（FineUI.Core.AppBox.sln）；
2. 打开 appsettings.json，配属数据库连接字符串（ConnectionStrings->SQLServer）；
  - 在 Startup.cs 代码中使用 options.UseSqlServer(Configuration.GetConnectionString("SQLServer")))
3. 运行（Ctrl+F5）！

4. 请使用管理员账号登陆网站（用户名：admin 密码：admin）。

### 知识储备

1. 本项目采用Entity Framework Core的Code First开发模式，数据库会在网站第一次运行时自动创建。
2. 如果对Entity Framework Core不熟悉，请事先学习微软官方文档：https://learn.microsoft.com/en-us/ef/core/
3. 如果尚未安装.Net 8.0，请先安装 SDK：https://dotnet.microsoft.com/download

## 发布历史

各版本的更新内容见 [CHANGELOG.md](CHANGELOG.md)。

## 许可边界

本仓库中由合肥三生石上软件有限公司拥有著作权的示例或应用项目源代码采用 [MIT 许可证](LICENSE)。FineUI 各端框架源码、二进制软件包、内嵌的 FineUI.js 运行时以及 FineUI 名称、标识和商标不属于 MIT 授权范围，仍适用各自的商业或社区版许可。具体边界见 [NOTICE.md](NOTICE.md)。

## 参与贡献

请先阅读 `CONTRIBUTING.md`。安全问题请按 `SECURITY.md` 私下报告。
