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
    [CheckPower(Name = "CoreMenuEdit")]
    public partial class MenuEditModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Menu Menu { get; set; }

        #endregion

        #region OnGet

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu = await DB.Menus
                .Include(m => m.Parent)
                .Include(m => m.ViewPower)
                .Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Menu == null)
            {
                return Content("无效参数！");
            }

            return Page();
        }

        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                tbxViewPower.Text = Menu.ViewPowerID == null ? "" : Menu.ViewPowerName;

                MenuEdit_GetIconItems().ForEach(item => rblIconList.Items.Add(item));

                // 初始化图标列表的选中值
                if (!String.IsNullOrEmpty(Menu.ImageUrl))
                {
                    rblIconList.SelectedValue = Menu.ImageUrl;
                }


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

            if (Menu.Parent != null)
            {
                // 当前节点的父节点
                ddbParent.Value = Menu.ParentID.ToString();
                ddbParent.Text = Menu.Parent.Name;
            }
        }


        #endregion

        #region Grid1_RowDataBound

        protected void Grid1_RowDataBound(object sender, GridRowDataBoundEventArgs e)
        {
            var menuID = Convert.ToInt32(e.RowID);

            // 如果此部门是当前部门（Dept）或者当前部门的子项，则禁止选择
            if (IsOrChildOfCurrentModel(menuID))
            {
                e.RowSelectable = false;
            }
            else
            {
                e.RowSelectable = true;
            }
        }



        private bool IsOrChildOfCurrentModel(int menuID)
        {
            if (menuID == Menu.ID)
            {
                return true;
            }

            var menu = FindModelFromDataSource(menuID);
            if (menu.ParentID != null)
            {
                return IsOrChildOfCurrentModel(menu.ParentID.Value);
            }

            return false;
        }


        // 从表格的数据源中查找指定ID的部门对象
        private Menu FindModelFromDataSource(int menuID)
        {
            Menu result = null;
            var dataSource = Grid1.DataSource as List<Menu>;
            if (dataSource != null && dataSource.Count > 0)
            {
                result = dataSource.FirstOrDefault<Menu>(d => d.ID == menuID);
            }
            return result;
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


            // 从页面文本输入框中取出用户修改的值，并更新模型属性 Menu.ViewPowerID
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

            

            DB.Entry(Menu).State = EntityState.Modified;
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

        }


        #endregion


    }
}
