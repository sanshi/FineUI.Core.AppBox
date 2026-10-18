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
    [CheckPower(Name = "CorePowerEdit")]
    public partial class PowerEditModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Power Power { get; set; }

        private Power _storedPower;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            _storedPower = await DB.Powers
                .FirstOrDefaultAsync(item => item.ID == id);

            if (_storedPower == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                Power = _storedPower;
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
            _storedPower.Name = Power.Name;
            _storedPower.GroupName = Power.GroupName;
            _storedPower.Title = Power.Title;
            _storedPower.Remark = Power.Remark;
            await DB.SaveChangesAsync();

            // 权限是按名字判定的，改了名各会话都要重新解析
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
