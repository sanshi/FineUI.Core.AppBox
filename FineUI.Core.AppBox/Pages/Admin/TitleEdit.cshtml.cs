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
    [CheckPower(Name = "CoreTitleEdit")]
    public partial class TitleEditModel : AdminPageBase
    {
        #region Fields

        [BindProperty]
        public Title Title { get; set; }

        private Title _storedTitle;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            _storedTitle = await DB.Titles
                .FirstOrDefaultAsync(item => item.ID == id);

            if (_storedTitle == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                Title = _storedTitle;
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
            }
        }

        #endregion


        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            // 更新 Page_Init 已按当前 URL 校验的实体，表单中的主键不决定保存目标。
            _storedTitle.Name = Title.Name;
            _storedTitle.Remark = Title.Remark;
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
