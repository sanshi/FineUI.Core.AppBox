# FineUI.Core.AppBox

FineUI.Core.AppBox 是 FineUI 官方应用系统样板，一个基于 FineUI.Core（社区版）的通用权限管理框架，包含用户、职称、部门、角色与角色权限管理等模块。本仓库是该项目的唯一真相源，欢迎通过 Issue 和 Pull Request 参与技术讨论与改进。

<!-- fineui-community:start -->
## 加入 FineUI 社区

欢迎[加入 FineUI 社区](https://fineui.com/fans/)，一站式获取社区版、完整示例、空项目、快速入门和 AppBox 等配套资源，及时了解版本更新，交流控件用法与项目实践。**FineUI.Core、FineUI.Pro 与 FineUI.Java 社区版均可永久免费商用**，欢迎一起分享经验、讨论问题。
<!-- fineui-community:end -->

## 依赖方式

项目文件已声明从公共软件包仓库获取的 NuGet 包 `FineUI.Core`。正常联网构建时，包管理器会自动还原依赖。

## 生成设计时文件

安装 Node.js 后，双击仓库根目录的 `生成并监听设计时文件.bat`。脚本会先扫描页面，再持续监听 `.cshtml` 与对应后台文件的改动；按回车或关闭窗口即可停止。

只有页面声明了可生成字段的 FineUI 控件时，才会生成 `.cshtml.designer.cs`；没有控件字段的页面不会生成空的设计时文件。工具会清理已不再需要的自动生成文件，保留无法确认来源或包含手写成员的文件。

只检查文件是否需要生成、更新或清理，可在仓库根目录运行 `.\生成并监听设计时文件.bat --check`；只生成一次而不监听，可运行 `node tools/generate-designers.mjs`。

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

## 页面配置

全站默认值放在 `appsettings.json` 的 `FineUI` 节。每用户的主题 Cookie 由应用的 `AppPageManagerInitializer.Initialize(pm, request)` 读取；页面基类在 GET 处理器之前调用它。

RazorForms 单页的安全策略和开发模式放在每轮都会执行的 Razor 页面开头或公共服务端初始化中，并在第一个控件声明前确定。回发会重跑模板，但不会重跑 `OnGet`，因此不能只在 `OnGet` 设置这些配置：

```cshtml
@{
    var pm = PageManager.Instance;
    pm.EnableWatermark = true;
    pm.WatermarkText = "I❤︎FineUI";
}
```

配置顺序是“全站默认 → 应用公共初始化 → 单页设置 → 控件声明”。布局保留 `@F.PageManager`、样式与脚本输出；依赖控件字段的首次数据绑定放在 `Page_Load` 的 `!IsPostBack` 分支。主题 Cookie 初始化只用于 GET；CSP 安全策略、开发模式及当前声明需每轮从服务器取得。

## RazorForms 升级与严格 CSP

RazorForms 默认使用 JSON 首屏配置和 JSON 回发，普通控件事件回发到当前完整 URL，保留原查询参数。无需添加新的架构启用属性。MVC、RazorPages 以及明确指定网址的上传、下载、自定义 JSON 入口继续使用各自的调用方式。

升级时一起更新 FineUI.Core 包、DLL 和配套客户端资源，并刷新已经打开的页面。旧页签不能继续使用原脚本响应协议；保存结果未知时先核查业务数据，再决定是否重新提交。回滚也必须同时恢复服务器和客户端资源并刷新页面。

普通策略下，原来的 `RegisterStartupScript(Window1.GetMaximizeReference())` 和字符串消息回调仍兼容。开启严格 CSP 时，这些写法须迁为控件即时辅助方法、`PageContext.Execute`、具名函数或命令回调。例如回发中可调用 `new WindowAjaxHelper(Window1).Maximize()`；消息确认后的动作可以写成：

```csharp
new Alert
{
    Message = "保存成功！",
    OkCommand = ClientCommand.HideActiveWindowPostBack("saved")
}.Show();
```

执行页面函数使用 `PageContext.InvokeFunction("app.afterSave", data)`，参数是 JSON 数据。不要把脚本字符串或 `Get...Reference()` 的返回值当作函数名。首屏初始化用控件声明属性或模板中的具名函数；控件即时辅助方法保留仅在回发生效的边界。

`RendererFunction="renderSalary(2)"` 这样的 JSON 工厂参数可以保留；内联函数、脚本参数、树节点 `OnClientClick`、HTML `onclick` 和 `javascript:` 网址须迁移。可信 Razor 模板内的普通脚本可以使用页面 nonce；通过 `Html.Raw` 或用户内容注入的脚本不能自动获得授权。第三方组件需要放宽策略时应说明具体原因，不能把所有页面都视为严格 CSP 示例。

**不兼容提醒**：AJAX 的 `complete`、`success` 和全局成功钩子仍接收原始响应文本，但 RazorForms 文本已是 JSON。`AuthenticationTimeout` 等通过查找 `window.location.href` 判断跳转的代码必须改为 `JSON.parse(data)` 后检查 `commands` 中的 `page.redirect`，不会提供旧脚本正文模拟。

RazorForms 每轮回发真正重跑当前 Razor（含布局与分部模板）取声明，丢弃 HTML，不执行 OnGet 或 Page_Load；随后合并当前值，再运行一次 Page_Load 和可信事件。完整控件/事件声明不在浏览器或整页 Session 中保存。业务实际修改的有限属性与浏览器当前值继续累计，空串、null、false、0、空集合不会被模板初值覆盖。

把首次数据装载从 `DataSource="@Model.GetRows()"` 移到后台：

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        Grid1.DataSource = GetRows();
        Grid1.DataBind();
    }
}
```

未迁移的无副作用查询通常只多执行一次；只在 OnGet 初始化的对象、一次性资源或有副作用的模板表达式可能失败，必须调整。固定选项、页面安全策略等声明每轮从服务器取得；不要覆盖原生绑定已收到的输入模型。动态新增控件在回发 Page_Load 中为已恢复控件重新登记事件，不接受浏览器方法名。当前固定工厂不支持任意自定义派生控件，使用前须核查。

条件/循环影响匿名位置时使用稳定业务 ID。无名或重名的表格命令改变文字、参数后，须使用唯一 CommandName 或在每轮 Page_Load 重设。由数据绑定生成且没有对应模板项的富 HTML Display，以及动态 HTML 数据字段/格式化配置，每轮在服务器重设。普通策略的旧确认/网址表达式可显式放在模板或每轮后台登记，不能作为浏览器当前文字往返。

编辑器工具栏等参数使用 `OptionItem.JsonValue`：`new OptionItem { Key = "toolbar", JsonValue = new JArray { new JArray("bold", "italic") } }`，模板可写 `JsonValue="@Model.ToolbarOptions"`。布尔使用 JValue，显式 JSON null 使用 JValue.CreateNull()。原 Value/PersistOriginal 仍兼容；字符串即使长得像 JSON，也不会自动改成结构化参数。第三方编辑器自身的 CSP 要求须另外核查。

安装 Node.js 后，在仓库根目录运行 `node tools/check-razorforms-upgrade.mjs`，可定位“协议升级必须核查”“严格 CSP 时迁移”和“人工核查”三类候选；加 `--json` 输出清单。扫描只读源码，不自动重写；原生 `?handler=...` 与 DataSource 是核查项，不能批量删除。

## 许可边界

本仓库中由合肥三生石上软件有限公司拥有著作权的示例或应用项目源代码采用 [MIT 许可证](LICENSE)。FineUI 各端框架源码、二进制软件包、内嵌的 FineUI.js 运行时以及 FineUI 名称、标识和商标不属于 MIT 授权范围，仍适用各自的商业或社区版许可。具体边界见 [NOTICE.md](NOTICE.md)。

## 参与贡献

请先阅读 `CONTRIBUTING.md`。安全问题请按 `SECURITY.md` 私下报告。
