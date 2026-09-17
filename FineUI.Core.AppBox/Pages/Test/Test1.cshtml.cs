using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineUI.Core.AppBox.Pages.Test
{
    public partial class Test1Model : BaseModel
    {
        private static bool _isOK = false;

        //lock只能锁定一个引用类型变量
        private static object _lock = new object();

        public void OnGet()
        {
            _isOK = false;

            //多线程
            //new System.Threading.Thread(Done).Start();
            //new System.Threading.Thread(Done).Start();


        }


        static void Done()
        {
            //lock只能锁定一个引用类型变量
            //lock (_lock)
            //{
            if (!_isOK)
            {
                FineUI.Core.PageContext.RegisterStartupScript("F.alert('ok');");
                System.Threading.Thread.Sleep(1000);
                _isOK = true;
            }
            //}
        }
    }
}
