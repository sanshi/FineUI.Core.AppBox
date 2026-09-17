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

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentUser = await DB.Users
                .Include(u => u.Roles)
                .Include(u => u.Dept)
                .Include(u => u.Titles)
                .Where(u => u.ID == id).AsNoTracking().FirstOrDefaultAsync();

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
