# 发布历史

本文件在五个 AppBox 仓库中内容完全一致（FineUI.Core.AppBox、FineUI.Core.AppBox.Dapper、FineUI.Pro.AppBox、FineUI.Pro.AppBox.Dapper、FineUI.Java.AppBox）。

**前缀说明**：`[Core]` = 仅 Core 版（FineUI.Core.AppBox）、`[Pro]` = 仅 Pro 版（FineUI.Pro.AppBox）、`[Java]` = 仅 Java 版（FineUI.Java.AppBox），Dapper 仓库专用条目用 `[Core.Dapper]` / `[Pro.Dapper]`。更早的历史条目保留了当年的版本标识（`[EFCore]` / `[Dapper]` / `[Core/Pro]`）。

**分组说明**：新版本的条目按「新增 / 变更 / 废弃 / 移除 / 修复」五类归组，一条只归一类（看条目的主要动作，不要重复归到多组）；没有条目的分组不写标题。定义与产品发布历史（根目录 `CHANGELOG.md`）保持一致，安全相关条目用 `[安全]` 前缀标注、不单列分组，破坏性变更在条目里写明「不兼容提醒」并给出迁移办法。

2009 年至今的历史记录保留原始措辞，仅归并了层次标记与分组标题。

## v16.0（2026-09-30）

### 变更

- 升级到 FineUI.Core/Pro（社区版）v16.0。
- 客户端点击事件统一改用 `ClickHandler` 引用页面或公共脚本中的具名函数。
  - 覆盖关闭窗口、打开新增窗口、动态帮助菜单，以及批量删除、启用和禁用操作。
- 可信 HTML 统一改用服务端 `RawHtml` / `TextRawHtml` 和客户端 `F.rawHtml`。
- 启用严格模式，明确关闭 `AllowDangerousRawTag` 和 `AllowDangerousScriptTag`。
- **[Pro]** 页面级自定义回发统一改用 `F.customEvent`，并在 `Page_CustomEvent` 中处理事件名称和结构化参数。
- **[Pro]** 全局设置 `EnableImplicitPostBack=false`，未声明服务端事件的动作控件不再自动回发，与 Core、Java 保持一致。
  - 纯客户端按钮只保留 `ClickHandler`；服务端按钮只声明 `OnClick`；控件级 `EnablePostBack` 仅用于显式覆盖。
  - 类库兼容默认值仍为 `true`，老项目只升级 DLL 且不修改 `Web.config` 时，回发行为保持不变。
- **[Pro]** 全局设置 `EnableImplicitChangeEvents=false`，变化控件声明服务端事件即可自动回发。
  - 删除与服务端变化事件重复的 `AutoPostBack=true`；显式 `AutoPostBack` 值仍用于覆盖默认判定。
  - 其他控件发起回发时只同步当前值，不再连带触发变化事件；类库默认 `true` 以兼容老项目。
- **[Pro]** `Web.config` 明确设置 `fileEncoding=utf-8`，历史非 UTF-8 源码统一转换为 UTF-8，避免 ASP.NET 按系统代码页误读中文页面。
- **[Core]** 删除 `Startup` 中的 FineUI JSON 模型绑定器和 RazorForms 过滤器手工登记，现由 `AddFineUI` 自动完成。
- **[Core]** 自定义回发改用 `F.customEvent` 函数。
  - 涉及 5 个页面共 7 处：用户管理（删除/启用/禁用）、部门成员、角色成员、职称成员、角色权限。
  - 全局函数 `__customEvent` 仅为兼容既有代码保留，不再推荐使用，请改用 `F.customEvent`。
- **[Java]** 批量删除和启停操作使用 `hasSelection`、`getSelectedRows`、`F.confirm`、`F.rawHtml` 与 `F.customEvent` 组成清晰的客户端链路。

### 移除

- 删除 Tree 控件已废除的 `MiniModePopWidth` 属性（v16 中该属性已无任何效果，保留会产生编译警告）。

## v13.0（2025-10-28）

- 升级到FineUI.Core/Pro（社区版）v13.0。
- **[Core]** 通过F.beforeAjaxError函数拦截未认证身份的AJAX请求（401 Unauthorized，比如页面停留时间过长导致的Cookie过期）（earthpea - https://t.zsxq.com/CSHpi）。
  - 在公共JavaScript文件res/common.js中增加F.beforeAjaxError函数，可以处理如下几种异常的AJAX请求（弹出确认对话框）：
    - Cookie过期导致身份验证失败时，提醒用户后跳转到登录页面（身份验证失败，是否跳转到登录页面？）。
      - 重现方法：在浏览器调试工具中，清除当前网站的Cookie。
    - 客户端AJAX请求超时，提醒用户后刷新页面（客户端请求超时，是否刷新页面并重试？）。
      - 重现方法：设置较短的AJAX超时时间，比如PageManager.Instance.AjaxTimeout=1; 然后在后台代码中Thread.Sleep(5000)。
    - 服务器宕机时，提醒用户后刷新页面（发生未知错误，是否刷新页面并重试？）。
      - 重现方法：停止IIS服务。
    - 服务器异常时，提醒用户后刷新页面（服务器异常，是否刷新页面并重试？）。
      - 重现方法：在后台代码中抛出异常，比如throw new Exception("Test exception.");
  - 文章：https://fineui.com/docs/#/Questions/10600_appboxcore_cookie_expire
- 优化列表页面的布局和样式（经典的Form+Grid布局）。
  - 为body添加f-body-darkerbg，为页面启用深色背景。
  - 为Form和Grid设置ShowBorderShadow=true属性。
  - 为外层容器设置布局属性Layout="VBox"和BoxConfigSpace="10"，同时设置Form的RemoveLastFieldsMargin="true"（移除最后一行中表单字段的底部外边距）。
- 优化编辑页面的布局和样式。
  - 为最外层面板设置布局属性IsViewPort="true"和Layout="Fit"。
  - 为唯一的子控件Form设置AutoScroll="true"，当编辑页面高度变小时，垂直滚动条出现在Form控件（而不是外部容器或者页面）。
- 为BaseModel.cs新增GetIdentityRoleNames方法，获取当前用户的角色名称列表（用于页面显示）。
- 在BaseAdminModel.cs中，更新PageManager控件的EnableWatermark、WatermarkText等属性，将当前登录用户名称显式为页面水印。
- 在用户管理页面，通过usersToDelete.Any(u => u.Name == "admin")来阻止删除超级管理员用户。
  - **[Pro]** 将多行删除（btnDeleteSelected_Click）和单行删除（Grid1_RowCommand）合并为一个逻辑处理函数（DeleteRows）。
- **[Pro]** 表格行命令的权限检查（删除表格的数据预绑定事件PreDataBound）。
  - 之前：在表格的Grid1_PreDataBound事件中，设置行命令按钮的Enabled属性。
  - 现在：直接在页面加载时（Page_Load）设置行命令按钮的Enabled属性（Grid1.FindColumn("editField").Enabled = powerCoreUserEdit），代码更加简洁直观，并且和Core版保持一致。
  - 涉及的页面：用户列表、角色列表、职称列表、部门列表、菜单列表、权限列表。
- **[Pro]** 表格选中项优化。
  - 将表格属性DataKeyNames="ID"改为DataIDField="ID"。
  - 后台获取选中项由GetSelectedDataKeyID(Grid1)改为Convert.ToInt32(Grid1.SelectedRowID)和Grid1.SelectedRowIDArray.Select(u => Convert.ToInt32(u)).ToArray()。
  - 删除PageBase.cs中的帮助函数GetSelectedDataKeyID和GetSelectedDataKeyIDs。
- 用户编辑页面增强。
  - 后台CurrentUser属性需要在OnGetAsync中赋值，然后在页面中通过类似<f:Label For="CurrentUser.Name"></f:Label>的标签调用（此时Page_Load方法尚未执行）。
  - 关闭按钮通过ClickHandler引用common.js中的具名函数，从而避免在后台拼接JavaScript代码。
  - 重构所属角色、所属部门和拥有职称三个功能。
    - 之前：一个TwinTriggerBox和一个hfSelectedDept，点击触发器弹出窗口，用户选择后关闭弹出窗口并更新编辑页面的两个输入框。
      - 逻辑和交互都比较复杂：需要单独的选择页面（UserSelectRole.cshtml），还需要处理打开弹出窗体，以及用户选择后更新两个输入框的逻辑。
    - 现在：直接在DropDownBox的下拉框中选择即可。
      - 代码简洁（删除了150行客户端JavaScript代码，用来处理触发器输入框和弹出窗体的交互行为）。
      - 逻辑简单（删除了3个选择页面，粗略估计删除了100行页面标签和100行后台代码）。
      - 交互直观（避免了2个弹出窗体的层叠显示）。
- 选择用户到当前职称页面增强。
  - 需要为表格设置ClearSelectionBeforeBinding=false，否则在数据库分页事件中重新绑定数据时（Grid1.DataBind()）会清空当前已经选中的行。
  - 增加一个文本标签，用来实时显示当前已经选中的用户列表，比如：选中了 4 位用户：舒兆国（user8）, 谭志洪（user64）, 敖志敏（user406）, 刘艳杰（user394）。
- 在部门列表页面，将模拟树的表格改为真正的树表格。
  - 前台页面标签更新：
    - 之前：为表格定义属性EnableSimulateTree="true"，为姓名列定义属性DataSimulateTreeLevelField="TreeLevel"。
    - 现在：新增表格属性EnableTree="true" TreeColumn="Name" DataIDField="ID" DataParentIDField="ParentID" ExpandAllTreeNodes="true"。
  - 后台代码更新：
    - 之前：需要为Dept模型类增加TreeLevel属性（用来标识当前节点在树形结构中的层级，从0开始），并在DeptHelper静态类中递归计算每个节点的层级。
    - 现在：不再需要为Dept模型类增加TreeLevel属性，也不再需要DeptHelper静态类，直接通过await DB.Depts.ToListAsync()获取所有部门列表。
  - 涉及的页面：部门列表页面，部门用户页面，菜单管理页面。
- 在部门编辑页面，将模拟树的下拉列表控件改为下拉树表格（DropDownBox+Grid）。
  - 前台页面标签更新：
    - 之前：DropDownList控件，属性EnableSimulateTree="true" DataSimulateTreeLevelField="TreeLevel" DataEnableSelectField="Enabled"。
    - 现在：DropDownBox控件，启用树表格相关属性EnableTree="true" TreeColumn="Name" DataIDField="ID" DataParentIDField="ParentID" ExpandAllTreeNodes="true"。
  - 后台代码更新1：
    - 之前：需要一个ResolveDDL方法递归更新每个部门的层级（TreeLevel）和是否可选（Enabled），编辑部门时当前部门和其子节点是禁用的。
    - 现在：使用表格的服务端行绑定事件（OnRowDataBound="Grid1_RowDataBound"），在后台事件中判断某个部门是否禁用（此部门是当前部门或者当前部门的子项）。
  - 后台代码更新2：
    - 之前：向下拉列表数据源中添加了一项"--根节点--"（值为-1），用于表示当前部门为顶层部门。
    - 现在：为DropDownBox设置属性AutoShowClearIcon="true"，如果下拉框为空，则表示将当前部门设置为顶层部门。
  - 涉及的页面：编辑部门页面，新增部门页面，编辑菜单页面，新增菜单页面。
- [EFCore]新增静态类QueryableExtensions。
  - 为IQueryable<T>增加SortAndPageAsync和SortAsync扩展方法，用于简化分页查询的代码；删除BaseModel中的SortAndPage和Sort方法。
  - 代码更新：await SortAndPageAsync<User>(q, Grid1); => await q.SortAndPageAsync(Grid1);
- [Dapper]将WhereBuilder改造为泛型类QueryableBuilder<T>。
  - 新增SortAndPageAsync和SortAsync方法；删除BaseModel中的SortAndPage和Sort方法。
  - 代码更新1：await CountAsync<User>(builder); => await builder.CountAsync();
  - 代码更新2：await SortAndPageAsync<Role>(builder, Grid1); => await q.SortAndPageAsync(Grid1);
- 删除不再使用的代码。
  - 删除静态帮助类MenuHelper、DeptHelper。
  - 删除接口类ICustomTree。
  - 删除BaseModel.cs中用来计算模拟树下拉列表的ResolveDDL方法。
  - 模型类Dept和Menu不再实现ICloneable和ICustomTree接口。
  - 删除类Dept和Menu中的TreeLevel、Enabled和IsTreeLeaf三个属性。
  - 删除视图模型类PagingInfoViewModel。
  - 删除UserSelectRole、UserSelectDept、UserSelectTitle三个页面。

## v11.7.2（2024-12-20）

- 升级到FineUI.Core/Pro（社区版）v11.7.2。
- 修正主菜单可能重复加载的问题（鸽子飞扬- https://t.zsxq.com/a5wWU）。
  - 更新MenuHelper、DeptHelper和ConfigHelper类，引入静态只读变量静态只读_lockObj和lock关键词，确保全局静态变量的访问是线程安全的。
- 修正登录后未点击[安全退出]按钮，再次手工通过登录页面重新登录后，主菜单还是上一个用户菜单的问题（2877408506）。
  - 方案一：登录时清空Session中保存的内容 HttpContext.Session.Remove("UserPowerList");
  - 方案二（采纳）：打开/Login页面时，如果发现用户处于登录状态，则直接跳转到登录后的落地页面。
    - **[Core]**：if(User.Identity.IsAuthenticated){ Response.Redirect("/"); }，对应于 / 页面（视图文件Pages/Index.cshtml）。
    - **[Pro]**：if(User.Identity.IsAuthenticated){ Response.Redirect(FormsAuthentication.DefaultUrl);}，对应于 /main.aspx 页面。
- **[Core.Dapper]** 修正用户编辑页面报错的问题（A+『致远、田』 - https://t.zsxq.com/hnjEk）。
  - 错误详情：The member Dept of type FineUI.Core.AppBox.Dapper.Dept cannot be used as a parameter value at Dapper.SqlMapper.LookupDbType。
  - 为User模型类的Dept属性添加[NotMapped]特性；删除Dept、Roles、Titles三个属性（在Dapper中用不到并且容易产生干扰）。
  - 获取当前用户（GetUserByIDAsync）的SQL语句中“depts.Name UserDeptName” 应该改为 “depts.Name DeptName”，因为用户模型中定义的属性名为DeptName。
  - 从用户编辑页面去除密码输入框（提供单独的修改密码功能）；用户名不可修改（文本输入框改为Label控件）。
  - 后台事件中移除对密码的模型验证代码 ModelState.Remove("Name"); 应该改为 ModelState.Remove("CurrentUser.Name");
- **[Core.Dapper]** 登录时用户所属的角色列表应该通过SQL语句查询（以前通过user.Roles是拿不到数据的）。
- **[Core]** 优化用户编辑页面。
  - 从用户编辑页面去除密码输入框（提供单独的修改密码功能）；用户名不可修改（文本输入框改为Label控件）。
  - 后台事件中移除对密码的模型验证代码 ModelState.Remove("Name"); 应该改为 ModelState.Remove("CurrentUser.Name");
- **[Pro.Dapper]** 优化用户编辑页面。
  - 将方法GetCurrentUser提取到基类中并命名为GetUserByID（用户浏览页面user_view.aspx共用此方法）。

## v11.6（2024-09-28）

- 升级到FineUI.Core/Pro（社区版）v11.6。
- **[Core]** 采用全新的表格行命令事件（以角色列表管理页面为例，JavaScript的代码量由原来的 50 行减少为 0 行）。
  - 使用<f:Command>标签设置命令图标（Icon="Pencil"），提示信息（ToolTip）和删除前的客户端确认对话框（ConfirmText）。
  - 客户端rowcommand事件中return false;，用于阻止服务端事件的执行。
  - 无需通过RegisterPreStartupScript(String.Format("window._POWERS={0}", powers.ToString()));向前台输出有关权限的JS变量。
    - 在Page_Load中可以直接设置表格行内按钮的启用状态（Grid1.FindCommand("Edit").Enabled=powerCoreRoleEdit）。
  - 后台删除操作在Grid1_RowCommand事件中进行（之前是Page_CustomEvent），通过事件参数可以获取当前行ID（e.RowID）。
  - 弹出窗体列通过<f:Command>标签的WindowID、WindowIFrameUrlFormatString、WindowIFrameUrlFields等属性控制。
  - 删除common.js中的__createActionCellEl函数。
- **[Core]** 修正编辑菜单页面保存时出现的[The value 'CoreTitleUserView' is not valid for 浏览权限.]错误。
  - 在进行模型验证（ModelState.IsValid）前排除 ViewPowerID 属性的服务端验证（ModelState.Remove("Menu.ViewPowerID")）。
        -介绍文章：http://fineui.com/docs/#/Questions/2000_modelstate
- **[Core]** 修正菜单编辑页面没有为图标列表赋初值的问题（Page_Load中设置rblIconList.SelectedValue属性）。
- **[Core]** 快速设置表格列属性的推荐写法（For="Users.First().ChineseName"）。
  - 此时生成的DataField为"ChineseName"（如果不使用For的推荐写法，可能需要手工设置DataField="ChineseName"）。
  - 此时可以设置QuickSortField="true"来启用列排序（无需显式指定排序字段SortField="Title"）。
- **[Core.Dapper]** 修正多次刷新页面会出现错误的问题（数据库链接未释放）（183937161）。
  - 错误详情：MySql.Data.MySqlClient.MySqlException (0x80004005): error connecting: Timeout expired.
    ---The timeout period elapsed prior to obtaining a connection from the pool.
    ---This may have occurred because all pooled connections were in use and max pool size was reached.
  - 根源：MyConnectionService.cs中的GetDbConnection()方法中，未对_connection是否为空进行判断，导致每次调用都会创建一个新的MySqlConnection对象。
    ---如果调用了GetDbConnection()方法 3 次，则最后一次创建的数据库连接会在当前HTTP请求结束时由Dispose()释放，但是仍然有 2 个数据库连接未释放。
  - 解决办法：GetDbConnection()方法中增加_connection==null的判断，只有尚未创建数据库连接时才新建MySqlConnection对象。

## v11.4（2024-06-28）

- 所有第三方库升级到最新版（EF Core、Dapper、NewtonsoftJson、MySql.Data）。
- 升级到FineUI.Core/Pro（社区版）v11.4（FineUI.Core.AppBox支持RazorForms开发模式）。
- **[Core]** 项目目标框架由.Net Core 3.1 升级到 .Net 8.0。
- **[Pro]** 项目目标框架由.Net Framework 4.5.2 升级到 .Net Framework 4.8.1。
- [Core/Pro]建议使用VS2022打开项目工程。
- 更新res/index.js文件。
  - activeTab方法改名为activateTab。
  - 去除_menuStyle变量，左侧菜单只支持智能树控件。
- 更新res/index.css文件（顶部按钮样式）。
- 更新主题选择页面（用户选择的主题保存在Cookie中）。
- 页面外边距通过Margin来实现，比如用户列表页面，设置外部容器的IsViewPort=true和Margin=15两个属性。
- HTML编码处理（FineUI.Core/Pro v11.2引入的不兼容改变）。
  - 为删除确认框中的文本增加旧版原始HTML标记（现已改用F.rawHtml）。
  - 为菜单管理中的图标增加旧版原始HTML标记（现已改用RawHtml/TextRawHtml）。
- **[Core]** 新版的EFCore支持没有实体类表示连接表的多对多关系（Many-to-many relationships without an entity class to represent the join table）。
  - 删除AppBoxContext中的实体类RoleUsers、TitleUsers和RolePowers。
  - 多对多关系可以简化为：modelBuilder.Entity<Role>().HasMany(r => r.Users).WithMany(u => u.Roles).UsingEntity("RoleUsers");
  - 模型类Role增加Users和Powers属性，删除RoleUsers和RolePowers属性。
  - 删除IKey2ID接口，从BaseModel.cs中删除AddEntities2、RemoveEntities2和ReplaceEntities2方法。
  - 实现获取用户列表并关联部门、角色和职称三个数据。
    - 之前：DB.Users.Include(u => u.Dept).Include(u => u.RoleUsers).ThenInclude(ru => ru.Role).Include(u => u.TitleUsers).ThenInclude(tu => tu.Title)
    - 现在：DB.Users.Include(u => u.Dept).Include(u => u.Roles).Include(u => u.Titles)
  - 新增关联表数据（为多个用户赋予某个角色）。
    - 之前（可以直接操作关联表的实体类）：AddEntities2<RoleUser>(roleID, selectedUserIDs)
    - 现在（删除了关联表的实体类）：
      - 第一步（获取角色和关联的用户列表）：var role = DB.Roles.Include(r => r.Users).Where(r => r.ID == roleID).FirstOrDefault();
      - 第二步（向当前用户列表中新增用户）：AddEntities<User>(role.Users, selectedUserIDs);
- **[Core]** 增加公共res/js/common.js文件。
  - 在Shared/_Layout.cshtml中增加common.js文件的引用。
  - 自定义定义__customEvent函数（对__doPostBack进行简单的封装）。

## v7.1.1（2021-06-28）

- 升级到FineUI.Core/Pro（社区版）v7.1.1。
- 最外层框架由上下结构改为左右结构。
- 将左侧菜单改为智能树菜单（删除手风琴控件菜单）。
- 从框架页index.js分离出来mobileview.js（用来处理屏幕尺寸小于992px的情况，社区版不支持移动端访问）。

## v6.2（2020-03-31）

- 升级到 FineUI.Core（社区版）v6.2.0。
- 基于 ASP.NET Core 的 RazorPages 和 TagHelpers 技术架构。
- 使用 Dapper 和 EntityFramework Core访问数据库。
- 基于 .Net Core 3.1。
- 部分代码参考网友【时不我待】的实现：https://t.zsxq.com/UBAqN3N
- 功能更新。
  - 页面处理器（GET/POST）和数据库操作全部改为异步调用（async/await）。
    - 服务器的可用线程是有限的，在高负载情况下的可能所有线程都被占用，此时服务器就无法处理新的请求，直到有线程被释放。
    - 使用同步代码时，可能会出现多个线程被占用而不能执行任何操作的情况，因为它们正在等待 I/O 完成。
    - 使用异步代码时，当线程正在等待 I/O 完成时，服务器可以将其线程释放用于处理其他请求。
  - 将基类的ExecuteUpdate、Sort、SortAndPage、Count、FindByID方法全部改为异步调用。
  - 增加页面模型基类BaseAdminModel，并设置[Authorize]特性以阻止未登陆用户访问管理页面。
  - 页面模型类中，将对ViewBag的调用改为类属性。
  - [Dapper]修正在线用户数计算错误的问题（将DB.Execute改为DB.QueryFirstOrDefault）。
  - 更新用户密码页面，设置HiddenField的Name=hfUserID属性，以便在后台通过函数参数获取值。
    - 可选实现：设置CurrentUser的[BindProperty]特性，然后通过CurrentUser.ID获取。
    - 可选实现：函数参数IFormCollection values，然后通过Convert.ToInt32(values["CurrentUser.ID"].ToString())获取。
  - 使用TextBox标签的For属性（For=Title.Name）时，无需设置Required=true和ShowRedStar=true，这两个属性会从Title模型的特性中读取并设置。
  - 用户列表页面的触发器输入框，由于设置了OnTrigger2ClickFields=Panel1，因此无需额外传入参数new Parameter("ttbSearchMessage","F.ui.ttbSearchMessage.getValue()")。
  - 菜单编辑页面，如果指定的浏览权限名称错误，则弹出框提示（浏览权限 XXX 不存在！）。
  - [Dapper]Menu模型类，在Dapper版有ViewPowerName属性，而EFCore版没有ViewPowerName属性。
  - [Dapper]使用依赖注入添加数据库连接实例。
    - 新建一个实现了IDisposable接口的类MyConnectionService，在Dispose中清除数据库连接实例。
    - 在Startup.cs中使用AddScoped注册服务（每个请求共享一个数据库连接实例）。
      - services.AddScoped(ctx => new MyConnectionService(dbConnectionString));
    - 在BaseModel.cs类中获取数据库连接实例。
      - FineUI.Core.PageContext.GetRequestService<MyConnectionService>();
      - 由于同时需要在静态函数和实例函数中调用，所以通过当前请求上下文获取服务对象。
      - 如果仅需要在实例函数中调用，可以通过类的构造函数注入。
  - [EFCore]新增IKey2ID接口。
    - RolePower、RoleUser、TitleUser实现了IKey2ID接口，BaseModel新增AddEntities2、ReplaceEntities2方法。
    - 编辑用户页面。
      - ReplaceEntities<Role>(_user.Roles, roleIDs); 改为：ReplaceEntities2<RoleUser>(_user.RoleUsers, roleIDs, _user.ID);
      - _user.Dept = Attach<Dept>(Convert.ToInt32(hfSelectedDept)); 改为：_user.DeptID = Convert.ToInt32(hfSelectedDept);
    - 新增用户页面。
      - AddEntities<Role>(user.Roles, roleIDs); 改为：AddEntities2<RoleUser>(roleIDs, CurrentUser.ID);

  - Menu模型类，Dapper版有ViewPowerName属性，而EFCore版没有ViewPowerName属性。
    - 列表页面：
      - EFCore版：<f:RenderField For="Menus.First().ViewPower.Name"></f:RenderField>
      - Dapper版：<f:RenderField For="Menus.First().ViewPowerName"></f:RenderField>
    - 编辑页面：
      - EFCore版：<f:TextBox For="Menu.ViewPowerID" Text="@(Model.Menu.ViewPower == null ? "" : Model.Menu.ViewPower.Name)" Name="ViewPowerName"></f:TextBox>
      - Dapper版：<f:TextBox For="Menu.ViewPowerName"></f:TextBox>
    - 编辑页面后台：
      - EFCore版：OnPostMenuEdit_btnSaveClose_ClickAsync(string ViewPowerName)
      - Dapper版：OnPostMenuEdit_btnSaveClose_ClickAsync()，通过 Menu.ViewPowerName 获取用户的输入值。
  - [EFCore]不支持没有实体类来表示联接表的多对多关系。
    - 无有效负载的多对多联接表有时称为纯联接表 (PureJoinTable)。
    - 数据模型开始时很简单，随着内容的增加，纯联接表 (PJT) 通常会发展为有效负载的联接表。
    - 新增实体类：RolePower、RoleUser、TitleUser。
    - 更新FineUI.Core.AppBoxContext中的OnModelCreating，包含多对多，一对多，单个导航等定义。
    - 更新模型类User，删除Roles和Titles导航属性，新增RoleUsers和TitleUsers导航属性。
  - [EFCore]更新FineUI.Core.AppBoxDatabaseInitializer，并在程序启用阶段调用（Program.cs）。
  - [EFCore]使用依赖注入添加数据库连接实例。
    - 在Startup.cs的ConfigureServices中，通过AddDbContext来注册EFCore服务。
    - 在BaseModel.cs类中获取数据库连接实例。
      - FineUI.Core.PageContext.GetRequestService<FineUI.Core.AppBoxContext>();
      - 由于同时需要在静态函数和实例函数中调用，所以通过当前请求上下文获取服务对象。
      - 如果仅需要在实例函数中调用，可以通过类的构造函数注入。
  - [EFCore]在页面初始化查询中，添加对AsNoTracking()的调用。
    - 如果返回的实体未在当前上下文中更新（未调用SaveChanges），AsNoTracking方法将会提升性能。
  - [EFCore]由于现在需要用关联表表示多对多关系，所以需要对之前的代码进行重构。
    - 重构BaseModel中的GetRolePowerNames方法。
      - db.Roles.Include(r => r.Powers).Where(r => roleIDs.Contains(r.ID)).ToList();
      - 改为：
      - db.Roles.Include(r => r.RolePowers).ThenInclude(rp => rp.Power).Where(r => roleIDs.Contains(r.ID)).ToList();
    - 重构用户编辑页面初始化代码。
      - DB.Users.Include(u => u.Roles).Where(m => m.ID == id).FirstOrDefault();
      - String.Join(",", CurrentUser.Roles.Select(r => r.Name).ToArray());
      - 改为：
      - await DB.Users.Include(u => u.RoleUsers).ThenInclude(ru => ru.Role).Where(m => m.ID == id).FirstOrDefaultAsync();
      - String.Join(",", CurrentUser.RoleUsers.Select(ru => ru.Role.Name).ToArray());
    - 重构职称列表页面删除行代码。
      - DB.Users.Where(u => u.Titles.Any(r => r.ID == deletedRowID)).Count();
      - 改为：
      - await DB.Users.Where(u => u.TitleUsers.Any(r => r.TitleID == deletedRowID)).CountAsync();

## 历史：AppBoxMvc/Pro 的更新记录

## v5.2（2018-09-20）

- 升级到FineUIMvc/Pro（社区版）v5.2.0。
- Net Framework框架由4.0升级为4.5.2。
- 修正表格改变每页记录数后，序号列可能显示异常的问题（何少波-3RJIU3F）。
- 系统配置->表格每页显示记录数，由数字输入框改为下拉列表。
- ORM工具由EntityFramework改为Dapper。
  - 简单封装插入和更新：ExecuteInsert，ExecuteUpdate
  - 简单封装分页和排序：Count，Sort，SortAndPage

## v5.0（2018-04-23）

- 升级到FineUIMvc/Pro（社区版）v5.0.0。
- 删除部分列表页面中Form标签的Height和BodyPadding定义。
- 调整弹出窗体高度，以及部分表格列宽度。
- 首页TabStrip增加ShowInkBar=true属性。
  - 通过CSS样式去除激活选项卡的背景色。
- 为角色权限管理页面增加右键菜单。

## v4.1.0（2018-02-13）

- FineUIMvc升级为v4.1.0.1。
  - 字符串替换：[ui-icon]->[f-icon], [ui-state]->[f-state], [ui-widget]->[f-widget]。
  - 更新目录：res/themes。
- 图标字体更新为内置图标字体。
  - IconFont.Home -> IconFont._Home
  - IconFont.Download -> IconFont._Download
  - IconFont.Question -> IconFont._Question
  - IconFont.Bank -> IconFont._Skin
  - IconFont.SignOut -> IconFont._SignOut
  - IconFont.Refresh -> IconFont._Refresh
  - IconFont.Expand -> IconFont._Maximize（f-iconfont-maximize, f-iconfont-restore）
- 后台接受表格字段参数由JArray Grid1_fields改为string[] Grid1_fields。
- 从角色职称部门中移除用户时，接受参数由JArray deletedUserIDs改为string[] deletedUserIDs。
- 添加用户到角色职称部门时，接受参数由JArray selectedRowIDs改为int[] selectedRowIDs。
- 用户列表页面删除用户时，接受参数由JArray deletedRowIDs改为int[] deletedRowIDs。
- 编辑菜单项保存时，出错（The property value of 'Power.ID' on one end of a relationship
  - do not match the property value of 'Menu.ViewPowerID' on the other end）
  - MenuEdit_btnSaveClose_Click中增加对menu.ViewPowerID的赋值，以及对传入参数ViewPowerName为空的处理。
- 主题仓库增加自定义纯色和自定义背景主题。
- 更新首页标签（Views/Home/Index.cshtml），支持新增的纯色主题和图片背景主题。

## v1.2.0（2017-03-13）

- FineUIMvc升级为v1.2.0。
- 修正改变分页大小时没有更新分页工具栏的问题（jacky_j-9259）。
- 修正菜单管理页面不能将ViewPower设为空的问题（sorachen-9269）。
- 修正弹出窗体可能会出现横向滚动条的问题，需要为顶层面板设置布局（张文-9275）。
- 用户新增和编辑页面可以在弹出窗体中修改角色、部门和职务。
- 修正越权访问页面时会报错[服务器无法在发送 HTTP 标头之后追加标头]（龙涛软件-9374）。
  - 页面能正确返回[您无权访问此页面！]，但VS调试时报错[服务器无法在发送 HTTP 标头之后追加标头]。

## v1.0.0（2017-01-12）

- 数据模型增加外键属性，比如User.DeptID属性，并更新DapperContext（Map->HasForeignKey）。
- 菜单列表页面增加图标列。
- 两种权限控制方式，CheckPower注解（自定义方法过滤器CheckPowerAttribute）或者CheckPower方法。
- 角色用户页面，角色表格排序后保持之前的选中项。
- 列表页面所有事件（触发输入框、表格排序和分页、每页记录数改变、窗体关闭、行删除）共用一个后台处理函数。
- 自定义视图模型GridPagingInfo，来简化表格分页排序的数据处理和传递。
- POST方法的安全验证防止跨站请求伪造（CSRF）和过度提交（Over-Posting）。
- 编辑用户数据时更新部分字段，其他编辑是全部更新。
- 实现AppBox v6.0的功能。

## 历史：AppBox 的更新记录

## v6.0（2016-10-27）

- FineUI升级为v6.0.0。
- 首页左侧树控件的EnableIcons设为true。
- 删除表格ClearSelectedRowsAfterPaging属性，请使用ClearSelectionBeforePaging属性。
- 用户编辑时所属角色、部门和职称，改为TwinTriggerBox控件，增加清空图标（JS脚本实现）。

## v4.1（2014-07-21）

- 项目更新为.Net40。
- Entity Framework升级为v6.1。
- FineUI升级为v4.1.0。
- 如果手风琴内树节点为空，则不显示手风琴项。
- 修正新增菜单时JavaScript错误。
- 为登录后首页的外部面板增加AutoScroll="true"，使其在适当的时候出现纵向滚动条。
- 重构admin/role_power.aspx页面的客户端脚本，使其更简洁和准确。
- 修正对关联属性排序时出错的问题（忽然白发、☆☆αβ☆☆）。
- 修正角色权限管理页面按钮未进行权限管理的问题。
- 优化跨页保持选中项的实现。
  - 影响页面role_user_addnew.aspx、dept_user_addnew.aspx、title_user_addnew.aspx。
  - 在Page_Load中而不是在控件事件中调用SyncSelectedRowIndexArrayToHiddenField。
- 恢复登录后首页的大标题栏。

## v4.0（2014-03-03）

- Entity Framework升级为v6.0（Code First开发模式）。
- FineUI升级为v4.0正式版。
- 自定义首页顶部工具栏样式（仅用于Neptune主题，按钮背景透明）。
- 更新左右分栏页面样式（注意BodyPadding的使用，例如admin/title_user.aspx页面）。
- 更新配置后，点击确定按钮刷新框架页面。
- 更新角色权限管理页面，优化全选/反选按钮，并增加表格的右键菜单。
- 修正表格PageIndex越界的问题。
  - 重现：用户表转到第二页，搜索一个关键字，如果结果只有几个，则显示为空。
  - 修正办法：PageBase中SortAndPage方法中对PageIndex的有效性进行验证。
- 添加用户到职称角色部门时可以在用户名、中文名中检索。

## v3.0.2（2013-08-29）

- 修正每次启用都会重新创建数据库的问题。
- 修正无法添加菜单的问题。
- 修正编辑菜单后不更新菜单列表的问题。
- 修正菜单管理和部门管理中，打开顶级节点修改页面出错的问题。
- 修正菜单管理和部门管理中，删除有子节点的父节点直接报错的问题。
- 左侧菜单列表中，不显示空目录，也不显示拥有空目录的空目录。
- 修正新增权限后，新增的权限不显示在角色权限页面。

## v3.0.1（2013-08-28）

- 修正每次启用都会重新创建数据库的问题。
- 修正无法添加菜单的问题。
- 修正编辑菜单后不更新菜单列表的问题。

## v3.0（2013-08-28）

- 基于最新的FineUI v3.3和Entity Framework v5.0（Code First开发模式）。
- 扁平化的权限设计，简单高效易于扩展。
- 修正弹出框高度变小时表单仍然不出现滚动条的问题。
- 超级管理员帐号（admin）不受系统权限控制，默认拥有所有权限。
- 只有超级管理员自己才能编辑自己。

## v2.1（2012-12-17）

- 修正新增和编辑部门时，无法选择顶级部门的BUG。
- 修正用户表的date类型sqlserver2005无法识别的问题。
- 修正工程的编译错误。
- 修正可以添加重复用户名的用户；用户名不可编辑；性别为必填项。
- 编辑新增菜单时，模块名称只能选择，这个名称是从代码来的。

## v2.0（2012-12-09）

- 配置项增加“菜单样式”和“网站主题”。
- 表格增加“每页记录数”过滤条件。
- 删除“角色菜单管理”菜单，现在从“角色模块管理”获得角色拥有的菜单列表。
- 现在需要在ModuleTypeHelper代码中定义模块的标题；“菜单管理”中的菜单标题可以从模块标题同步过来。
- 一个用户可以属于多个角色；删除角色之前需要先清空属于此角色的用户列表。
- 优化编辑用户和新增用户窗体。
  - 性别从下拉列表变为单选框列表。
  - 所属角色从下列列表变为触发器输入框，在新窗口中选择用户所属的角色。
  - 所属部门从下列列表变为触发器输入框，在新窗口中选择用户所属的部门。
  - 增加拥有职称字段，同样为触发器输入框。
- 添加用户到当前角色窗体，可以在表格中跨页选择多个用户。
- 用户表增加了一些字段，比如分机号、住址、身份证号、到职日期等。
- 一个用户只能属于一个部门；删除部门之前需要先清空属于此部门的用户列表。
- 一个用户可以拥有多个职称；删除职称之前需要先清空拥有此职称的用户列表。

## v1.0（2012-06-24）

- 第一个正式版本。
- AppBox为捐赠软件，请捐赠作者来获取全部源代码（http://fineui.com/donate/）。

## v0.1.0（2009-09-26）

- 第一个开源版本。

注：AppBox 第一个开源版本于 2009-09-26 发布。

Copyright (C) 2008-2026 合肥三生石上软件有限公司
