

using FineUI.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox.Pages
{
    public partial class LoginModel : BaseModel
    {
        #region Fields



        #endregion

        #region Page_Get

        public void Page_Get(object sender, EventArgs e)
        {
            // 已登录用户打开登录地址时直接跳到首页，框架随后跳过登录页模板和 Page_Load。
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Window1.Title = String.Format("FineUI.Core.AppBox v{0}", GetProductVersion());
            }
        }


        #endregion


        #region Events

        protected async Task btnSubmit_ClickAsync(object sender, EventArgs e)
        {
            string userName = tbxUserName.Text.Trim();
            string password = tbxPassword.Text.Trim();

            var user = await DB.Users
                .Include(u => u.Roles)
                .Where(u => u.Name == userName).AsNoTracking().FirstOrDefaultAsync();

            if (user != null)
            {
                if (PasswordUtil.ComparePasswords(user.Password, password))
                {
                    if (!user.Enabled)
                    {
                        Alert.Show("用户未启用，请联系管理员！");
                    }
                    else
                    {
                        // 登录成功
                        await LoginSuccess(user);

                        // 从受保护页面跳到登录页时，身份验证组件会把原页面地址写入响应的跳转头。
                        // 例如访问 /Admin/Role 后登录，应只跳回该页；普通登录才打开首页。
                        if (Response.StatusCode != StatusCodes.Status302Found)
                        {
                            FineUI.Core.PageContext.Redirect("/Index");
                        }
                    }
                }
                else
                {
                    Alert.Show("用户名或密码错误！");
                }
            }
            else
            {
                Alert.Show("用户名或密码错误！");
            }
        }


        private async Task LoginSuccess(User user)
        {
            await RegisterOnlineUserAsync(user.ID);

            // 用户所属的角色字符串，以逗号分隔
            string roleIDs = String.Empty;
            if (user.Roles != null)
            {
                roleIDs = String.Join(",", user.Roles.Select(r => r.ID).ToArray());
            }

            var claims = new[] { new Claim("UserID", user.ID.ToString()), new Claim("UserName", user.Name), new Claim("RoleIDs", roleIDs) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal userInfo = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userInfo,
                new AuthenticationProperties()
                {
                    IsPersistent = false
                });

            // 与身份绑定的会话缓存必须显式清掉：身份 Cookie 过期后换个账号登进来时，
            // 浏览器的会话还是原来那个，不清就会沿用上一个人的权限列表
            HttpContext.Session.Remove(SK_USER_POWER_LIST);
            HttpContext.Session.Remove(SK_USER_POWER_VERSION);
            HttpContext.Session.Remove(SK_ACTIVE_CHECKED_AT);

        }

        #endregion


    }
}
