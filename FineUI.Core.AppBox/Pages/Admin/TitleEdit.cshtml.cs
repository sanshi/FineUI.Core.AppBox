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
    public partial class TitleEditModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Title Title { get; set; }

        #endregion

        #region Page_Get

        public async Task Page_GetAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            Title = await DB.Titles
                .Where(m => m.ID == id).AsNoTracking().FirstOrDefaultAsync();

            if (Title == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
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

            DB.Entry(Title).State = EntityState.Modified;
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion

    }
}
