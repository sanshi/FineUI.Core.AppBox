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
    public partial class MenuEditModel : AdminPageBase
    {
        #region Fields

        [BindProperty]
        public Menu Menu { get; set; }

        private Menu _storedMenu;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            var id = GetQueryIntValue("id");
            _storedMenu = await DB.Menus
                .Include(m => m.Parent)
                .Include(m => m.ViewPower)
                .Where(m => m.ID == id).FirstOrDefaultAsync();

            if (_storedMenu == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                Menu = _storedMenu;
            }
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
            if (menuID == _storedMenu.ID)
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

            // 页面禁选只改善操作体验；保存时还要核对父菜单，防止手工构造请求造成循环。
            int? parentId = null;
            if (!String.IsNullOrEmpty(ddbParent.Value))
            {
                if (!Int32.TryParse(ddbParent.Value, out var selectedParentId))
                {
                    Alert.Show("无效的上级菜单！");
                    return;
                }
                parentId = selectedParentId;
            }

            var visited = new HashSet<int>();
            int? ancestorId = parentId;
            while (ancestorId.HasValue)
            {
                if (ancestorId == _storedMenu.ID || !visited.Add(ancestorId.Value))
                {
                    Alert.Show("不能把当前菜单设为自己的下级！");
                    return;
                }
                var ancestor = await DB.Menus
                    .Where(menu => menu.ID == ancestorId.Value)
                    .Select(menu => new { menu.ParentID })
                    .FirstOrDefaultAsync();
                if (ancestor == null)
                {
                    Alert.Show("上级菜单不存在！");
                    return;
                }
                ancestorId = ancestor.ParentID;
            }


            // 浏览权限按当前输入解析，写入服务器查询到的菜单实体。
            var viewPowerName = tbxViewPower.Text;
            if (String.IsNullOrEmpty(viewPowerName))
            {
                _storedMenu.ViewPowerID = null;
            }
            else
            {
                var viewPower = await DB.Powers
                    .Where(p => p.Name == viewPowerName)
                    .FirstOrDefaultAsync();

                if (viewPower != null)
                {
                    _storedMenu.ViewPowerID = viewPower.ID;
                }
                else
                {
                    Alert.Show("浏览权限 " + viewPowerName + " 不存在！");
                    return;
                }
            }

            

            _storedMenu.Name = Menu.Name;
            _storedMenu.SortIndex = Menu.SortIndex;
            _storedMenu.NavigateUrl = Menu.NavigateUrl;
            _storedMenu.ImageUrl = Menu.ImageUrl;
            _storedMenu.Remark = Menu.Remark;
            _storedMenu.ParentID = parentId;
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();

        }


        #endregion


    }
}
