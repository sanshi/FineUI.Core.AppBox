using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


using FineUI.Core;
using Newtonsoft.Json.Linq;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreConfigView")]
    public partial class ConfigModel : BaseAdminModel
    {
        #region Fields



        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var powerCoreConfigEdit = CheckPower("CoreConfigEdit");

                // 根据用户权限控制页面控件的可用状态
                btnSave.Enabled = powerCoreConfigEdit;

                // 初始化页面控件属性
                await Task.Run(() =>
                {
                    JSBeautifyLib.JSBeautify jsb = new JSBeautifyLib.JSBeautify(ConfigHelper.HelpList, new JSBeautifyLib.JSBeautifyOptions());
                    tbxHelpList.Text = jsb.GetResult();
                });

            }
        }

        #endregion


        #region Events

        protected async Task btnSave_ClickAsync(object sender, EventArgs e)
        {
            // 在操作之前进行权限检查
            if (!CheckPower("CoreConfigEdit"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            try
            {
                JArray.Parse(tbxHelpList.Text.Trim());
            }
            catch (Exception)
            {
                tbxHelpList.MarkInvalid("格式不正确，必须是JSON字符串！");
                return;
            }


            ConfigHelper.PageSize = Convert.ToInt32(ddlPageSize.SelectedValue);
            ConfigHelper.HelpList = tbxHelpList.Text.Trim();
            await ConfigHelper.SaveAllAsync();

            RegisterStartupScript("top.window.location.reload(false);");
        }

        #endregion

    }
}