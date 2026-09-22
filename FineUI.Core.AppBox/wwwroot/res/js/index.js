
// 主选项卡标签页
var _mainTabs = F.cookie('MainTabs') || 'multi';

// 获取后台定义的侧边栏宽度
var SIDEBAR_WIDTH_CONSTANT = parseInt($('html').css('--sidebar-width'), 10);
// 获取后台定义的侧边栏微型模式宽度
var SIDEBAR_MINIMODE_WIDTH_CONSTANT = parseInt($('html').css('--sidebar-minimode-width'), 10);


// _sidebarWidth变量会随着用户拖动分隔条而改变
var _sidebarWidth = SIDEBAR_WIDTH_CONSTANT;


// 社区版下载
function onBaseDownloadClick(event) {
    // 删除按钮的徽标
    this.setBadge(false);
    window.open('http://fineui.com/fans/', '_blank');
}

// 点击企业版试用
function onApplyTrialClick(event) {
    var windowApplyTrial = F(PARAMS.windowApplyTrial);
    windowApplyTrial.show();
}

// 点击主题仓库
function onThemeSelectClick(event) {
    var windowThemeRoller = F(PARAMS.windowThemeRoller);
    windowThemeRoller.show();
}

// 点击加载动画
function onLoadingSelectClick(event) {
    var windowLoadingSelector = F(PARAMS.windowLoadingSelector);
    windowLoadingSelector.show();
}

// 设置长期存在的Cookie
function setCookie(name, value) {
    F.cookie(name, value, {
        expires: 100  // 单位：天
    });
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
    var sideheaderEl = $('#sideheader');

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
            sideheaderEl.removeClass('minimode');
            sidebarRegion.setWidth(_sidebarWidth);
            // 启用分隔条拖动
            sidebarRegion.setSplitDraggable(true);

            // 禁用微型模式，并重新加载树控件数据
            treeMenu.miniMode = false;
            treeMenu.loadData();
        } else {
            sideheaderEl.addClass('minimode');
            sidebarRegion.setWidth(SIDEBAR_MINIMODE_WIDTH_CONSTANT);
            // 禁用分隔条拖动
            sidebarRegion.setSplitDraggable(false);

            // 启用微型模式，并重新加载树控件数据
            treeMenu.miniMode = true;
            treeMenu.loadData();
        }
    });
}

// 侧边栏分隔条拖动事件
function onSidebarSplitDrag(event) {
    _sidebarWidth = this.width;
}


// 点击仅显示社区版示例
function onShowOnlyCommunityClick(event) {
    var checked = this.isChecked();

    setCookie('ShowOnlyCommunity', checked);
    top.window.location.reload();
}


function onSearchTrigger1Click(event) {
    F.removeCookie('SearchText');
    top.window.location.reload();
}

function onSearchTrigger2Click(event) {
    var ttbxSearch = this;
    if (ttbxSearch.el.hasClass('collapsed')) {
        ttbxSearch.el.removeClass('collapsed').addClass('expanded').outerWidth(200);
    } else {
        var ttbxSearchValue = ttbxSearch.getValue();
        if (ttbxSearchValue) {
            setCookie('SearchText', ttbxSearchValue);
            top.window.location.reload();
        } else if (F.cookie('SearchText')) {
            // 处于搜索状态时把文字清空再回车（或再点放大镜）：等同点清空图标，退出搜索、恢复显示全部示例。
            // 不判断 cookie 直接重载的话，展开搜索框什么都没输就回车会白刷一次页面。
            onSearchTrigger1Click(event);
        }
    }
}

function onSearchBlur(event) {
    var ttbxSearch = this;
    if (ttbxSearch.getValue()) {
        return;
    }

    var searchText = F.cookie('SearchText');
    if (searchText) {
        // 把文字清空、但没回车提交就失焦：搜索其实还生效着（菜单仍是过滤后的），
        // 所以把生效中的搜索词填回去、保持展开——搜索框展开就代表正在搜索，框里显示的就是当前生效的词。
        // 这里若收成 24px，清空图标和放大镜会把输入框挤没（界面看着像没搜索、菜单却还是过滤后的）。
        ttbxSearch.setValue(searchText);
    } else {
        // 没有生效的搜索：收回成一个 24px 的放大镜。
        ttbxSearch.el.removeClass('expanded').addClass('collapsed').outerWidth(24);
    }
}

// 点击标题栏工具图标 - 查看源代码
function onToolSourceCodeClick(event) {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var windowSourceCode = F(PARAMS.windowSourceCode);


    var activeTab = mainTabStrip.getActiveTab();
    var iframeWnd, iframeUrl;
    if (activeTab.iframe) {
        iframeWnd = activeTab.getIFrameWindow();
        iframeUrl = activeTab.getIFrameUrl();
    }

    var files = [iframeUrl];
    var sourcefilesNode = $(iframeWnd.document).find('head meta[name=sourcefiles]');
    if (sourcefilesNode.length) {
        $.merge(files, sourcefilesNode.attr('content').split(';'));
    }
    windowSourceCode.show(PARAMS.sourceUrl + '?files=' + encodeURIComponent(files.join(';')));

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

// 点击标题栏工具图标 - 在新标签页中打开
function onToolNewWindowClick(event) {
    var mainTabStrip = F(PARAMS.mainTabStrip);

    var activeTab = mainTabStrip.getActiveTab();
    if (activeTab.iframe) {
        var iframeUrl = activeTab.getIFrameUrl();
        iframeUrl = PARAMS.processNewWindowUrl(iframeUrl);
        window.open(iframeUrl, '_blank');
    }
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

// 更新首页的框架风格
function updateFramePageStyle(indexStyle) {
    $('body').removeClass('f-dark-top f-dark-left f-dark-top-left').addClass(indexStyle);
}

// 点击框架页风格
function onMenuFramePageStyleCheckChange(event, item, checked) {
    var indexStyle = item.getAttr('data-tag');

    setCookie('FramePageStyle', indexStyle);
    updateFramePageStyle(indexStyle);
}

// 点击显示模式
function onMenuDisplayModeCheckChange(event, item, checked) {
    var displayMode = item.getAttr('data-tag');

    setCookie('DisplayMode', displayMode);
    top.window.location.reload();
}

// 点击语言
function onMenuLangCheckChange(event, item, checked) {
    var lang = item.getAttr('data-tag');

    setCookie('Language', lang);
    top.window.location.reload();
}

// 点击选项卡标签页
function onMenuMainTabsCheckChange(event, item, checked) {
    var mainTabs = item.getAttr('data-tag');

    setCookie('MainTabs', mainTabs);
    top.window.location.reload();
}



// 示例数
function getExamplesCount() {
    var hfExamplesCount = F(PARAMS.hfExamplesCount);
    if (hfExamplesCount) {
        return hfExamplesCount.getValue();
    } else {
        return F(PARAMS.treeMenu).getNodeCount(true);
    }
}

var THEMES = ["Pure_Black", "Pure_Green", "Pure_Blue", "Pure_Purple", "Pure_Orange", "Pure_Red", "Default", "Metro_Blue", "Metro_Dark_Blue", "Metro_Gray", "Metro_Green", "Metro_Orange", "Black_Tie", "Blitzer", "Cupertino", "Dark_Hive", "Dot_Luv", "Eggplant", "Excite_Bike", "Flick", "Hot_Sneaks", "Humanity", "Le_Frog", "Mint_Choc", "Overcast", "Pepper_Grinder", "Redmond", "Smoothness", "South_Street", "Start", "Sunny", "Swanky_Purse", "Trontastic", "UI_Darkness", "UI_Lightness", "Vader", "custom_default", "image_black_sky", "image_green_rain", "image_green_drip", "image_green_poppy", "image_green_lotus", "image_blue_sky", "image_blue_star", "image_blue_moon", "image_blue_drip", "image_purple_fog", "image_orange_light", "image_red_dawn"];
//var THEMES = ["Pure_Black", "Pure_Green", "Pure_Blue", "Pure_Purple", "Pure_Orange", "Cupertino", "Start", "Dark_Hive", "Flick", "South_Street", "custom_default", "image_green_rain", "image_green_lotus", "image_blue_sky", "image_purple_fog"];
// 转到下一个主题
function nextThemePlease() {
    var currentTheme = F.cookie('Theme');
    var currentThemeIndex = $.inArray(currentTheme, THEMES);
    if (currentThemeIndex != -1) {
        currentThemeIndex++;
        if (currentThemeIndex >= THEMES.length) {
            currentThemeIndex = 0;
        }
        F.cookie('Theme', THEMES[currentThemeIndex], {
            expires: 100  // 单位：天
        });
        window.location.reload();
    }
}

function updateBreadcrumb(treeInstance, treeNodeId) {
    $('#breadcrumb .breadcrumb-inner').html(generateBreadcrumbHtml(treeInstance, treeNodeId));
}

function generateBreadcrumbHtml(treeInstance, nodeId) {
    var result = [];

    // 容错处理 - 找不到左侧树节点信息
    if (!nodeId) {
        return '';
    }

    var nodePathString = treeInstance.getNodePath(nodeId);
    if (!nodePathString) {
        return '';
    }

    var nodePaths = nodePathString.split('/');
    if (nodePaths && nodePaths.length) {
        var nodePathLength = nodePaths.length;
        $.each(nodePaths, function (index, item) {
            if (item === 'root') {
                //result.push('<span class="breadcrumb-root">首页</span>');
            } else {
                var cls = 'breadcrumb-text';
                if (index === nodePathLength - 1) {
                    cls += ' breadcrumb-last';
                }
                // v13.1.0 - 显式模式为TwoLevel时，顶级节点无法通过getNodeEl获取
                var itemHtml = treeInstance.getNodeData(item).text;

                //var itemHtml = treeInstance.getNodeEl(item).find('.f-tree-cell-text').html();
                result.push('<span class="' + cls + '">' + itemHtml + '</span>');
            }
        });
    }

    return result.join('<span class="breadcrumb-separator">/</span>');
}


F.ready(function () {
    var mainTabStrip = F(PARAMS.mainTabStrip);
    var treeMenu = F(PARAMS.treeMenu);
    if (!treeMenu) return;

    // 初始化框架页风格
    updateFramePageStyle(F.cookie('FramePageStyle'));

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
        maxTabMessage: '请先关闭一些选项卡（最多允许打开 30 个）！',
        // 应用部署在子路径时，子路径只属于实际 HTTP 请求地址，Hash 只保存框架页内路由。
        // F.initTreeTabStrip 会借助 F.baseUrl 补回前缀来匹配菜单并加载内嵌页面。
        compactHash: true,
        beforeNodeClick: function (event, treeNodeId) {
            var nodeEl = treeMenu.getNodeEl(treeNodeId);

            // 如果树节点有属性data-tag='pop-window1'，则弹出主题选择框
            // 如果树节点有属性data-tag='newtab'，则在浏览器的新选项卡中打开页面
            var nodeTag = nodeEl.attr('data-tag');
            var nodeData = treeMenu.getNodeData(treeNodeId);
            if (nodeTag === 'pop-window1') {
                F(PARAMS.windowThemeRoller).show();
                return false;
            } else if (nodeTag === 'newtab') {
                window.open(nodeData.href, '_blank');
                return false;
            }
        },
        beforeTabAdd: function (event, tabOptions, treeNodeId) {
            // 注意：此时尚未创建选项卡，所以第二个参数为tabOptions，而不是tabInstance
            // 手工调用F.addMainTab也会运行到这里，由于不是点击左侧树节点触发的，所以此时treeNodeId为空
            if (!treeNodeId) {
                return;
            }

            // 此时尚未创建选项卡实例，可以通过tabOptions修改选项卡的初始属性
            // v12.1.0 - 关闭时移除当前选项卡实例（注意：被移除的选项卡不能再次通过showTab或者activeTab来激活！）
            tabOptions.removeOnClose = true;


            // 如果树节点有属性data-tag='custom-title'，则修改选项卡的标题文字
            var nodeEl = treeMenu.getNodeEl(treeNodeId);
            var nodeTag = nodeEl.attr('data-tag');
            if (nodeTag === 'custom-title') {
                var parentNode = treeMenu.getParentData(treeNodeId);
                // 父节点文本可能是可信 HTML（textRaw 为真）：连同信任标记一起传，避免标题被转义成字面 HTML
                var customTitle = parentNode.text + ' - ' + nodeEl.text();
                tabOptions.title = parentNode.textRaw ? F.rawHtml(customTitle) : customTitle;
            }

            // 显示当前页面所在的路径
            updateBreadcrumb(treeMenu, treeNodeId);
        },
        tabChange: function (event, tabInstance, treeNodeId) {
            // tabInstance：当前激活的选项卡实例，如果为空则表示所有选项卡都被关闭了
            // 同步更新路径面包屑
            updateBreadcrumb(treeMenu, treeNodeId);
        }
    };

    // 是否单标签页
    if (_mainTabs === 'single') {
        $('body').addClass('maintabs-single');
        // 1. 单标签页选项卡标识符（使用默认的首页选项卡） 
        // 2. 标签页存在则直接更新（因为只有这一个标签页）
        $.extend(initOptions, {
            singleTabId: PARAMS.mainTabStrip + '_singleTab',
            refreshWhenExist: true
        });
    }

    F.initTreeTabStrip(treeMenu, mainTabStrip, initOptions);


    // 如果地址哈希值不存在，则添加响应式首页
    //:: 只有示例工程（Examples）有"响应式首页"这个选项卡，它靠 PARAMS.mainUrl / dashboardUrl 定位。
    //:: 空项目与 QuickStart 的 PARAMS 里没有这两个参数，必须跳过——否则下一行 mainHashUrl.indexOf
    //:: 会在 undefined 上抛错，而且 addExampleTabByHref(undefined) 会去开一个没有地址的选项卡。
    if (PARAMS.mainUrl) {
        var hashFragment = window.location.hash.substr(1);
        // mainUrl 是内嵌页面的完整应用内地址；启用 compactHash 后 Hash 不含 F.baseUrl。
        var mainHashUrl = PARAMS.mainUrl;
        if (initOptions.compactHash) {
            var baseUrl = (F.baseUrl || '').replace(/\/+$/, '');
            if (baseUrl && baseUrl !== '/' && mainHashUrl.indexOf(baseUrl + '/') === 0) {
                mainHashUrl = mainHashUrl.substring(baseUrl.length);
            }
        }
        if (!hashFragment || hashFragment.indexOf(mainHashUrl) >= 0) {
            addExampleTabByHref(PARAMS.dashboardUrl);
        }
    }

    //addExampleTabByHref("url");

    // 下一个主题
    //:: 页面按键事件
    $(document).on('keydown', function (event) {
        //:: Shift + L
        if (event.shiftKey && event.keyCode === 76) {
            nextThemePlease();
        }
    });

});
