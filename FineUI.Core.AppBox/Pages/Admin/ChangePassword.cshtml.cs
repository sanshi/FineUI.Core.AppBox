using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



using FineUI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace FineUI.Core.AppBox.Pages.Admin
{
    public partial class ChangePasswordModel : BaseAdminModel
    {
        #region Fields



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

        protected async Task btnSave_ClickAsync(object sender, EventArgs e)
        {
            int? id = GetIdentityID();

            // 检查当前密码是否正确
            string oldPass = tbxOldPassword.Text.Trim();
            string newPass = tbxNewPassword.Text.Trim();
            string confirmNewPass = tbxConfirmNewPassword.Text.Trim();

            if (newPass != confirmNewPass)
            {
                tbxConfirmNewPassword.MarkInvalid("确认密码和新密码不一致！");
            }
            else
            {
                User user = await DB.Users.Where(u => u.ID == id).AsNoTracking().FirstOrDefaultAsync();

                if (user != null)
                {
                    if (!PasswordUtil.ComparePasswords(user.Password, oldPass))
                    {
                        tbxOldPassword.MarkInvalid("当前密码不正确！");
                    }
                    else
                    {
                        user.Password = PasswordUtil.CreateDbPassword(newPass);
                        await DB.SaveChangesAsync();

                        Alert.ShowInTop("修改密码成功！");
                    }
                }
            }
        }


        #endregion


    }
}