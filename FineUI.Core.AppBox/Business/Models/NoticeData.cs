using System;
using System.Collections.Generic;

namespace FineUI.Core.AppBox
{
    /// <summary>
    /// 公告示例数据（内存，进程内固定几条）。
    ///
    /// 真实项目里这里换成查数据库；换掉之后公开页的其余部分一个字都不用改——
    /// 「免登录」只影响请求能不能进来，不影响进来之后能做什么。
    /// </summary>
    public static class NoticeData
    {
        private static readonly List<Notice> All = new List<Notice>();

        static NoticeData()
        {
            var b = new DateTime(2026, 8, 28, 9, 0, 0);
            Add(8, "关于国庆节放假安排的通知", b, "行政部",
                "根据国务院办公厅通知精神，结合公司实际情况，现将国庆节放假安排通知如下：10 月 1 日至 10 月 7 日放假调休，共 7 天。9 月 28 日（星期日）上班。请各部门提前做好工作安排，值班人员名单于 9 月 25 日前报行政部。");
            Add(7, "员工年度体检启动，请在线预约", b.AddDays(-3).AddHours(5).AddMinutes(30), "人力资源部",
                "2026 年度员工体检已启动，体检机构为市第一人民医院体检中心，预约时间为 9 月 1 日至 9 月 20 日。请各位同事登录内部系统「我的福利」栏目自行预约，体检费用由公司统一结算。");
            Add(6, "机房设备维护导致的服务中断预告", b.AddDays(-6).AddHours(8), "信息中心",
                "为提升系统稳定性，信息中心将于 9 月 12 日 22:00 至次日 2:00 对核心机房进行设备维护。期间办公系统、邮件系统将间断不可用，请各位同事提前保存工作内容。");
            Add(5, "2026 年第三季度安全生产检查结果公示", b.AddDays(-10).AddHours(2).AddMinutes(20), "安全监察部",
                "第三季度安全生产检查已完成，共检查 12 个作业区域，发现隐患 7 项，均已整改闭环。检查结果与整改台账在安全监察部备查，对表现突出的三个班组予以通报表扬。");
            Add(4, "关于开展消防应急演练的通知", b.AddDays(-15).AddHours(1), "安全监察部",
                "为提高全员应急处置能力，定于 9 月 18 日下午 15:00 在办公楼开展消防应急疏散演练。届时请听到警报后按疏散指示图有序撤离至一号广场集合，各层疏散引导员按分工到位。");
            Add(3, "新版办公系统上线及培训安排", b.AddDays(-21).AddMinutes(30), "信息中心",
                "新版办公系统已完成上线，登录地址与账号保持不变。为帮助大家熟悉新界面，信息中心安排三场集中培训，时间分别为 9 月 5 日、9 月 8 日、9 月 10 日下午 14:00，地点在三楼培训室，可任选一场参加。");
            Add(2, "档案室搬迁期间的借阅安排", b.AddDays(-28).AddHours(7).AddMinutes(45), "行政部",
                "档案室将于 9 月 1 日至 9 月 10 日搬迁至新办公楼负一层。搬迁期间纸质档案暂停借阅，急需调阅的请联系行政部登记，由专人协助办理。电子档案查询不受影响。");
            Add(1, "关于规范办公用品领用流程的通知", b.AddDays(-35).AddHours(-1).AddMinutes(-10), "行政部",
                "为控制办公成本，办公用品领用自 9 月 1 日起统一由部门助理按月汇总申请，个人不再单独领取。清单模板见行政部共享目录，每月 25 日前提交次月计划。");
        }

        private static void Add(int id, string title, DateTime publishTime, string department, string content)
        {
            All.Add(new Notice
            {
                ID = id,
                Title = title,
                PublishTime = publishTime,
                Department = department,
                Content = content
            });
        }

        /// <summary>
        /// 全部公告，按发布时间倒序（最新在前）
        /// </summary>
        public static List<Notice> GetAll()
        {
            return All;
        }

        /// <summary>
        /// 按主键取一条，找不到返回 null
        /// </summary>
        public static Notice Find(int id)
        {
            foreach (var notice in All)
            {
                if (notice.ID == id)
                {
                    return notice;
                }
            }
            return null;
        }
    }
}
