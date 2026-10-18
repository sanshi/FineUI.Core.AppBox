using System;
using Microsoft.AspNetCore.Http;

namespace FineUI.Core.AppBox
{
    /// <summary>
    /// 每次页面请求读取当前用户偏好；本页的 Page_Init 可以随后覆盖这些公共配置。
    /// </summary>
    public static class AppPageManagerInitializer
    {
        public static void Initialize(PageManager pm, HttpRequest request)
        {
            ApplyTheme(pm, request);
        }

        private static void ApplyTheme(PageManager pm, HttpRequest request)
        {
            var themeName = request.Cookies["Theme"];
            if (string.IsNullOrEmpty(themeName))
            {
                return;
            }

            if (Enum.TryParse(themeName, true, out Theme theme)
                && Enum.IsDefined(typeof(Theme), theme))
            {
                // 切换回内置主题时，先清空自定义主题。
                pm.CustomTheme = string.Empty;
                pm.Theme = theme;
            }
            else
            {
                pm.CustomTheme = themeName;
            }
        }
    }
}
