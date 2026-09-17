using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FineUI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreRoleUserView")]
    public partial class RoleUserModel : BaseAdminModel
    {
        #region Fields

        public List<Role> Roles { get; set; }
        public List<User> Users { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreRoleUserNew = CheckPower("CoreRoleUserNew");
                var powerCoreRoleUserDelete = CheckPower("CoreRoleUserDelete");

                // 根据用户权限控制页面控件的可用状态
                btnNew.Enabled = powerCoreRoleUserNew;
                btnDeleteSelected.Enabled = powerCoreRoleUserDelete;
				
				// 行内删除按钮的权限
                Grid2.FindCommand("Delete").Enabled = powerCoreRoleUserDelete;

                
                // 初始化左侧表格（返回左侧表格数据）
                var roles = await LoadGrid1DataAsync();
                if (roles.Count == 0)
                {
                    // 没有数据
                    Alert.Show("请先添加角色！");
                    return;
                }

                // 初始化左侧表格默认选中行
                Grid1.SelectedRowID = roles.First().ID.ToString();


                // 初始化右侧表格
                ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();
                Grid2.PageSize = ConfigHelper.PageSize;

                await LoadGrid2DataAsync();
            }
        }

        private async Task<List<Role>> LoadGrid1DataAsync()
        {
            var q = DB.Roles;
            var roles = await q.SortAsync(Grid1);

            Grid1.DataSource = roles;
            Grid1.DataBind();

            return roles;
        }

        private async Task LoadGrid2DataAsync()
        {
            // 左侧表格选中的行
            if (String.IsNullOrEmpty(Grid1.SelectedRowID))
            {
                Grid2.DataSource = null;
                Grid2.DataBind();

                return;
            }

            var roleID = Convert.ToInt32(Grid1.SelectedRowID);

            IQueryable<User> q = DB.Users;

            string searchText = ttbSearchMessage.Text?.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 过滤选中角色下的所有用户
            q = q.Where(u => u.Roles.Any(r => r.ID == roleID));


            // 获取总记录数（在添加条件之后，排序和分页之前）
            Grid2.RecordCount = await q.CountAsync();

            // 排列和数据库分页
            var users = await q.SortAndPageAsync(Grid2);

            // 绑定表格数据
            Grid2.DataSource = users;
            Grid2.DataBind();
        }

        #endregion

        #region Grid1 Events
        protected async Task Grid1_SortAsync(object sender, GridSortEventArgs e)
        {
            // 左侧表格排序后，选中项清空，需要重新选中第一行
            var roles = await LoadGrid1DataAsync();

            // 默认选中第一个行
            Grid1.SelectedRowID = roles.First().ID.ToString();

            // 重新加载右侧表格数据
            await LoadGrid2DataAsync();
        }

        protected async Task Grid1_RowClickAsync(object sender, GridRowEventArgs e)
        {
            await LoadGrid2DataAsync();
        }

        #endregion

        #region Grid2 Events

        protected async Task Window1_CloseAsync(object sender, WindowCloseEventArgs e)
        {
            await LoadGrid2DataAsync();
        }

        protected async Task Grid2_SortAsync(object sender, GridSortEventArgs e)
        {
            await LoadGrid2DataAsync();
        }

        protected async Task Grid2_PageIndexChangedAsync(object sender, GridPageEventArgs e)
        {
            await LoadGrid2DataAsync();
        }

        protected async Task ddlGridPageSize_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            // 设置每页显示的项数
            Grid2.PageSize = Convert.ToInt32(ddlGridPageSize.SelectedValue);

            await LoadGrid2DataAsync();
        }

        protected async Task ttbSearchMessage_Trigger1ClickAsync(object sender, EventArgs e)
        {
            // 清空输入框的内容，并隐藏清空图标
            ttbSearchMessage.Text = String.Empty;
            ttbSearchMessage.ShowTrigger1 = false;

            await LoadGrid2DataAsync();
        }

        protected async Task ttbSearchMessage_Trigger2ClickAsync(object sender, EventArgs e)
        {
            // 显示清空图标
            ttbSearchMessage.ShowTrigger1 = true;

            await LoadGrid2DataAsync();
        }


        #endregion

        #region Page_CustomEvent

        protected async Task Page_CustomEventAsync(object sender, CustomEventArgs e)
        {
            var eventName = e.EventName;
            var eventArguments = e.EventArgumentsAsJObject;

            if (eventName == "Grid2_DeleteRows")
            {
                // 删除某些行
                var rowIDs = eventArguments.Value<JArray>("rowIDs").ToObject<int[]>();

                await DeleteRowsAsync(rowIDs);
            }

        }

        private async Task DeleteRowsAsync(int[] rowIDs)
        {
			// 左侧表格选中的行
			if (String.IsNullOrEmpty(Grid1.SelectedRowID))
			{
				return;
			}
			var roleID = Convert.ToInt32(Grid1.SelectedRowID);


			// 在操作之前进行权限检查
			if (!CheckPower("CoreRoleUserDelete"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            Role role = DB.Roles
                .Include(r => r.Users)
                .Where(r => r.ID == roleID)
                .FirstOrDefault();

            role.Users.Where(u => rowIDs.Contains(u.ID)).ToList().ForEach(u => role.Users.Remove(u));

            await DB.SaveChangesAsync();

            // 用户与角色的关联变了，其在线会话的权限缓存作废
            InvalidatePermissionCaches();


            await LoadGrid2DataAsync();
        }

        #endregion

        #region Grid2_RowCommand

        protected async Task Grid2_RowCommandAsync(object sender, GridRowCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                // 删除行
                var rowID = Convert.ToInt32(e.RowID);

                await DeleteRowsAsync(new int[] { rowID });
            }
        }

        #endregion
    }
}