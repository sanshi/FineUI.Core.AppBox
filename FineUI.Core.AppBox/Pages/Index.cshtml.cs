
using FineUI.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox.Pages
{
    [Authorize]
    public partial class IndexModel : BaseModel
    {
        #region Fields



        #endregion

        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            // 用户可见的菜单列表
            List<Menu> menus = await ResolveUserMenuListAsync();
            if (menus.Count == 0)
            {
                ShowNotify("系统管理员尚未给你配置菜单！");
                return;
            }

            GetTreeNodes(menus).ForEach(n => treeMenu.Nodes.Add(n));

            btnUserName.Text = GetIdentityName();
            //ProductVersion = GetProductVersion();
            //ConfigTitle = String.Format("FineUI.Core.AppBox v{0}", GetProductVersion());

            btnSystemHelp.Menu = GetSystemHelpMenu();
        }

        #region GetSystemHelpMenu

        // 帮助菜单
        private FineUI.Core.Menu GetSystemHelpMenu()
        {
            FineUI.Core.Menu menu = new FineUI.Core.Menu();

            JArray ja = JArray.Parse(ConfigHelper.HelpList);
            foreach (JObject jo in ja)
            {
                string text = jo.Value<string>("Text");
                Icon icon = IconHelper.String2Icon(jo.Value<string>("Icon"), true);
                string id = jo.Value<string>("ID");
                string url = jo.Value<string>("URL");

                if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(url))
                {
                    FineUI.Core.MenuButton menuItem = new FineUI.Core.MenuButton();
                    menuItem.Text = text;
                    menuItem.Icon = icon;
                    // 选项卡参数作为数据下发，具名函数只负责读取并打开选项卡。
                    menuItem.Attributes["data-id"] = id;
                    menuItem.Attributes["data-url"] = Url.Content(url);
                    menuItem.Attributes["data-text"] = text;
                    menuItem.ClickHandler = "onHelpMenuClick";

                    menu.Items.Add(menuItem);
                }
            }

            return menu;
        }

        #endregion

        #region GetTreeNodes

        /// <summary>
        /// 创建树菜单
        /// </summary>
        /// <param name="menus"></param>
        /// <returns></returns>
        private List<TreeNode> GetTreeNodes(List<Menu> menus)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            // 生成树
            ResolveMenuTree(menus, null, nodes);

            // 展开第一个树节点
            nodes[0].Expanded = true;

            return nodes;
        }

        /// <summary>
        /// 生成菜单树
        /// </summary>
        /// <param name="menus"></param>
        /// <param name="parentMenuID"></param>
        /// <param name="nodes"></param>
        /// <returns>当前目录下有多少个子节点</returns>
        private int ResolveMenuTree(List<Menu> menus, int? parentMenuID, IList<TreeNode> nodes)
        {
            int count = 0;
            foreach (var menu in menus.Where(m => m.ParentID == parentMenuID))
            {
                TreeNode node = new TreeNode();
                nodes.Add(node);
                count++;

                node.Text = menu.Name;
                node.IconUrl = menu.ImageUrl;
                if (!String.IsNullOrEmpty(menu.NavigateUrl))
                {
                    node.NavigateUrl = Url.Content(menu.NavigateUrl);
                }

                int childCount = ResolveMenuTree(menus, menu.ID, node.Nodes);

                // 如果当前节点是子节点
                if (childCount == 0)
                {
                    node.Leaf = true;

                    // 但是此节点不是超链接，则删除
                    if (String.IsNullOrEmpty(menu.NavigateUrl))
                    {
                        nodes.Remove(node);
                        count--;
                    }
                }

            }

            return count;
        }

        #endregion

        #region ResolveUserMenuList

        // 获取用户可用的菜单列表
        private async Task<List<Menu>> ResolveUserMenuListAsync()
        {
            // 当前登陆用户的权限列表
            List<string> rolePowerNames = GetRolePowerNames();

            // 当前用户所属角色可用的菜单列表
            List<Menu> menus = new List<Menu>();

            List<Menu> allMenus = await DB.Menus.Include(m => m.ViewPower).OrderBy(m => m.SortIndex).ToListAsync();
            
            foreach (var menu in allMenus)
            {
                // 如果此菜单不属于任何模块，或者此用户所属角色拥有对此模块的权限
                if (menu.ViewPowerID == null || rolePowerNames.Contains(menu.ViewPower.Name))
                {
                    menus.Add(menu);
                }
            }

            return menus;
        }

        #endregion


        #endregion


        #region Events

        protected async Task btnSignOut_ClickAsync(object sender, EventArgs e)
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();

            Response.Redirect("/Login");
        }


        #endregion

    }
}
