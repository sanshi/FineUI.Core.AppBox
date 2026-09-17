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
    [CheckPower(Name = "CoreRoleUserNew")]
    public partial class RoleUserNewModel : BaseAdminModel
    {
        #region Fields

        public List<User> Users { get; set; }

        #endregion


        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var roleID = GetQueryIntValue("roleID");

                var role = await DB.Roles
                    .Where(t => t.ID == roleID).AsNoTracking().FirstOrDefaultAsync();

                if (role == null)
                {
                    // 参数错误，首先弹出Alert对话框然后关闭弹出窗口
                    Alert.Show("参数错误！", String.Empty, ActiveWindow.GetHideReference());
                    return;
                }

                hfRoleID.Text = roleID.ToString();


                await LoadDataAsync();
            }
        }


        private async Task LoadDataAsync()
        {
            var roleID = Convert.ToInt32(hfRoleID.Text);

            IQueryable<User> q = DB.Users;

            string searchText = ttbSearchMessage.Text?.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除已经属于本角色的用户
            q = q.Where(u => u.Roles.All(r => r.ID != roleID));

            //// 排除已经拥有的用户（排除这些用户（用户有一个职称就是当前操作的职称））
            //q = q.Where(u => !u.Roles.Any(r => r.ID == roleID));

            // 获取总记录数（在添加条件之后，排序和分页之前）
            Grid1.RecordCount = await q.CountAsync();

            // 排列和数据库分页
            var users = await q.SortAndPageAsync(Grid1);

            // 绑定表格数据
            Grid1.DataSource = users;
            Grid1.DataBind();
        }

        #endregion


        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            var roleID = Convert.ToInt32(hfRoleID.Text);

            // 选中的用户ID列表（跨页保持选中行）
            var selectedRowIDs = Grid1.SelectedRowIDArray.Select(u => Convert.ToInt32(u)).ToArray();
            if (selectedRowIDs.Length == 0)
            {
                Alert.Show("请至少选择一项！");
                return;
            }

            Role role = DB.Roles.Include(r => r.Users)
                .Where(r => r.ID == roleID)
                .FirstOrDefault();

            AddEntities<User>(role.Users, selectedRowIDs);

            await DB.SaveChangesAsync();

            // 用户与角色的关联变了，其在线会话的权限缓存作废
            InvalidatePermissionCaches();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

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


    }
}
