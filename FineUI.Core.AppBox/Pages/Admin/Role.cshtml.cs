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
    [CheckPower(Name = "CoreRoleView")]
    public partial class RoleModel : BaseAdminModel
    {
        #region Fields

        public List<Role> Roles { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreRoleNew = CheckPower("CoreRoleNew");
                var powerCoreRoleEdit = CheckPower("CoreRoleEdit");
                var powerCoreRoleDelete = CheckPower("CoreRoleDelete");

                // 根据用户权限控制页面控件的可用状态
                btnNew.Enabled = powerCoreRoleNew;

                // 行内编辑按钮的权限
                Grid1.FindCommand("Edit").Enabled = powerCoreRoleEdit;
                // 行内删除按钮的权限
                Grid1.FindCommand("Delete").Enabled = powerCoreRoleDelete;


                // 初始化页面控件属性
                ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();
                Grid1.PageSize = ConfigHelper.PageSize;

                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            IQueryable<Role> q = DB.Roles;

            string searchText = ttbSearchMessage.Text?.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(p => p.Name.Contains(searchText));
            }

            // 获取总记录数（在添加条件之后，排序和分页之前）
            Grid1.RecordCount = await q.CountAsync();

            // 排列和数据库分页
            var roles = await q.SortAndPageAsync(Grid1);

            // 绑定表格数据
            Grid1.DataSource = roles;
            Grid1.DataBind();
        }

        #endregion


        #region Events

        protected async Task Window1_CloseAsync(object sender, WindowCloseEventArgs e)
        {
            await LoadDataAsync();
        }

        protected async Task Grid1_SortAsync(object sender, GridSortEventArgs e)
        {
            await LoadDataAsync();
        }

        protected async Task Grid1_PageIndexChangedAsync(object sender, GridPageEventArgs e)
        {
            await LoadDataAsync();
        }

        protected async Task ddlGridPageSize_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            // 设置每页显示的项数
            Grid1.PageSize = Convert.ToInt32(ddlGridPageSize.SelectedValue);

            await LoadDataAsync();
        }

        protected async Task ttbSearchMessage_Trigger1ClickAsync(object sender, EventArgs e)
        {
            // 清空输入框的内容，并隐藏清空图标
            ttbSearchMessage.Text = String.Empty;
            ttbSearchMessage.ShowTrigger1 = false;

            await LoadDataAsync();
        }

        protected async Task ttbSearchMessage_Trigger2ClickAsync(object sender, EventArgs e)
        {
            // 显示清空图标
            ttbSearchMessage.ShowTrigger1 = true;

            await LoadDataAsync();
        }


        #endregion


        #region Grid1_RowCommand

        protected async Task Grid1_RowCommandAsync(object sender, GridRowCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                // 删除行
                var rowID = Convert.ToInt32(e.RowID);

                // 在操作之前进行权限检查
                if (!CheckPower("CoreRoleDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                int userCount = await DB.Users.Where(u => u.Roles.Any(r => r.ID == rowID)).CountAsync();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此角色的用户！");
                    return;
                }

                // 执行数据库操作
                var Role = await DB.Roles.Where(m => m.ID == rowID).FirstOrDefaultAsync();
                DB.Roles.Remove(Role);

                await DB.SaveChangesAsync();

                // 角色没了，其成员的权限视图变了
                InvalidatePermissionCaches();

                await LoadDataAsync();
            }
        }

        #endregion

    }
}
