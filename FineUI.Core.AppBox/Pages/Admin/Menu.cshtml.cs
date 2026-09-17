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
    [CheckPower(Name = "CoreMenuView")]
    public partial class MenuModel : BaseAdminModel
    {
        #region Fields

        public List<Menu> Menus { get; set; }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreMenuNew = CheckPower("CoreMenuNew");
                var powerCoreMenuEdit = CheckPower("CoreMenuEdit");
                var powerCoreMenuDelete = CheckPower("CoreMenuDelete");

                // 根据用户权限控制页面控件的可用状态
                btnNew.Enabled = powerCoreMenuNew;

                // 行内编辑按钮的权限
                Grid1.FindCommand("Edit").Enabled = powerCoreMenuEdit;
                // 行内删除按钮的权限
                Grid1.FindCommand("Delete").Enabled = powerCoreMenuDelete;


                // 初始化页面控件属性


                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            Grid1.DataSource = await DB.Menus.OrderBy(m => m.SortIndex).AsNoTracking().ToListAsync();
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
                if (!CheckPower("CoreMenuDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                int childCount = await DB.Menus.Where(m => m.Parent.ID == rowID).CountAsync();
                if (childCount > 0)
                {
                    Alert.ShowInTop("删除失败！请先删除子菜单！");
                    return;
                }


                var menu = await DB.Menus.Where(m => m.ID == rowID).FirstOrDefaultAsync();
                DB.Menus.Remove(menu);

                await DB.SaveChangesAsync();


                await LoadDataAsync();
            }
        }

        #endregion


    }
}
