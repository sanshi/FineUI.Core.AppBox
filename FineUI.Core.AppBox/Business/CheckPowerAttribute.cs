using System;
using System.Threading.Tasks;

using FineUI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FineUI.Core.AppBox
{
    /// <summary>
    /// 页面级权限验证过滤器：把权限名标在页面模型类上，未通过则不执行页面处理器。
    ///
    /// 实现的是页面过滤器（IAsyncPageFilter）而不是结果过滤器：结果过滤器在处理器方法之后才运行，
    /// 那时数据早已查过、写操作也已发生，只是把输出换掉——等于没拦住。
    /// </summary>
    // 全限定 System.Attribute：FineUI.Core 命名空间下也有一个 Attribute 类型，直接写会歧义
    public class CheckPowerAttribute : System.Attribute, IAsyncPageFilter, IOrderedFilter
    {
        /// <summary>
        /// 权限名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 过滤器执行顺序：负数让它排在框架注册的全局过滤器（默认顺序 0）之前。
        ///
        /// 过滤器先按 Order 升序排，Order 相同才按作用域（全局 → 页面 → 处理器方法）排。
        /// 不指定 Order 的话，标在页面模型类上的过滤器排在全局过滤器的里层；
        /// 里层短路只能让外层的 next() 提前返回，外层在 next() 之后要做的事照做。
        /// 排到最外层，校验不通过时整条页面流水线一步都不走——控件状态不恢复、Page_Load
        /// 与控件事件都不执行。取 -10000 而不是最小值，是给将来可能更靠前的过滤器留出余地。
        /// </summary>
        public int Order => -10000;

        /// <summary>
        /// 处理器方法选定阶段：本过滤器不需要在此做事
        /// </summary>
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 处理器方法执行前校验权限：不通过就短路（不调用 next），处理器不会执行
        /// </summary>
        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            HttpContext httpContext = context.HttpContext;

            // 权限验证通过：照常执行页面处理器
            if (String.IsNullOrEmpty(Name) || BaseModel.CheckPower(httpContext, Name))
            {
                await next.Invoke();
                return;
            }

            if (httpContext.Request.Method == "GET")
            {
                BaseModel.CheckPowerFailWithPage(httpContext);

                //http://stackoverflow.com/questions/9837180/how-to-skip-action-execution-from-an-actionfilter
                context.Result = new EmptyResult();
            }
            else if (httpContext.Request.Method == "POST")
            {
                BaseModel.CheckPowerFailWithAlert();

                // https://www.cnblogs.com/sanshi/p/18193981
                context.Result = UIHelper.Result();
            }
            else
            {
                context.Result = new EmptyResult();
            }
        }
    }
}
