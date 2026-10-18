using FineUI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox.Pages.Admin
{
    [CheckPower(Name = "CoreDeptEdit")]
    public partial class DeptEditModel : AdminPageBase
    {
        #region Fields

        [BindProperty]
        public Dept Dept { get; set; }

        private Dept _storedDept;

        #endregion

        #region Page_Init

        public async Task Page_InitAsync(object sender, EventArgs e)
        {
            int.TryParse(Request.Query["id"], out var id);
            _storedDept = await DB.Depts
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (_storedDept == null)
            {
                FineUI.Core.PageContext.AbortPage("无效参数！", ActiveWindow.HideCommand());
                return;
            }

            if (!IsPostBack)
            {
                Dept = _storedDept;
            }
        }

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
            // EF Core的`Include`方法默认不会递归加载嵌套的导航属性。
            // 当你使用`.Include(d => d.Parent)`时，它只会加载直接父部门（即第一层父部门），而不会继续加载父部门的父部门（即第二层及以上）。
            // 如果你知道最大的嵌套层级，你可以使用多个`ThenInclude`来逐层包含，这种方法要求你预先知道最大嵌套层数
            //  .Include(d => d.Parent)
            //  .ThenInclude(p => p.Parent) // 第二层
            //  .ThenInclude(pp => pp.Parent) // 第三层
            Grid1.DataSource = await DB.Depts.OrderBy(d => d.SortIndex).AsNoTracking().ToListAsync();
            Grid1.DataBind();

            if (Dept.Parent != null)
            {
                // 当前节点的父节点
                ddbParent.Value = Dept.ParentID.ToString();
                ddbParent.Text = Dept.Parent.Name;
            }
        }


        #endregion

        #region Grid1_RowDataBound

        protected void Grid1_RowDataBound(object sender, GridRowDataBoundEventArgs e)
        {
            var deptID = Convert.ToInt32(e.RowID);

            // 如果此部门是当前部门（Dept）或者当前部门的子项，则禁止选择
            if (IsOrChildOfCurrentModel(deptID))
            {
                e.RowSelectable = false;
            }
            else
            {
                e.RowSelectable = true;
            }
        }

        private bool IsOrChildOfCurrentModel(int deptID)
        {
            if (deptID == _storedDept.ID)
            {
                return true;
            }

            var dept = FindModelFromDataSource(deptID);
            if (dept.ParentID != null)
            {
                return IsOrChildOfCurrentModel(dept.ParentID.Value);
            }

            return false;
        }


        // 从表格的数据源中查找指定ID的部门对象
        private Dept FindModelFromDataSource(int deptID)
        {
            Dept result = null;
            var dataSource = Grid1.DataSource as List<Dept>;
            if (dataSource != null && dataSource.Count > 0)
            {
                result = dataSource.FirstOrDefault<Dept>(d => d.ID == deptID);
            }
            return result;
        }

        #endregion

        #region Events


        protected async Task btnSaveClose_ClickAsync(object sender, EventArgs e)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            // 父部门必须存在，且不能把当前部门挂到自己或自己的子部门下面。
            int? parentId = null;
            if (!String.IsNullOrEmpty(ddbParent.Value))
            {
                if (!Int32.TryParse(ddbParent.Value, out var selectedParentId))
                {
                    Alert.Show("无效的上级部门！");
                    return;
                }
                parentId = selectedParentId;
            }

            var visited = new HashSet<int>();
            int? ancestorId = parentId;
            while (ancestorId.HasValue)
            {
                if (ancestorId == _storedDept.ID || !visited.Add(ancestorId.Value))
                {
                    Alert.Show("不能把当前部门设为自己的下级！");
                    return;
                }
                var ancestor = await DB.Depts
                    .Where(dept => dept.ID == ancestorId.Value)
                    .Select(dept => new { dept.ParentID })
                    .FirstOrDefaultAsync();
                if (ancestor == null)
                {
                    Alert.Show("上级部门不存在！");
                    return;
                }
                ancestorId = ancestor.ParentID;
            }

            _storedDept.Name = Dept.Name;
            _storedDept.SortIndex = Dept.SortIndex;
            _storedDept.Remark = Dept.Remark;
            _storedDept.ParentID = parentId;
            await DB.SaveChangesAsync();

            // 关闭本窗体（触发窗体的关闭事件）
            ActiveWindow.HidePostBack();
        }


        #endregion


    }
}
