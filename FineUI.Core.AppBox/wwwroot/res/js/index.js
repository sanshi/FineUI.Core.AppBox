// 入口页公共脚本：只放 Examples、EmptyProject、QuickStart 和 AppBox 都能使用的行为。

// 获取后台定义的侧边栏宽度
var SIDEBAR_WIDTH_CONSTANT = parseInt($('html').css('--sidebar-width'), 10);
// 获取后台定义的侧边栏微型模式宽度
var SIDEBAR_MINIMODE_WIDTH_CONSTANT = parseInt($('html').css('--sidebar-minimode-width'), 10);

// _sidebarWidth变量会随着用户拖动分隔条而改变
var _sidebarWidth = SIDEBAR_WIDTH_CONSTANT;

// 点击主题仓库
function onThemeSelectClick(event) {
    F(PARAMS.windowThemeRoller).show();
}

// 点击折叠/展开按钮
function onFoldClick(event) {
    toggleSidebar();
}

// 设置折叠按钮的状态
function setFoldButtonStatus(collapsed) {
    var foldButton = F(PARAMS.btnCollapseSidebar);
    if (collapsed) {
        foldButton.setIconFont('f-iconfont-unfold');
    } else {
        foldButton.setIconFont('f-iconfont-fold');
    }
}

// 获取折叠按钮的状态
function getFoldButtonStatus() {
    return F(PARAMS.btnCollapseSidebar).iconFont === 'f-iconfont-unfold';
}

// 展开侧边栏
function expandSidebar() {
    toggleSidebar(false);
}

// 折叠侧边栏
function collapseSidebar() {
    toggleSidebar(true);
}

// 折叠/展开侧边栏
function toggleSidebar(collapsed) {
    var sidebarRegion = F(PARAMS.sidebarRegion);
    var treeMenu = F(PARAMS.treeMenu);
    var sideheaderEl = $('#sideheader');
    var currentCollapsed = getFoldButtonStatus();

    if (F.isUND(collapsed)) {
        collapsed = !currentCollapsed;
    } else if (currentCollapsed === collapsed) {
        return;
    }

    F.noAnimation(function () {
        setFoldButtonStatus(collapsed);

        if (!collapsed) {
            sideheaderEl.removeClass('minimode');
            sidebarRegion.setWidth(_sidebarWidth);
            sidebarRegion.setSplitDraggable(true);
            treeMenu.miniMode = false;
            treeMenu.loadData();
        } else {
            sideheaderEl.addClass('minimode');
            sidebarRegion.setWidth(SIDEBAR_MINIMODE_WIDTH_CONSTANT);
            sidebarRegion.setSplitDraggable(false);
            treeMenu.miniMode = true;
            treeMenu.loadData();
        }
    });
}

// 侧边栏分隔条拖动事件
function onSidebarSplitDrag(event) {
    _sidebarWidth = this.width;
}

// 点击标题栏工具图标 - 刷新
function onToolRefreshClick(event) {
    var activeTab = F(PARAMS.mainTabStrip).getActiveTab();
    if (activeTab.iframe) {
        activeTab.getIFrameWindow().location.reload();
    }
}

// 添加标签页（通过href在树中查找）
function addExampleTabByHref(href, actived) {
    F.addMainTabByHref(F(PARAMS.mainTabStrip), F(PARAMS.treeMenu), href, actived);
}

// 添加标签页
function addExampleTab(tabOptions) {
    if (typeof (tabOptions) === 'string') {
        tabOptions = {
            id: arguments[0],
            iframeUrl: arguments[1],
            title: arguments[2],
            icon: arguments[3],
            createToolbar: arguments[4],
            refreshWhenExist: arguments[5],
            iconFont: arguments[6]
        };
    }
    F.addMainTab(F(PARAMS.mainTabStrip), tabOptions);
}

// 关闭选中标签页
function removeActiveTab() {
    F(PARAMS.mainTabStrip).getActiveTab().close();
}

// 获取当前激活选项卡的ID
function getActiveTabId() {
    var activeTab = F(PARAMS.mainTabStrip).getActiveTab();
    return activeTab ? activeTab.id : '';
}

// 激活选项卡，并刷新其中的内容
function activeTabAndRefresh(tabId) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var targetTab = mainTabStrip.getTab(tabId);
    var oldActiveTab = mainTabStrip.getActiveTab();
    if (targetTab) {
        targetTab.activate();
        targetTab.refreshIFrame();
        oldActiveTab.close();
    }
}

// 激活选项卡，并更新父选项卡中的内容
function activeTabAndUpdate(tabId, param1) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var targetTab = mainTabStrip.getTab(tabId);
    var oldActiveTab = mainTabStrip.getActiveTab();
    if (targetTab) {
        targetTab.activate();
        targetTab.getIFrameWindow().updatePage(param1);
        oldActiveTab.close();
    }
}

// 通知框
function notify(msg) {
    F.notify({
        message: msg,
        messageIcon: 'information',
        target: '_top',
        header: false,
        displayMilliseconds: 3 * 1000,
        positionX: 'center',
        positionY: 'center'
    });
}

F.ready(function () {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var treeMenu = F(PARAMS.treeMenu);
    if (!treeMenu) return;

    var initOptions = {
        maxTabCount: 30,
        maxTabMessage: '请先关闭一些选项卡（最多允许打开 30 个）！',
        // 应用部署在子路径时，子路径只属于实际 HTTP 请求地址，Hash 只保存入口页内路由。
        compactHash: true,
        beforeTabAdd: function (event, tabOptions) {
            // 关闭时移除选项卡实例，避免入口页长期运行时不断积累已经关闭的实例。
            tabOptions.removeOnClose = true;
        }
    };

    // 项目专属脚本只扩展选项，不得再次调用 F.initTreeTabStrip。
    if (typeof configureIndexPage === 'function') {
        configureIndexPage(initOptions, treeMenu, mainTabStrip);
    }

    F.initTreeTabStrip(treeMenu, mainTabStrip, initOptions);

    if (typeof onIndexPageReady === 'function') {
        onIndexPageReady(initOptions, treeMenu, mainTabStrip);
    }
});
