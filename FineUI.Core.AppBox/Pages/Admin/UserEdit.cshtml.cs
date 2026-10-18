using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;


using FineUI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreUserEdit")]
    public partial class UserEditModel : AdminPageBase
    {
        #region Fields
        
        [BindProperty]
        public User CurrentUser { get; set; }

        private User _storedUser;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            _storedUser = await DB.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .FirstOrDefaultAsync(user => user.ID == id);
            if (_storedUser == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (_storedUser.Name == "admin" && GetIdentityName() != "admin")
            {
                FineUI.Core.PageContext.AbortPage("你无权编辑超级管理员！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                CurrentUser = _storedUser;
            }
        }


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
			// 用户所属部门
            if (CurrentUser.Dept != null)
            {
                ddbDept.Value = CurrentUser.DeptID.ToString();
                ddbDept.Text = CurrentUser.DeptName;
            }

            gridDept.DataSource = await DB.Depts.OrderBy(d => d.SortIndex).AsNoTracking().ToListAsync();
            gridDept.DataBind();
        }

        #endregion

        #region InitUserRole

        private async Task InitUserRoleAsync()
        {
			// 用户所属角色
            if (CurrentUser.Roles.Count > 0)
            {
                ddbRoles.Values = CurrentUser.Roles.Select(r => r.ID.ToString()).ToArray();
                ddbRoles.Texts = CurrentUser.Roles.Select(r => r.Name).ToArray();
            }

            cblRoles.DataSource = await DB.Roles.AsNoTracking().ToListAsync();
            cblRoles.DataBind();

        }
        #endregion

        #region InitUserTitle

        private async Task InitUserTitleAsync()
        {
			// 用户拥有职称
            if (CurrentUser.Titles.Count > 0)
            {
                ddbTitles.Values = CurrentUser.Titles.Select(u => u.ID.ToString()).ToArray();
                ddbTitles.Texts = CurrentUser.Titles.Select(u => u.Name).ToArray();
            }

            cblTitles.DataSource = await DB.Titles.AsNoTracking().ToListAsync();
            cblTitles.DataBind();

        }
        #endregion

        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            // 不对 Name 和 Password 进行模型验证
            ModelState.Remove("CurrentUser.Name");
            ModelState.Remove("CurrentUser.Password");

            if (!ModelState.IsValid)
            {
                return;
            }

            // 更新 Page_Init 已按当前 URL 查到并校验的实体；表单字段不决定保存目标。
            _storedUser.ChineseName = CurrentUser.ChineseName;
            _storedUser.Gender = CurrentUser.Gender;
            _storedUser.Enabled = CurrentUser.Enabled;
            _storedUser.Email = CurrentUser.Email;
            _storedUser.CompanyEmail = CurrentUser.CompanyEmail;
            _storedUser.OfficePhone = CurrentUser.OfficePhone;
            _storedUser.OfficePhoneExt = CurrentUser.OfficePhoneExt;
            _storedUser.HomePhone = CurrentUser.HomePhone;
            _storedUser.CellPhone = CurrentUser.CellPhone;
            _storedUser.Remark = CurrentUser.Remark;

            // 更新用户所属的角色
            int[] roleIDs = ddbRoles.Values.Select(r => Convert.ToInt32(r)).ToArray();
            ReplaceEntities<Role>(_storedUser.Roles, roleIDs);

            // 更新用户拥有的职称
            int[] titleIDs = ddbTitles.Values.Select(r => Convert.ToInt32(r)).ToArray();
            ReplaceEntities<Title>(_storedUser.Titles, titleIDs);

            // 如果选择了部门，则更新部门ID，否则设置为null
            if (!String.IsNullOrEmpty(ddbDept.Value))
            {
                _storedUser.DeptID = Convert.ToInt32(ddbDept.Value);
            }
            else
            {
                _storedUser.DeptID = null;
            }

            await DB.SaveChangesAsync();

            // 用户的角色集合变了，其在线会话的权限缓存作废
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

        }

        #endregion

    }
}
