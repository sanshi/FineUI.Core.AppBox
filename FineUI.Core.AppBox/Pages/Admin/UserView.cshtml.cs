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
    [CheckPower(Name = "CoreUserView")]
    public partial class UserViewModel : BaseAdminModel
    {
        #region Fields

        public User CurrentUser { get; set; }

        #endregion

        #region Page_Get

        public async Task Page_GetAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            CurrentUser = await DB.Users
                .Include(u => u.Roles)
                .Include(u => u.Dept)
                .Include(u => u.Titles)
                .Where(u => u.ID == id).AsNoTracking().FirstOrDefaultAsync();

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
                // 用户所属角色
                labRoles.Text = String.Join(",", CurrentUser.Roles.Select(r => r.Name).ToArray());

                // 用户的职称列表
                labTitles.Text = String.Join(",", CurrentUser.Titles.Select(t => t.Name).ToArray());


                // 用户所属的部门
                if (CurrentUser.DeptID != null)
                {
                    labDept.Text = CurrentUser.DeptName;
                }

                labEnabled.Text = CurrentUser.Enabled ? "启用" : "禁用";
            }
        }

        #endregion


        #region Events

        

        #endregion
    }
}
