using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FineUI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CorePowerNew")]
    public partial class PowerNewModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Power Power { get; set; }

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

            DB.Powers.Add(Power);
            await DB.SaveChangesAsync();

            // 新增了权限点，超级管理员的权限视图是权限表全集，也跟着变了
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion


    }
}
