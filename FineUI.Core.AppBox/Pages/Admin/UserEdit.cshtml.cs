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
    public partial class UserEditModel : BaseAdminModel
    {
        #region Fields
        
        [BindProperty]
        public User CurrentUser { get; set; }

        #endregion

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentUser = await DB.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .Where(m => m.ID == id).AsNoTracking().FirstOrDefaultAsync();
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

            // 更新部分字段（先从数据库检索用户，再覆盖用户输入值，注意没有更新Name，Password，CreateTime等字段）
            var _user = await DB.Users
                .Include(u => u.Dept)
                .Include(u => u.Roles)
                .Include(u => u.Titles)
                .Where(m => m.ID == CurrentUser.ID).FirstOrDefaultAsync();

            // 回发带回来的主键是页面上的隐藏字段，可以被篡改，所以要把 OnGet 里的那两道判断原样再做一遍：
            // 只在打开页面时判过，拥有编辑权限的普通用户改一下隐藏字段就能编辑超级管理员
            if (_user == null)
            {
                Alert.Show("无效参数！");
                return;
            }

            if (_user.Name == "admin" && GetIdentityName() != "admin")
            {
                Alert.Show("你无权编辑超级管理员！");
                return;
            }

            _user.ChineseName = CurrentUser.ChineseName;
            _user.Gender = CurrentUser.Gender;
            _user.Enabled = CurrentUser.Enabled;
            _user.Email = CurrentUser.Email;
            _user.CompanyEmail = CurrentUser.CompanyEmail;
            _user.OfficePhone = CurrentUser.OfficePhone;
            _user.OfficePhoneExt = CurrentUser.OfficePhoneExt;
            _user.HomePhone = CurrentUser.HomePhone;
            _user.CellPhone = CurrentUser.CellPhone;
            _user.Remark = CurrentUser.Remark;

            // 更新用户所属的角色
            int[] roleIDs = ddbRoles.Values.Select(r => Convert.ToInt32(r)).ToArray();
            ReplaceEntities<Role>(_user.Roles, roleIDs);

            // 更新用户拥有的职称
            int[] titleIDs = ddbTitles.Values.Select(r => Convert.ToInt32(r)).ToArray();
            ReplaceEntities<Title>(_user.Titles, titleIDs);

            // 如果选择了部门，则更新部门ID，否则设置为null
            if (!String.IsNullOrEmpty(ddbDept.Value))
            {
                _user.DeptID = Convert.ToInt32(ddbDept.Value);
            }
            else
            {
                _user.DeptID = null;
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
