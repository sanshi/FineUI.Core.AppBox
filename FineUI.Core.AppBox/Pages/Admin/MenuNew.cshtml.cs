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
    [CheckPower(Name = "CoreMenuNew")]
    public partial class MenuNewModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Menu Menu { get; set; }

        #endregion


        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MenuEdit_GetIconItems().ForEach(item => rblIconList.Items.Add(item));


                // 绑定下拉树表格
                await BindParentDDBAsync();
            }
        }

        public List<RadioItem> MenuEdit_GetIconItems()
        {
            List<RadioItem> items = new List<RadioItem>();

            string[] icons = new string[] { "tag_yellow", "tag_red", "tag_purple", "tag_pink", "tag_orange", "tag_green", "tag_blue" };
            foreach (string icon in icons)
            {
                string value = String.Format("~/res/icon/{0}.png", icon);
                RadioItem item = new RadioItem
                {
                    Value = value,
                    TextRawHtml = new RawHtml(String.Format("<img style=\"vertical-align:bottom;\" src=\"{0}\" />&nbsp;{1}", Url.Content(value), icon))
                };

                items.Add(item);
            }

            return items;
        }

        #endregion

        #region BindParentDDBAsync

        private async Task BindParentDDBAsync()
        {
            Grid1.DataSource = await DB.Menus.OrderBy(m => m.SortIndex).AsNoTracking().ToListAsync();
            Grid1.DataBind();
        }


        #endregion


        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            // http://fineui.com/docs/#/Questions/2000_modelstate
            // 暂时排除 ViewPowerID 属性的服务端验证，下面会单独处理 ViewPowerID 属性
            ModelState.Remove("Menu.ViewPowerID");

            if (!ModelState.IsValid)
            {
                return;
            }

            // 设置父菜单
            if (!String.IsNullOrEmpty(ddbParent.Value))
            {
                int parentID = Convert.ToInt32(ddbParent.Value);
                Menu.ParentID = parentID;
            }
            else
            {
                Menu.ParentID = null;
            }



            var viewPowerName = tbxViewPower.Text;
            if (String.IsNullOrEmpty(viewPowerName))
            {
                Menu.ViewPowerID = null;
            }
            else
            {
                var viewPower = await DB.Powers
                    .Where(p => p.Name == viewPowerName)
                    .FirstOrDefaultAsync();

                if (viewPower != null)
                {
                    Menu.ViewPowerID = viewPower.ID;
                }
                else
                {
                    Alert.Show("浏览权限 " + viewPowerName + " 不存在！");
                    return;
                }
            }

            DB.Menus.Add(Menu);
            await DB.SaveChangesAsync();


            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

        }


        #endregion


    }
}
