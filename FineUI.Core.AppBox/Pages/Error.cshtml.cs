using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FineUI.Core.AppBox.Pages
{
    /// <summary>
    /// 错误页只渲染普通 HTML，不需要创建 FineUI 页面管理器。
    /// </summary>
    public partial class ErrorModel : PageModel
    {
        /// <summary>本次错误的提示文字：默认为空，处理器运行后为“Error”或异常信息。</summary>
        public string ErrorMessage { get; private set; }

        /// <summary>本次请求的追踪编号：默认为空，处理器运行后从活动记录或 HTTP 请求取得。</summary>
        public string RequestId { get; private set; }

        public void OnGet()
        {
            ErrorMessage = "Error";

            var exception = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exception != null)
            {
                ErrorMessage = exception.Error.Message;
            }

            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}