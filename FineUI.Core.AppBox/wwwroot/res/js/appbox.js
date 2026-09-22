// AppBox 专属脚本：只同步到 5 个 AppBox 仓库，不进 Examples / EmptyProject / QuickStart。
// 与 common.js 同层加载（Core 的 Pages/Shared/_Layout.cshtml、Java 的 templates/shared/layout.html、
// Pro 的 Business/PageBase.cs）——这些页面大多是 iframe 独立文档，只在主页面挂这个文件会漏掉它们。
//
// index.js 保持"所有示例项目共用"的定位，AppBox 独有的回调一律写在这里。

// 关闭当前所在的弹出窗口（各编辑/新建子页「关闭」按钮的 ClickHandler）。
// 原本在 AppBox 自己的 common.js 里；共享 common.js 没有这个函数，而它被 70 多个子页引用，
// 所以随 AppBox 专属层一起走，不能丢。
function onCloseActiveWindowClick(event) {
    F.activeWindow.hide();
}

// 顶栏「下载源码」按钮：跳到社区版下载页
function onDownloadClick(event) {
    window.open('https://fineui.com/fans/', '_blank');
}

// 选项卡工具条「最大化」：把左侧栏和顶部面板一起收起/展开，并切换按钮图标
function onToolMaximizeClick(event) {
    var topPanel = F(PARAMS.topPanel);
    var sidebarRegion = F(PARAMS.sidebarRegion);

    var currentTool = this;
    F.noAnimation(function () {
        if (currentTool.iconFont === 'f-iconfont-maximize') {
            currentTool.setIconFont('f-iconfont-restore');

            sidebarRegion.collapse();
            topPanel.collapse();
        } else {
            currentTool.setIconFont('f-iconfont-maximize');

            sidebarRegion.expand();
            topPanel.expand();
        }
    });
}

// 用户菜单里的「系统帮助」：按菜单项上带的 data-* 属性打开一个选项卡
function onHelpMenuClick(event) {
    var item = this;
    addExampleTab(item.getAttr('data-id'), item.getAttr('data-url'), item.getAttr('data-text'));
}
