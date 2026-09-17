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
    [CheckPower(Name = "CoreDeptNew")]
    public partial class DeptNewModel : BaseAdminModel
    {
        #region Fields

        [BindProperty]
        public Dept Dept { get; set; }

        #endregion

        
        #region Page_Load

        protected async Task Page_LoadAsync(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 绑定下拉树表格
                await BindParentDDBAsync();
            }
        }


        private async Task BindParentDDBAsync()
        {
            Grid1.DataSource = await DB.Depts.OrderBy(d => d.SortIndex).AsNoTracking().ToListAsync();
            Grid1.DataBind();
        }



        #endregion


        #region Events

        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            // 设置父部门
            if (!String.IsNullOrEmpty(ddbParent.Value))
            {
                int parentID = Convert.ToInt32(ddbParent.Value);
                Dept.ParentID = parentID;
            }
            else
            {
                Dept.ParentID = null;
            }


            DB.Depts.Add(Dept);
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion


    }
}
