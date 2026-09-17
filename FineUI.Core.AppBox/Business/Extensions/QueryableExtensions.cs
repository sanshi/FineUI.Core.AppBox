using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using FineUI.Core;

namespace FineUI.Core.AppBox
{
    public static class QueryableExtensions
    {
        #region Extensions

        /// <summary>
        /// 对 IQueryable 进行排序和分页，并返回结果列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="q">数据源</param>
        /// <param name="grid">Grid 控件，包含分页和排序信息</param>
        /// <returns>排序和分页后的结果列表</returns>
        public static async Task<List<T>> SortAndPageAsync<T>(this IQueryable<T> q, Grid grid) where T : class
        {
            return await SortAndPageAsync(q, grid.PageIndex, grid.PageSize, grid.RecordCount, grid.SortField, grid.SortDirection);
        }


        /// <summary>
        /// 对 IQueryable 进行排序，并返回结果列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="q">数据源</param>
        /// <param name="grid">Grid 控件，包含分页和排序信息</param>
        /// <returns>排序后的结果列表</returns>
        public static async Task<List<T>> SortAsync<T>(this IQueryable<T> q, Grid grid) where T : class
        {
            return await SortAsync(q, grid.SortField, grid.SortDirection);
        }

        #endregion


        #region SortAsync/SortAndPageAsync

        private static IQueryable<T> Sort<T>(IQueryable<T> q, Grid grid)
        {
            return q.SortBy(grid.SortField + " " + grid.SortDirection);
        }

        private static IQueryable<T> Sort<T>(IQueryable<T> q, string sortField, string sortDirection)
        {
            return q.SortBy(sortField + " " + sortDirection);
        }


        // 排序
        private static async Task<List<T>> SortAsync<T>(IQueryable<T> q, string sortField, string sortDirection)
        {
            return await q.SortBy(sortField + " " + sortDirection).ToListAsync();
        }

        // 排序后分页
        private static async Task<List<T>> SortAndPageAsync<T>(IQueryable<T> q, int pageIndex, int pageSize, int recordCount, string sortField, string sortDirection)
        {
            //// 对传入的 pageIndex 进行有效性验证//////////////
            int pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }
            if (pageIndex > pageCount - 1)
            {
                pageIndex = pageCount - 1;
            }
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            ///////////////////////////////////////////////

            return await Sort(q, sortField, sortDirection).Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        }

        #endregion
    }
}