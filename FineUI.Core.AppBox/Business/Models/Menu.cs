using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FineUI.Core.AppBox
{
    public class Menu : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "菜单名称")]
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "图标")]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [Display(Name = "链接")]
        [StringLength(200)]
        public string NavigateUrl { get; set; }

        [Display(Name = "备注")]
        [StringLength(500)]
        public string Remark { get; set; }

        [Display(Name = "排序")]
        [Required]
        public int SortIndex { get; set; }





        [Display(Name = "上级菜单")]
        public int? ParentID { get; set; }
        public Menu Parent { get; set; }


		[Display(Name = "浏览权限")]
        public int? ViewPowerID { get; set; }
        
        public Power ViewPower { get; set; }

		
		
		// 计算属性
		[Display(Name = "浏览权限")]
        public string ViewPowerName
        {
            get
            {
                return ViewPower?.Name;
            }
        }
        
        public List<Menu> Children { get; set; }




    }
}