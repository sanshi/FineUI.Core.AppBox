using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using FineUI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreUserChangePassword")]
    public partial class UserChangePasswordModel : BaseAdminModel
    {
        #region Fields

        public User CurrentUser { get; set; }

        #endregion

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentUser = await DB.Users.Where(m => m.ID == id).AsNoTracking().FirstOrDefaultAsync();

            if (CurrentUser == null)
            {
                return Content("无效参数！");
            }

            if (CurrentUser.Name == "admin" && GetIdentityName() != "admin")
            {
                return Content("你无权编辑超级管理员！");
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
            // 回发带回来的主键是页面上的隐藏字段，可以被篡改，所以这里要重新解析、重新查库，
            // 并且把 OnGet 里的那道判断原样再做一遍：只在打开页面时判过，等于没判
            if (!Int32.TryParse(hfUserID.Text, out int userID))
            {
                Alert.Show("无效参数！");
                return;
            }

            var item = await DB.Users.Where(u => u.ID == userID).FirstOrDefaultAsync();
            if (item == null)
            {
                Alert.Show("无效参数！");
                return;
            }

            if (item.Name == "admin" && GetIdentityName() != "admin")
            {
                Alert.Show("你无权修改超级管理员的密码！");
                return;
            }

            item.Password = PasswordUtil.CreateDbPassword(tbxPassword.Text.Trim());
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion
    }
}
