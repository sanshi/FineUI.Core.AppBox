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

        #endregion

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Power = await DB.Powers
                .Where(m => m.ID == id).AsNoTracking().FirstOrDefaultAsync();

            if (Power == null)
            {
                return Content("无效参数！");
            }

            return Page();
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

            DB.Entry(Power).State = EntityState.Modified;
            await DB.SaveChangesAsync();

            // 权限是按名字判定的，改了名各会话都要重新解析
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
