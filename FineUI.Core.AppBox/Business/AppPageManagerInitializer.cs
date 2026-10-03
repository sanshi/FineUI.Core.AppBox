using System;
using Microsoft.AspNetCore.Http;

namespace FineUI.Core.AppBox
{
    /// <summary>
    /// 首屏公共配置：读取当前用户偏好，页面可以在后台处理方法中进一步覆盖。
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
