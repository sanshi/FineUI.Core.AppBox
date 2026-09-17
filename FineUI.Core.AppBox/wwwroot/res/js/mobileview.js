
function isSmallWindowWidth() {
    var windowWidth = $(window).width();
    return windowWidth < 992;
}

function checkMobileView() {
    var bodyEl = $('body');
    var isMobileView = bodyEl.hasClass('mobileview');

    if (isSmallWindowWidth()) {
        if (!isMobileView) {
            // 如果窗体宽度小于992px，但是尚未启用移动视图，则启用
            bodyEl.addClass('mobileview');
            F.viewPortExtraWidth = SIDEBAR_WIDTH_CONSTANT;
            
            // 更新为移动视图之前，要先展开左侧面板
            _sidebarWidth = SIDEBAR_WIDTH_CONSTANT;
            if (getFoldButtonStatus()) {
                // 如果当前处于折叠状态
                toggleSidebar(false);
            } else {
                // 如果当前处于展开状态，将左侧面板的宽度还原为初始值
                F(PARAMS.sidebarRegion).setWidth(_sidebarWidth);
            }
            setFoldButtonStatus(true);
            F(PARAMS.mainPanel).doLayout();
        }
    } else {
        if (isMobileView) {
            // 如果窗体宽度大于992px，但是已经启用移动视图，则禁用
            bodyEl.removeClass('mobileview');
            F.viewPortExtraWidth = 0;

            // 更新为正常视图之前，要先隐藏侧边栏和遮罩层
            hideMask();
            setFoldButtonStatus(false);
            F(PARAMS.mainPanel).doLayout();
        }
    }

    // 第一次检查时不启用动画
    if (!bodyEl.hasClass('mobileview-transition')) {
        bodyEl.addClass('mobileview-transition');
    }

}

// 移动视图 - 隐藏侧边栏和遮罩层
function hideMask() {
    $('.mainpanel').removeClass('showsidebar');
    $('.bodyregion .showsidebar-mask').hide();
    setFoldButtonStatus(true);
}

// 移动视图 - 显示侧边栏和遮罩层
function showMask() {
    $('.mainpanel').addClass('showsidebar');
    $('.bodyregion .showsidebar-mask').show();
    setFoldButtonStatus(false);
}



// 点击折叠/展开按钮
function onFoldClick(event) {
    if (isSmallWindowWidth()) {
        var sidebarregionEl = $('.sidebarregion');
        var bodyregionEl = $('.bodyregion');

        // 创建遮罩层
        var maskEl = bodyregionEl.find('.showsidebar-mask');
        if (!maskEl.length) {
            maskEl = $('<div class="showsidebar-mask"></div>').appendTo(bodyregionEl.find('>.f-panel-bodyct'));
            maskEl.on('click', function () {
                if (isSmallWindowWidth()) {
                    hideMask();
                }
            });

            sidebarregionEl.on('click', '.leftregion .f-tree-node-leaf', function () {
                if (isSmallWindowWidth()) {
                    hideMask();
                }
            });
        }

        if (getFoldButtonStatus()) {
            showMask();
        } else {
            hideMask();
        }

    } else {
        toggleSidebar();
    }
}


F.ready(function () {
    // 页面打开检查是否移动视图
    checkMobileView();

    // 根据屏幕尺寸检查是否移动视图
    F.windowResize(function () {
        checkMobileView();
    });

    // 如果初始启用移动视图，则检查折叠按钮的状态
    if (isSmallWindowWidth()) {
        setFoldButtonStatus(true);
    }
});