


// 显示居中通知对话框（messageIcon: 'information', 'warning', 'question', 'error', 'success'）
function showCenterNotify(message, messageIcon) {
    F.notify({
        message: message,
        messageIcon: messageIcon || '',
        modal: true,
        hideOnMaskClick: true,
        header: false,
        displayMilliseconds: 3000,
        positionX: 'center',
        positionY: 'center',
        messageAlign: 'center',
        minWidth: 200
    });
}


F.ready(function () {


    F.beforeAjaxError(function (data, textStatus, xhr) {

        var errorMessage = '发生未知错误，是否刷新页面并重试？';
        var errorAction = 'reload';

        if (textStatus === 'timeout') {
            errorMessage = '客户端请求超时，是否刷新页面并重试？';
            errorAction = 'reload';
        } else {
            if (xhr.status === 401) {
                // 通过xhr.status来判断是否是 401 Unauthorized 错误
                errorMessage = '身份验证失败，是否跳转到登录页面？';
                errorAction = "redirect";

                // 服务端主动拒绝时（如账号被禁用）会在响应体里给一句纯文本说明，比通用文案有用；
                // 而认证中间件那条路返回的是整页 HTML，不能原样弹出来，所以先判断是不是一句短的人话
                if (typeof data === 'string' && data.length > 0 && data.length < 200 && data.indexOf('<') < 0) {
                    errorMessage = data + '是否跳转到登录页面？';
                }
            } else if (xhr.status === 500) {
                errorMessage = '服务器异常，是否刷新页面并重试？';
                errorAction = 'reload';
            }
        }
        
        // F.confirm是异步函数
        F.confirm({
            message: errorMessage,
            ok: function () {
                if (errorAction === 'reload') {
                    // 刷新顶层页面
                    top.window.location.reload();
                } else {
                    // 顶层跳转到登录页面
                    top.window.location.href = "/Login";
                }
            },
            cancel: function () {
                // 继续留在当前页面，啥也不做
            }
        });

        // 需要显式的返回 false，来阻止执行返回的脚本
        return false;
    });

});


// 关闭当前所在的弹出窗口（子页「关闭」按钮的 ClickHandler）
function onCloseActiveWindowClick(event) {
    F.activeWindow.hide();
}
