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
    [CheckPower(Name = "CoreOnlineView")]
    public partial class OnlineModel : BaseAdminModel
    {
        #region Fields

        public List<Online> Onlines { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 初始化页面控件属性
                ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();
                Grid1.PageSize = ConfigHelper.PageSize;

                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            IQueryable<Online> q = DB.Onlines.Include(o => o.User);

            string searchText = ttbSearchMessage.Text?.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.User.Name.Contains(searchText));
            }

            // 2个小时内活跃的用户
            DateTime lastD = DateTime.Now.AddHours(-2);
            q = q.Where(o => o.UpdateTime > lastD);

            // 获取总记录数（在添加条件之后，排序和分页之前）
            Grid1.RecordCount = await q.CountAsync();

            // 排列和数据库分页
            var onlines = await q.SortAndPageAsync(Grid1);

            // 绑定表格数据
            Grid1.DataSource = onlines;
            Grid1.DataBind();
        }

        #endregion


        #region Events

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