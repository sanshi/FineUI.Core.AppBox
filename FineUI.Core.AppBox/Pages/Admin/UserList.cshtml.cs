


using FineUI.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreUserView")]
    public partial class UserListModel : BaseAdminModel
    {
        #region Fields

        public List<User> Users { get; set; }

        #endregion

        #region Page_Load
        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreUserNew = CheckPower("CoreUserNew");
                var powerCoreUserEdit = CheckPower("CoreUserEdit");
                var powerCoreUserDelete = CheckPower("CoreUserDelete");
                var powerCoreUserChangePassword = CheckPower("CoreUserChangePassword");

                // 根据用户权限控制页面控件的可用状态
                btnNew.Enabled = powerCoreUserNew;
                btnChangeEnableUsers.Enabled = powerCoreUserEdit;
                btnDeletedSelected.Enabled = powerCoreUserDelete;

                // 行内编辑按钮的权限
                Grid1.FindCommand("Edit").Enabled = powerCoreUserEdit;
                // 行内删除按钮的权限
                Grid1.FindCommand("Delete").Enabled = powerCoreUserDelete;
                // 行内删除按钮的权限
                Grid1.FindCommand("ChangePassword").Enabled = powerCoreUserChangePassword;



                // 初始化页面控件属性
                ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();
                Grid1.PageSize = ConfigHelper.PageSize;

                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            IQueryable<User> q = DB.Users;

            string searchText = ttbSearchMessage.Text.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            if (GetIdentityName() != "admin")
            {
                q = q.Where(u => u.Name != "admin");
            }

            // 过滤启用状态
            string enableStatus = rblEnableStatus.SelectedValue;
            if (enableStatus != "all")
            {
                q = q.Where(u => u.Enabled == (enableStatus == "enabled" ? true : false));
            }


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


        protected async Task rblEnableStatus_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        #endregion

        #region Page_CustomEvent

        protected async Task Page_CustomEventAsync(object sender, CustomEventArgs e)
        {
            var eventName = e.EventName;
            var eventArguments = e.EventArgumentsAsJObject;

            if (eventName == "Grid1_DeleteRows")
            {
                // 删除某些行
                var rowIDs = eventArguments.Value<JArray>("rowIDs").ToObject<int[]>();

                await DeleteRowsAsync(rowIDs);
            }
            else if (eventName == "Grid1_EnableRows")
            {
                // 禁用启用某些行
                var rowIDs = eventArguments.Value<JArray>("rowIDs").ToObject<int[]>();
                var enabled = eventArguments.Value<string>("action") == "enable" ? true : false;

                if (!CheckPower("CoreUserEdit"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                // 列表里看不到 admin，但回传的主键不可信：超级管理员不能被禁用
                if (await DB.Users.AnyAsync(u => rowIDs.Contains(u.ID) && u.Name == "admin"))
                {
                    Alert.ShowInTop("不能修改超级管理员（admin）的启用状态！");
                    return;
                }

                DB.Users.Where(u => rowIDs.Contains(u.ID)).ToList().ForEach(u => u.Enabled = enabled);
                await DB.SaveChangesAsync();

                await LoadDataAsync();
            }
        }

        private async Task DeleteRowsAsync(int[] rowIDs)
        {
            if (!CheckPower("CoreUserDelete"))
            {
                CheckPowerFailWithAlert();
                return;
            }
            

            var usersToDelete = await DB.Users.Where(u => rowIDs.Contains(u.ID)).ToListAsync();

            if (usersToDelete.Any(u => u.Name == "admin"))
            {
                Alert.ShowInTop("不能删除超级管理员（admin）！");
                return;
            }


            usersToDelete.ForEach(u => DB.Users.Remove(u));
            await DB.SaveChangesAsync();

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

                await DeleteRowsAsync(new int[] { rowID });
            }
        }

        #endregion


    }
}
