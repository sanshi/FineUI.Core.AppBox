using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FineUI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreRoleEdit")]
    public partial class RoleEditModel : AdminPageBase
    {
        #region Fields

        [BindProperty]
        public Role Role { get; set; }

        private Role _storedRole;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            var id = GetQueryIntValue("id");
            _storedRole = await DB.Roles
                .FirstOrDefaultAsync(item => item.ID == id);


            if (_storedRole == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                Role = _storedRole;
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }

        #endregion


        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            // 更新 Page_Init 已按当前 URL 校验的实体，表单中的主键不决定保存目标。
            _storedRole.Name = Role.Name;
            _storedRole.Remark = Role.Remark;
            await DB.SaveChangesAsync();

            // 角色本身变了，稳妥起见让各会话重新解析一次权限
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
