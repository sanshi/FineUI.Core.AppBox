using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using FineUI.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Configuration;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Reflection;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace FineUI.Core.AppBox
{
    public class BaseModel : PageModel
    {
        #region IsPostBack

        /// <summary>
        /// 是否页面回发
        /// </summary>
        public bool IsPostBack
        {
            get
            {
                return FineUI.Core.PageContext.IsFineUIAjaxPostBack();
            }
        }

        #endregion

        #region RegisterStartupScript

        /// <summary>
        /// 注册客户端脚本
        /// </summary>
        /// <param name="scripts"></param>
        public void RegisterStartupScript(string scripts)
        {
            FineUI.Core.PageContext.RegisterStartupScript(scripts);
        }

        #endregion

        #region ViewBag

        private DynamicViewData _viewBag;

        /// <summary>
        /// 给 PageModel 加 ViewBag
        /// https://forums.asp.net/t/2128012.aspx?Razor+Pages+ViewBag+has+gone+
        /// https://github.com/aspnet/Mvc/issues/6754
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new DynamicViewData(ViewData);
                }
                return _viewBag;
            }
        }
        #endregion

        #region 只读静态变量

        private static readonly string SK_ONLINE_UPDATE_TIME = "OnlineUpdateTime";

        /// <summary>会话键：当前用户拥有的权限名列表缓存</summary>
        protected static readonly string SK_USER_POWER_LIST = "UserPowerList";
        /// <summary>会话键：上面那份缓存是按哪个权限版本号算出来的</summary>
        protected static readonly string SK_USER_POWER_VERSION = "UserPowerVersion";
        /// <summary>会话键：上次复核「用户仍存在且启用」的时间</summary>
        protected static readonly string SK_ACTIVE_CHECKED_AT = "ActiveCheckedAt";

        /// <summary>复核用户是否仍然有效的间隔（秒）</summary>
        private const int ACTIVE_CHECK_INTERVAL_SECONDS = 60;

        /// <summary>
        /// 全局权限版本号：角色的权限集合、用户的角色集合一有变化就加一，
        /// 各会话据此丢弃自己缓存的权限列表。
        ///
        /// 它是本进程内的静态计数：多实例部署时各实例互不通知，被改动的那台之外仍会用旧缓存
        /// 到会话结束；真要做集群，把它换成共享存储（如 Redis）里的一个计数或按用户的失效标记。
        /// </summary>
        /// <remarks>
        /// 从 1 开始，不能从 0 开始：会话里没存过版本号时 GetObject&lt;long&gt; 返回 0，
        /// 只有让 0 不等于任何有效版本，「有列表但没版本号」才会被判成过期而重算。
        /// </remarks>
        private static long _permissionVersion = 1;

        public static readonly string CHECK_POWER_FAIL_PAGE_MESSAGE = "您无权访问此页面！";
        public static readonly string CHECK_POWER_FAIL_ACTION_MESSAGE = "您无权进行此操作！";

        #endregion

        #region OnActionExecuting

        /// <summary>
        /// 页面处理器调用之前执行
        /// </summary>
        /// <param name="context"></param>
        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);

            // 如果用户已经登录，更新在线记录
            if (User.Identity.IsAuthenticated)
            {
                UpdateOnlineUser(GetIdentityID());
            }
        }

        /// <summary>
        /// 每个请求都复核一次「这个会话对应的用户是否仍然存在且启用」。
        ///
        /// 登录身份是登录那一刻的快照，账号事后被禁用或删除，他已经登录的会话不会自动失效。
        /// 复核结果按固定间隔缓存在会话里：不缓存就是每个请求一条查询，缓存太久则禁用迟迟不生效。
        /// 失效即登出——首屏跳登录页，回发返回 401，由客户端 common.js 的 F.beforeAjaxError 接住。
        /// </summary>
        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            // 登录页本身不做这道复核：否则浏览器里存着已禁用账号的 Cookie 时，
            // 连「换一个账号登录」都会被这里挡回去（提交登录表单也是一次回发）
            bool isLoginPage = HttpContext.Request.Path.StartsWithSegments("/Login");

            if (!isLoginPage && User.Identity.IsAuthenticated && !await IsActiveUserAsync())
            {
                await HttpContext.SignOutAsync();
                HttpContext.Session.Clear();

                if (HttpContext.Request.Method == "POST")
                {
                    // 回发返回 401 纯文本：这一步比 FineUI 的脚本管道更靠外，此时注册启动脚本不会进到响应里，
                    // 而 401 客户端本来就认得——common.js 的 F.beforeAjaxError 会提示并跳转登录页
                    context.Result = new ContentResult
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        ContentType = "text/plain;charset=UTF-8",
                        Content = "账号已被禁用或删除，请重新登录！"
                    };
                }
                else
                {
                    context.Result = new RedirectResult("/Login");
                }

                // 不调用 next：本次请求就此中止
                return;
            }

            await base.OnPageHandlerExecutionAsync(context, next);
        }

        /// <summary>
        /// 当前会话对应的用户是否仍然存在且启用（结果按 ACTIVE_CHECK_INTERVAL_SECONDS 秒缓存在会话里）
        /// </summary>
        protected async Task<bool> IsActiveUserAsync()
        {
            DateTime? checkedAt = HttpContext.Session.GetObject<DateTime?>(SK_ACTIVE_CHECKED_AT);
            if (checkedAt != null && DateTime.UtcNow.Subtract(checkedAt.Value).TotalSeconds < ACTIVE_CHECK_INTERVAL_SECONDS)
            {
                return true;
            }

            int? userID = GetIdentityID();
            if (userID == null)
            {
                return false;
            }

            bool active = await DB.Users.AnyAsync(u => u.ID == userID && u.Enabled);
            if (active)
            {
                // 用 UTC 记时间：本地时间遇到夏令时或对时回拨会算出负数，负数照样小于间隔，复核就被跳过了
                HttpContext.Session.SetObject<DateTime?>(SK_ACTIVE_CHECKED_AT, DateTime.UtcNow);
            }

            return active;
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);
        }

        #endregion

        #region 请求参数

        /// <summary>
        /// 获取查询字符串中的参数值
        /// </summary>
        protected string GetQueryValue(string queryKey)
        {
            return Request.Query[queryKey].ToString();
        }


        /// <summary>
        /// 获取查询字符串中的参数值
        /// </summary>
        protected int GetQueryIntValue(string queryKey)
        {
            int queryIntValue = -1;
            try
            {
                queryIntValue = Convert.ToInt32(Request.Query[queryKey]);
            }
            catch (Exception) { }

            return queryIntValue;
        }

        #endregion

        #region GetRequestEventArguments

        /// <summary>
        /// 获取回发的目标控件
        /// </summary>
        /// <returns></returns>
        public string GetRequestEventTarget()
        {
            return Request.Form["__EVENTTARGET"];
        }

        /// <summary>
        /// 获取回发的参数
        /// </summary>
        /// <returns></returns>
        public string GetRequestEventArgument()
        {
            return Request.Form["__EVENTARGUMENT"];
        }

        /// <summary>
        /// 获取回发的参数列表
        /// </summary>
        /// <returns></returns>
        public string[] GetRequestEventArguments()
        {
            var arg = GetRequestEventArgument();
            return arg.Split("$");
        }
        #endregion

        #region ShowNotify

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowNotify(string message)
        {
            ShowNotify(message, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon)
        {
            ShowNotify(message, messageIcon, Target.Top);
        }

        /// <summary>
        /// 显示通知对话框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageIcon"></param>
        /// <param name="target"></param>
        public virtual void ShowNotify(string message, MessageBoxIcon messageIcon, Target target)
        {
            Notify n = new Notify
            {
                Target = target,
                Message = message,
                MessageBoxIcon = messageIcon,
                PositionX = Position.Center,
                PositionY = Position.Top,
                DisplayMilliseconds = 3000,
                ShowHeader = false
            };

            n.Show();
        }


        #endregion

        #region 在线用户相关

        protected void UpdateOnlineUser(int? userID)
        {
            if (userID == null)
            {
                return;
            }

            DateTime now = DateTime.Now;
            object lastUpdateTime = HttpContext.Session.GetObject<DateTime>(SK_ONLINE_UPDATE_TIME);
            if (lastUpdateTime == null || (now.Subtract(Convert.ToDateTime(lastUpdateTime)).TotalMinutes > 5))
            {
                // 记录本次更新时间
                HttpContext.Session.SetObject<DateTime>(SK_ONLINE_UPDATE_TIME, now);

                Online online = DB.Onlines.Where(o => o.UserID == userID).FirstOrDefault();
                if (online != null)
                {
                    online.UpdateTime = now;
                    DB.SaveChanges();
                }

            }
        }

        protected async Task RegisterOnlineUserAsync(int userID)
        {
            DateTime now = DateTime.Now;

            Online online = await DB.Onlines.Where(o => o.UserID == userID).FirstOrDefaultAsync();

            // 如果不存在，就创建一条新的记录
            if (online == null)
            {
                online = new Online();
                DB.Onlines.Add(online);
            }
            online.UserID = userID;
            online.IPAdddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            online.LoginTime = now;
            online.UpdateTime = now;

            await DB.SaveChangesAsync();

            // 记录本次更新时间
            HttpContext.Session.SetObject<DateTime>(SK_ONLINE_UPDATE_TIME, now);
        }

        /// <summary>
        /// 在线人数
        /// </summary>
        /// <returns></returns>
        protected async Task<int> GetOnlineCountAsync()
        {
            DateTime lastM = DateTime.Now.AddMinutes(-15);
            return await DB.Onlines.Where(o => o.UpdateTime > lastM).CountAsync();
        }

        #endregion

        #region 当前登录用户信息

        // http://blog.163.com/zjlovety@126/blog/static/224186242010070024282/
        // http://www.cnblogs.com/gaoshuai/articles/1863231.html
        /// <summary>
        /// 当前登录用户的角色列表
        /// </summary>
        /// <returns></returns>
        protected List<int> GetIdentityRoleIDs()
        {
            return GetIdentityRoleIDs(HttpContext);
        }

        #endregion

        #region 权限检查

        /// <summary>
        /// 检查当前用户是否拥有某个权限
        /// </summary>
        /// <param name="powerType"></param>
        /// <returns></returns>
        protected bool CheckPower(string powerName)
        {
            return CheckPower(HttpContext, powerName);
        }

        /// <summary>
        /// 获取当前登录用户拥有的全部权限列表
        /// </summary>
        /// <param name="roleIDs"></param>
        /// <returns></returns>
        protected List<string> GetRolePowerNames()
        {
            return GetRolePowerNames(HttpContext);
        }

        /// <summary>
        /// 检查权限失败（页面第一次加载）
        /// </summary>
        public static void CheckPowerFailWithPage(HttpContext context)
        {
            string PageTemplate = "<!DOCTYPE html><html><head><meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"/><head><body>{0}</body></html>";
            context.Response.WriteAsync(String.Format(PageTemplate, CHECK_POWER_FAIL_PAGE_MESSAGE));
        }

        /// <summary>
        /// 检查权限失败（页面回发）
        /// </summary>
        public static void CheckPowerFailWithAlert()
        {
            FineUI.Core.PageContext.RegisterStartupScript(Alert.GetShowInTopReference(CHECK_POWER_FAIL_ACTION_MESSAGE));
        }

        /// <summary>
        /// 检查当前用户是否拥有某个权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="powerName"></param>
        /// <returns></returns>
        public static bool CheckPower(HttpContext context, string powerName)
        {
            // 如果权限名为空，则放行
            if (String.IsNullOrEmpty(powerName))
            {
                return true;
            }

            // 当前登陆用户的权限列表
            List<string> rolePowerNames = GetRolePowerNames(context);
            if (rolePowerNames.Contains(powerName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前登录用户拥有的全部权限列表
        /// </summary>
        /// <param name="roleIDs"></param>
        /// <returns></returns>
        public static List<string> GetRolePowerNames(HttpContext context)
        {
            // 版本号要在查库之前取：万一取完之后有人改了权限，最坏是「新数据配旧版本号」，
            // 下次不匹配再算一遍而已。反过来先查库后取版本号，会存下「新版本号配旧数据」，那就永远不会再重算了
            long version = Interlocked.Read(ref _permissionVersion);

            // 权限列表缓存在会话里，避免一个请求里反复查库（页面级判一次、每个按钮判一次、事件里再判一次）。
            // 光判「有没有缓存」是不够的：那样管理员改了角色权限，当事人要重新登录才生效。
            // 所以连同「算这份缓存时的版本号」一起存，版本对不上就重算。
            var cached = context.Session.GetObject<List<string>>(SK_USER_POWER_LIST);
            if (cached != null && context.Session.GetObject<long>(SK_USER_POWER_VERSION) == version)
            {
                return cached;
            }

            var db = BaseModel.GetDbConnection();

            List<string> rolePowerNames = new List<string>();

            // 超级管理员拥有所有权限
            if (GetIdentityName(context) == "admin")
            {
                rolePowerNames = db.Powers.Select(p => p.Name).ToList();
            }
            else
            {
                // 按用户主键重新查角色，不用身份信息里的角色标识：那是登录那一刻的快照，
                // 管理员给某人换了角色之后，只有重新查库才能拿到新的角色集合
                int? userID = GetIdentityID(context);

                var user = db.Users.Include(u => u.Roles).ThenInclude(r => r.Powers)
                    .AsNoTracking().Where(u => u.ID == userID).FirstOrDefault();

                if (user != null && user.Roles != null)
                {
                    foreach (var role in user.Roles)
                    {
                        foreach (var power in role.Powers)
                        {
                            if (!rolePowerNames.Contains(power.Name))
                            {
                                rolePowerNames.Add(power.Name);
                            }
                        }
                    }
                }
            }

            context.Session.SetObject<List<string>>(SK_USER_POWER_LIST, rolePowerNames);
            context.Session.SetObject<long>(SK_USER_POWER_VERSION, version);

            return rolePowerNames;
        }

        /// <summary>
        /// 让所有会话缓存的权限列表作废：凡是改动了「用户—角色—权限」三者关系的地方都要调用，
        /// 受影响的用户下一次请求就会按新权限判定，不必重新登录。
        ///
        /// 禁用或删除用户不必调用它：那两件事由每请求的账号有效性复核兜住（最迟 60 秒生效）。
        /// </summary>
        public static void InvalidatePermissionCaches()
        {
            Interlocked.Increment(ref _permissionVersion);
        }

        // http://blog.163.com/zjlovety@126/blog/static/224186242010070024282/
        // http://www.cnblogs.com/gaoshuai/articles/1863231.html
        /// <summary>
        /// 当前登录用户的角色列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetIdentityRoleNames(HttpContext context)
        {
            List<string> roleNames = new List<string>();

            var db = BaseModel.GetDbConnection();

            if (context.User.Identity.IsAuthenticated)
            {
                // 超级管理员拥有所有权限
                if (GetIdentityName(context) == "admin")
                {
                    return new List<string> { "超级管理员" };
                }
                else
                {
                    List<int> roleIDs = GetIdentityRoleIDs(context);

                    roleNames = db.Roles.Where(r => roleIDs.Contains(r.ID)).Select(r => r.Name).ToList();
                }
            }

            return roleNames;
        }

        public static List<int> GetIdentityRoleIDs(HttpContext context)
        {
            List<int> roleIDs = new List<int>();

            if (context.User.Identity.IsAuthenticated)
            {
                string userData = context.User.Claims.Where(x => x.Type == "RoleIDs").FirstOrDefault().Value;

                foreach (string roleID in userData.Split(','))
                {
                    if (!String.IsNullOrEmpty(roleID))
                    {
                        roleIDs.Add(Convert.ToInt32(roleID));
                    }
                }
            }

            return roleIDs;
        }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        /// <returns></returns>
        protected string GetIdentityName()
        {
            return GetIdentityName(HttpContext);
        }

        /// <summary>
        /// 当前登录用户名
        /// </summary>
        /// <returns></returns>
        public static string GetIdentityName(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userName = context.User.Claims.Where(x => x.Type == "UserName").FirstOrDefault().Value;
            return userName;
        }

        /// <summary>
        /// 当前登录用户标识符
        /// </summary>
        /// <returns></returns>
        protected int? GetIdentityID()
        {
            return GetIdentityID(HttpContext);
        }

        /// <summary>
        /// 当前登录用户标识符
        /// </summary>
        /// <returns></returns>
        public static int? GetIdentityID(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userID = context.User.Claims.Where(x => x.Type == "UserID").FirstOrDefault().Value;
            return Convert.ToInt32(userID);
        }

        #endregion

        #region InvalidModelState

        //protected void InvalidModelState(ModelStateDictionary state)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    sb.Append("<ul>");
        //    foreach (var key in state.Keys)
        //    {
        //        //将错误描述添加到sb中
        //        foreach (var error in state[key].Errors)
        //        {
        //            sb.AppendFormat("<li>{0}</li>", error.ErrorMessage);
        //        }
        //    }
        //    sb.Append("</ul>");

        //    Alert.Show(sb.ToString());
        //}

        #endregion

        #region GetProductVersion

        protected string GetProductVersion()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            return String.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build);
        }

        #endregion

        #region DB

        private AppBoxContext _db;
        /// <summary>
        /// 每个请求共享一个数据库连接实例
        /// </summary>
        protected AppBoxContext DB
        {
            get
            {
                if (_db == null)
                {
                    _db = BaseModel.GetDbConnection();
                }
                return _db;
            }
        }

        /// <summary>
        /// 获取数据库连接实例（静态方法）
        /// </summary>
        /// <returns></returns>
        public static AppBoxContext GetDbConnection()
        {
            return FineUI.Core.PageContext.GetRequestService<AppBoxContext>();
        }

        #region GetReflectionProperties

        /// <summary>
        /// 获取实例的属性名称列表
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private string[] GetReflectionProperties(object instance)
        {
            var result = new List<string>();
            foreach (PropertyInfo property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propertyName = property.Name;
                // NotMapped特性
                var notMappedAttr = property.GetCustomAttribute<NotMappedAttribute>(false);
                if (notMappedAttr == null && propertyName != "ID")
                {
                    result.Add(propertyName);
                }
            }
            return result.ToArray();
        }

        #endregion

        #region SortAsync/SortAndPageAsync

        //protected IQueryable<T> Sort<T>(IQueryable<T> q, Grid grid)
        //{
        //    return q.SortBy(grid.SortField + " " + grid.SortDirection);
        //}

        //protected IQueryable<T> Sort<T>(IQueryable<T> q, string sortField, string sortDirection)
        //{
        //    return q.SortBy(sortField + " " + sortDirection);
        //}


        //protected async Task<List<T>> SortAsync<T>(IQueryable<T> q, Grid grid)
        //{
        //    return await q.SortBy(grid.SortField + " " + grid.SortDirection).ToListAsync();
        //}

        //// 排序
        //protected async Task<List<T>> SortAsync<T>(IQueryable<T> q, string sortField, string sortDirection)
        //{
        //    return await q.SortBy(sortField + " " + sortDirection).ToListAsync();
        //}

        //protected async Task<List<T>> SortAndPageAsync<T>(IQueryable<T> q, Grid grid)
        //{
        //    return await SortAndPageAsync(q, grid.PageIndex, grid.PageSize, grid.RecordCount, grid.SortField, grid.SortDirection);
        //}

        //// 排序后分页
        //protected async Task<List<T>> SortAndPageAsync<T>(IQueryable<T> q, int pageIndex, int pageSize, int recordCount, string sortField, string sortDirection)
        //{
        //    //// 对传入的 pageIndex 进行有效性验证//////////////
        //    int pageCount = recordCount / pageSize;
        //    if (recordCount % pageSize != 0)
        //    {
        //        pageCount++;
        //    }
        //    if (pageIndex > pageCount - 1)
        //    {
        //        pageIndex = pageCount - 1;
        //    }
        //    if (pageIndex < 0)
        //    {
        //        pageIndex = 0;
        //    }
        //    ///////////////////////////////////////////////

        //    return await Sort(q, sortField, sortDirection).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        //}

        #endregion

        #region Attach/AddEntities/ReplaceEntities

        // 附加实体到数据库上下文中（首先在Local中查找实体是否存在，不存在才Attach，否则会报错）
        // http://patrickdesjardins.com/blog/entity-framework-4-3-an-object-with-the-same-key-already-exists-in-the-objectstatemanager
        protected T Attach<T>(int keyID) where T : class, IKeyID, new()
        {
            T t = DB.Set<T>().Local.Where(x => x.ID == keyID).FirstOrDefault();
            if (t == null)
            {
                t = new T { ID = keyID };
                DB.Set<T>().Attach(t);
            }
            return t;
        }

        // 向现有实体集合中添加新项
        protected void AddEntities<T>(ICollection<T> existItems, int[] newItemIDs) where T : class, IKeyID, new()
        {
            foreach (int roleID in newItemIDs)
            {
                T t = Attach<T>(roleID);
                existItems.Add(t);
            }
        }

        // 替换现有实体集合中的所有项
        // http://stackoverflow.com/questions/2789113/entity-framework-update-entity-along-with-child-entities-add-update-as-necessar
        protected void ReplaceEntities<T>(ICollection<T> existEntities, int[] newEntityIDs) where T : class, IKeyID, new()
        {
            if (newEntityIDs.Length == 0)
            {
                existEntities.Clear();
            }
            else
            {
                int[] tobeAdded = newEntityIDs.Except(existEntities.Select(x => x.ID)).ToArray();
                int[] tobeRemoved = existEntities.Select(x => x.ID).Except(newEntityIDs).ToArray();

                AddEntities<T>(existEntities, tobeAdded);

                existEntities.Where(x => tobeRemoved.Contains(x.ID)).ToList().ForEach(e => existEntities.Remove(e));
                //foreach (int roleID in tobeRemoved)
                //{
                //    existEntities.Remove(existEntities.Single(r => r.ID == roleID));
                //}
            }
        }

        // http://patrickdesjardins.com/blog/validation-failed-for-one-or-more-entities-see-entityvalidationerrors-property-for-more-details-2
        // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors


        #endregion

        #endregion


    }
}
