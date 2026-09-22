// AppBox 公共脚本：由公共布局或页面基类加载，主页面和 iframe 子页面都可使用。

// 关闭当前所在的弹出窗口（各编辑/新建子页“关闭”按钮的 ClickHandler）
function onCloseActiveWindowClick(event) {
    F.activeWindow.hide();
}
