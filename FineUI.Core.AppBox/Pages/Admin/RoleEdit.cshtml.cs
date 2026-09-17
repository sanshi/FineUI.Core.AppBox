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
    public partial class RoleEditModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Role Role { get; set; }

        #endregion

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Role = await DB.Roles
                .Where(m => m.ID == id).AsNoTracking().FirstOrDefaultAsync();


            if (Role == null)
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

            DB.Entry(Role).State = EntityState.Modified;
            await DB.SaveChangesAsync();

            // 角色本身变了，稳妥起见让各会话重新解析一次权限
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
