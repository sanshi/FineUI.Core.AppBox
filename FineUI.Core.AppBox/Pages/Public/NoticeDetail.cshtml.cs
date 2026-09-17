using System;

using FineUI.Core;
using Microsoft.AspNetCore.Mvc;

namespace FineUI.Core.AppBox.Pages.Public
{
    /// <summary>
    /// 公告详情（弹窗内打开）：同样无需登录，因为它也没有 [Authorize]。
    ///
    /// 它既能被列表页的弹窗打开，也能把地址直接发给别人打开（例如 /Public/NoticeDetail?id=8）——
    /// 这正是「公告链接可以往外发」想要的效果。
    ///
    /// 参数取值仍要当作不可信输入：查不到记录就直接输出提示、不渲染页面。
    /// </summary>
    public partial class NoticeDetailModel : BaseModel
    {
        #region Fields

        public Notice Notice { get; set; }

        #endregion

        #region OnGet

        public IActionResult OnGet(int id)
        {
            Notice = NoticeData.Find(id);

            if (Notice == null)
            {
                return Content("公告不存在或已撤回！");
            }

            return Page();
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                labTitle.Text = Notice.Title;
                labDepartment.Text = Notice.Department;
                labPublishTime.Text = Notice.PublishTime.ToString("yyyy-MM-dd HH:mm");
                labContent.Text = Notice.Content;
            }
        }

        #endregion
    }
}
