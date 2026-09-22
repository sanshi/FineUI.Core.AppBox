// AppBox 入口页专属脚本：只由五个 AppBox 主页面加载。

// 顶栏“下载源码”按钮
function onDownloadClick(event) {
    window.open('https://fineui.com/fans/', '_blank');
}

// 选项卡工具条“最大化”：把左侧栏和顶部面板一起收起或展开
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

// 用户菜单里的“系统帮助”：按菜单项携带的数据打开选项卡
function onHelpMenuClick(event) {
    var item = this;
    addExampleTab(item.getAttr('data-id'), item.getAttr('data-url'), item.getAttr('data-text'));
}
