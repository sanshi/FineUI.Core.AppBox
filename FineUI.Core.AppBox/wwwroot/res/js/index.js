
var SIDEBAR_WIDTH_CONSTANT = 260;
// _sidebarWidth变量会随着用户拖动分隔条而改变
var _sidebarWidth = SIDEBAR_WIDTH_CONSTANT;

// 设置长期存在的Cookie
function setCookie(name, value) {
    F.cookie(name, value, {
        expires: 100  // 单位：天
    });
}

// 点击主题仓库
function onThemeSelectClick(event) {
    var windowThemeRoller = F(PARAMS.windowThemeRoller);
    windowThemeRoller.show();
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
    var foldButton = F(PARAMS.btnCollapseSidebar);
    return foldButton.iconFont === 'f-iconfont-unfold';
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
    var logoEl = sidebarRegion.el.find('.logo');

    var currentCollapsed = getFoldButtonStatus();
    if (F.isUND(collapsed)) {
        collapsed = !currentCollapsed;
    } else {
        if (currentCollapsed === collapsed) {
            return;
        }
    }

    F.noAnimation(function () {

        setFoldButtonStatus(collapsed);

        if (!collapsed) {
            logoEl.removeClass('short').text(logoEl.attr('title'));
            sidebarRegion.setWidth(_sidebarWidth);
            // 启用分隔条拖动
            sidebarRegion.setSplitDraggable(true);

            // 禁用树微型模式
            treeMenu.miniMode = false;
            // 重新加载树菜单
            treeMenu.loadData();
        } else {
            logoEl.addClass('short').text('A');
            sidebarRegion.setWidth(60);
            // 禁用分隔条拖动
            sidebarRegion.setSplitDraggable(false);

            // 启用树微型模式
            treeMenu.miniMode = true;
            // 重新加载树菜单
            treeMenu.loadData();
        }
    });
}

// 侧边栏分隔条拖动事件
function onSidebarSplitDrag(event) {
    _sidebarWidth = this.width;
}


// 下载源代码
function onDownloadClick(event) {
    window.open('https://fineui.com/fans/', '_blank');
}

// 点击标题栏工具图标 - 刷新
function onToolRefreshClick(event) {
    var mainTabStrip = F(PARAMS.mainTabStrip);

    var activeTab = mainTabStrip.getActiveTab();
    if (activeTab.iframe) {
        var iframeWnd = activeTab.getIFrameWindow();
        iframeWnd.location.reload();
    }
}

// 点击标题栏工具图标 - 最大化
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


// 添加示例标签页（通过href在树中查找）
// href: 选项卡对应的网址
// actived: 是否激活选项卡（默认为true）
function addExampleTabByHref(href, actived) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var treeMenu = F(PARAMS.treeMenu);

    F.addMainTabByHref(mainTabStrip, treeMenu, href, actived);
}


// 添加示例标签页
// tabOptions: 选项卡参数
// tabOptions.id： 选项卡ID
// tabOptions.iframeUrl: 选项卡IFrame地址 
// tabOptions.title： 选项卡标题
// tabOptions.icon： 选项卡图标
// tabOptions.createToolbar： 创建选项卡前的回调函数（接受tabOptions参数）
// tabOptions.refreshWhenExist： 添加选项卡时，如果选项卡已经存在，是否刷新内部IFrame
// tabOptions.iconFont： 选项卡图标字体
// tabOptions.actived: 是否激活选项卡（默认为true）
// tabOptions.moveToEnd: 将新增选项卡移到尾部（如果选项卡已存在，则不改变位置）
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


// 点击「系统帮助」下拉菜单项：选项卡参数由服务端写在菜单项的 data-* 属性上
function onHelpMenuClick(event) {
    var item = this;
    addExampleTab(item.getAttr('data-id'), item.getAttr('data-url'), item.getAttr('data-text'));
}


// 关闭选中标签页
function removeActiveTab() {
    var mainTabStrip = F(PARAMS.mainTabStrip);

    var activeTab = mainTabStrip.getActiveTab();
    // 关闭选项卡（关闭时会根据选项卡的removeOnClose属性决定是否移除选项卡实例）
    activeTab.close();
}

// 获取当前激活选项卡的ID
function getActiveTabId() {
    var mainTabStrip = F(PARAMS.mainTabStrip);

    var activeTab = mainTabStrip.getActiveTab();
    if (activeTab) {
        return activeTab.id;
    }
    return '';
}

// 激活选项卡，并刷新其中的内容，示例：表格控件->杂项->在新标签页中打开（关闭后刷新父选项卡）
function activeTabAndRefresh(tabId) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var targetTab = mainTabStrip.getTab(tabId);
    var oldActiveTab = mainTabStrip.getActiveTab();

    if (targetTab) {
        targetTab.activate();
        targetTab.refreshIFrame();

        // 关闭选项卡（关闭时会根据选项卡的removeOnClose属性决定是否移除选项卡实例）
        oldActiveTab.close();
    }
}

// 激活选项卡，并刷新其中的内容，示例：表格控件->杂项->在新标签页中打开（关闭后更新父选项卡中的表格）
function activeTabAndUpdate(tabId, param1) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var targetTab = mainTabStrip.getTab(tabId);
    var oldActiveTab = mainTabStrip.getActiveTab();

    if (targetTab) {
        targetTab.activate();
        targetTab.getIFrameWindow().updatePage(param1);

        // 关闭选项卡（关闭时会根据选项卡的removeOnClose属性决定是否移除选项卡实例）
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

    // 初始化主框架中的树控件和选项卡互动，以及地址栏的更新
    // treeMenu： 主框架中的树控件实例，或者内嵌树控件的手风琴控件实例
    // mainTabStrip： 选项卡实例
    // options: 参数
    // options.updateHash： 切换Tab时，是否更新地址栏Hash值（默认值：true）
    // options.refreshWhenExist： 添加选项卡时，如果选项卡已经存在，是否刷新内部IFrame（默认值：true）
    // options.refreshWhenTabChange: 切换选项卡时，是否刷新内部IFrame（默认值：false）
    // options.maxTabCount: 最大允许打开的选项卡数量
    // options.maxTabMessage: 超过最大允许打开选项卡数量时的提示信息
    // options.beforeNodeClick: 节点点击事件之前执行（返回false则不执行点击事件）
    // options.beforeTabAdd: 添加选项卡之前执行（返回false则不添加选项卡）
    // options.moveToEnd: 将新增选项卡移到尾部（否则会显示在上一次关闭前的位置）（默认值：true）
    // options.syncSelectedNode: 切换选项卡时，是否同步选中的树节点（默认值：true）
    var initOptions = {
        maxTabCount: 30,
        maxTabMessage: '请先关闭一些选项卡（最多允许打开 30 个）！'
    };

    F.initTreeTabStrip(treeMenu, mainTabStrip, initOptions);

});

