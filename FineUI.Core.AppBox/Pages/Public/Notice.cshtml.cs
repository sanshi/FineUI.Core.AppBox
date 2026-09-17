using System;
using System.Collections.Generic;
using System.Linq;

using FineUI.Core;

namespace FineUI.Core.AppBox.Pages.Public
{
    /// <summary>
    /// 通知公告：无需登录即可访问的公开页示例。
    ///
    /// 能匿名访问，靠的是本类**没有** [Authorize]：本项目里「要不要登录」由这个特性决定，
    /// 后台功能页统一继承带 [Authorize] 的 BaseAdminModel，所以直接继承 BaseModel 的页面天然是公开的。
    ///
    /// 两件事因此成立：
    ///   1. 本类不标 [CheckPower]——那个特性管的是「登录用户有没有这一页的权限」，与「要不要登录」无关；
    ///   2. 本类继承 BaseModel 而不是 BaseAdminModel，公开页也就不带当前用户水印。
    ///
    /// 页面里的搜索与翻页都是回发，能正常工作说明匿名会话下的回发通道是通的。
    ///
    /// 读当前用户必须判空：匿名访问时没有身份。本页据此在搜索框右侧显示不同的提示文字。
    /// </summary>
    public partial class NoticeModel : BaseModel
    {
        #region Fields

        public List<Notice> Notices { get; set; }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 匿名访问时没有身份，取任何用户信息之前都要先判断是否已登录
                labSignedIn.Text = User.Identity.IsAuthenticated
                    ? String.Format("您已登录为 {0}", GetIdentityName())
                    : "您当前未登录，可直接浏览公告";

                LoadData();
            }
        }

        private void LoadData()
        {
            string searchText = ttbSearchTitle.Text?.Trim();
            var filtered = NoticeData.GetAll();
            if (!String.IsNullOrEmpty(searchText))
            {
                filtered = filtered
                    .Where(x => x.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            // 数据在内存里，分页自己算（数据库分页的页面走 QueryableBuilder，这里手工 Skip/Take）
            Grid1.RecordCount = filtered.Count;
            Notices = filtered.Skip(Grid1.PageIndex * Grid1.PageSize).Take(Grid1.PageSize).ToList();

            Grid1.DataSource = Notices;
            Grid1.DataBind();
        }

        #endregion


        #region Events

        protected void Grid1_PageIndexChanged(object sender, GridPageEventArgs e)
        {
            LoadData();
        }

        protected void ttbSearchTitle_Trigger1Click(object sender, EventArgs e)
        {
            ttbSearchTitle.Text = String.Empty;
            ttbSearchTitle.ShowTrigger1 = false;
            Grid1.PageIndex = 0;
            LoadData();
        }

        protected void ttbSearchTitle_Trigger2Click(object sender, EventArgs e)
        {
            ttbSearchTitle.ShowTrigger1 = true;
            Grid1.PageIndex = 0;
            LoadData();
        }

        #endregion
    }
}
