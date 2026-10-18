
using FineUI.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox
{
    [Authorize]
    public class AdminPageBase : PageBase
    {

        /// <summary>
        /// 每次管理页请求在公共主题之后设置当前用户水印。
        /// </summary>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            // 本基类只供管理页面使用，水印随公共主题在每次请求中设置。
            // 前序处理已返回结果时，不再创建页面管理器。
            if (context.Result != null)
            {
                return;
            }

            var pageManager = PageManager.Instance;
            var watermarkText = GetIdentityName();
            var roleNames = GetIdentityRoleNames(HttpContext);
            if (roleNames.Count > 0)
            {
                watermarkText = $"{roleNames[0]}（{watermarkText}）";
            }

            pageManager.EnableWatermark = true;
            pageManager.WatermarkFontSize = 16;
            pageManager.WatermarkText = watermarkText;
        }


    }
}
