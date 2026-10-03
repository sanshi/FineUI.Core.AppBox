
using FineUI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox
{
    [Authorize]
    public class BaseAdminModel : BaseModel
    {

        /// <summary>
        /// 页面处理器调用之前执行
        /// </summary>
        /// <param name="context"></param>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            if (HttpMethods.IsGet(Request.Method) && context.Result == null)
            {
                var pmInstance = PageManager.Instance;

                // 当前登录用户名称和角色信息
                var watermarkText = GetIdentityName();
                var roleNames = GetIdentityRoleNames(context.HttpContext);
                if (roleNames.Count > 0)
                {
                    watermarkText = $"{roleNames[0]}（{watermarkText}）";
                }

                // 公共主题初始化之后、单页 OnGet 之前设置管理页的用户水印。
                pmInstance.EnableWatermark = true;
                pmInstance.WatermarkFontSize = 16;
                pmInstance.WatermarkText = watermarkText;
            }
            
        }


    }
}
