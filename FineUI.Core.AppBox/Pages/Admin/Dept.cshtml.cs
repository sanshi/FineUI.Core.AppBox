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
    [CheckPower(Name = "CoreDeptView")]
    public partial class DeptModel : BaseAdminModel
    {
        #region Fields

        public List<Dept> Depts { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreDeptNew = CheckPower("CoreDeptNew");
                var powerCoreDeptEdit = CheckPower("CoreDeptEdit");
                var powerCoreDeptDelete = CheckPower("CoreDeptDelete");

                // 根据用户权限控制页面控件的可用状态
                btnNew.Enabled = powerCoreDeptNew;

                // 行内编辑按钮的权限
                Grid1.FindCommand("Edit").Enabled = powerCoreDeptEdit;
                // 行内删除按钮的权限
                Grid1.FindCommand("Delete").Enabled = powerCoreDeptDelete;

                // 初始化页面控件属性


                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            Grid1.DataSource = await DB.Depts.OrderBy(d => d.SortIndex).AsNoTracking().ToListAsync();
            Grid1.DataBind();
        }

        #endregion


        #region Events

        protected async Task Window1_CloseAsync(object sender, WindowCloseEventArgs e)
        {
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
                if (!CheckPower("CoreDeptDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                int userCount = await DB.Users.Where(u => u.DeptID == rowID).CountAsync();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此部门的用户！");
                    return;
                }

                int childCount = await DB.Depts.Where(d => d.Parent.ID == rowID).CountAsync();
                if (childCount > 0)
                {
                    Alert.ShowInTop("删除失败！请先删除子部门！");
                    return;
                }

                var dept = await DB.Depts.Where(d => d.ID == rowID).FirstOrDefaultAsync();
                DB.Depts.Remove(dept);
                await DB.SaveChangesAsync();


                await LoadDataAsync();
				
            }
        }

        #endregion

    }
}
