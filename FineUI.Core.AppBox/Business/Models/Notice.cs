using System;
using System.ComponentModel.DataAnnotations;

namespace FineUI.Core.AppBox
{
    /// <summary>
    /// 公告：公开页的表格行与详情内容。
    ///
    /// 本示例的公告数据由 NoticeData 提供的内存列表充当，所以这里没有 [Key]、也不参与数据库映射
    /// （示例要演示的是「免登录页面」，不想让读者为了跑通它先去建一张表）。真实项目把它换成
    /// 与其它模型一样的实体、由数据访问层查库即可——公开页读数据库与需要登录的页面没有任何区别。
    /// </summary>
    public class Notice
    {
        public int ID { get; set; }

        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "发布时间")]
        public DateTime PublishTime { get; set; }

        [Display(Name = "发布部门")]
        public string Department { get; set; }

        /// <summary>
        /// 正文：只在详情页显示，列表页不取此列
        /// </summary>
        public string Content { get; set; }
    }
}
