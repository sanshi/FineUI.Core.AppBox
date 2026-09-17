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
    [CheckPower(Name = "CoreDeptUserNew")]
    public partial class DeptUserNewModel : BaseAdminModel
    {
        #region Fields

        public List<User> Users { get; set; }

        #endregion


        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var deptID = GetQueryIntValue("deptID");

                var dept = await DB.Depts
                    .Where(d => d.ID == deptID).AsNoTracking().FirstOrDefaultAsync();

                if (dept == null)
                {
                    // 参数错误，首先弹出Alert对话框然后关闭弹出窗口
                    Alert.Show("参数错误！", String.Empty, ActiveWindow.GetHideReference());
                    return;
                }

                hfDeptID.Text = deptID.ToString();

                await LoadDataAsync();
            }
        }


        private async Task LoadDataAsync()
        {
            //var deptID = Convert.ToInt32(hfDeptID.Text);

            IQueryable<User> q = DB.Users;

            string searchText = ttbSearchMessage.Text?.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除所有已经拥有部门属性的用户（只包含部门为空的用户）
            q = q.Where(u => u.Dept == null);

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
            var deptID = Convert.ToInt32(hfDeptID.Text);

            // 选中的用户ID列表（跨页保持选中行）
            var selectedRowIDs = Grid1.SelectedRowIDArray.Select(u => Convert.ToInt32(u)).ToArray();
            if (selectedRowIDs.Length == 0)
            {
                Alert.Show("请至少选择一项！");
                return;
            }

            var users = await DB.Users
                 .Where(u => selectedRowIDs.Contains(u.ID))
                 .ToListAsync();

            users.ForEach(u => u.DeptID = deptID);

            await DB.SaveChangesAsync();

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
