


using FineUI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreUserNew")]
    public partial class UserNewModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public User CurrentUser { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 初始化用户所属的角色
                await InitUserRoleAsync();

                // 初始化用户拥有的职称
                await InitUserTitleAsync();

                // 初始化用户所属的部门
                await InitUserDeptAsync();

            }
        }

        #endregion
		
		#region InitUserDept

        private async Task InitUserDeptAsync()
        {
            gridDept.DataSource = await DB.Depts.OrderBy(d => d.SortIndex).AsNoTracking().ToListAsync();
            gridDept.DataBind();
        }

        #endregion

        #region InitUserRole

        private async Task InitUserRoleAsync()
        {
            cblRoles.DataSource = await DB.Roles.AsNoTracking().ToListAsync();
            cblRoles.DataBind();

        }
        #endregion

        #region InitUserTitle

        private async Task InitUserTitleAsync()
        {

            cblTitles.DataSource = await DB.Titles.AsNoTracking().ToListAsync();
            cblTitles.DataBind();

        }
        #endregion
		
        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            var _user = await DB.Users.Where(u => u.Name == CurrentUser.Name).FirstOrDefaultAsync();
            if (_user != null)
            {
                Alert.Show("用户 " + CurrentUser.Name + " 已经存在！");
                return;
            }

            // 创建保存到数据库的密码
            CurrentUser.Password = PasswordUtil.CreateDbPassword(CurrentUser.Password.Trim());
            CurrentUser.CreateTime = DateTime.Now;


            // 添加部门
            if (!String.IsNullOrEmpty(ddbDept.Value))
            {
                CurrentUser.DeptID = Convert.ToInt32(ddbDept.Value);
            }

            // 添加角色
            if (ddbRoles.Values != null && ddbRoles.Values.Length > 0)
            {
                CurrentUser.Roles = new List<Role>();

                int[] roleIDs = ddbRoles.Values.Select(r => Convert.ToInt32(r)).ToArray();
                AddEntities<Role>(CurrentUser.Roles, roleIDs);
            }


            // 添加职称
            if (ddbTitles.Values != null && ddbTitles.Values.Length > 0)
            {
                CurrentUser.Titles = new List<Title>();

                int[] titleIDs = ddbTitles.Values.Select(r => Convert.ToInt32(r)).ToArray();
                AddEntities<Title>(CurrentUser.Titles, titleIDs);
            }


            DB.Users.Add(CurrentUser);
            await DB.SaveChangesAsync();

            // 新用户带了角色：他本人还没有会话，这里作废是为了与其它改动路径保持一致
            InvalidatePermissionCaches();


            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
