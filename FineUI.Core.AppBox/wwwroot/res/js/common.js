


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
