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
    public partial class UserChangePasswordModel : AdminPageBase
    {
        #region Fields

        public User CurrentUser { get; set; }

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            var id = GetQueryIntValue("id");
            CurrentUser = await DB.Users.FirstOrDefaultAsync(user => user.ID == id);

            if (CurrentUser == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (CurrentUser.Name == "admin" && GetIdentityName() != "admin")
            {
                FineUI.Core.PageContext.AbortPage("你无权编辑超级管理员！", ActiveWindow.HideCommand());
                return;
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
            // Page_Init 已按当前 URL 校验目标，密码框和隐藏字段都不能改写目标主键。
            CurrentUser.Password = PasswordUtil.CreateDbPassword(tbxPassword.Text.Trim());
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion
    }
}
