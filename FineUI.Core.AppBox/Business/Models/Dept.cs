using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FineUI.Core.AppBox
{
    public class Dept : IKeyID
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "名称")]
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "排序")]
        [Required]
        public int SortIndex { get; set; }

        [Display(Name = "备注")]
        [StringLength(500)]
        public string Remark { get; set; }



        [Display(Name = "上级部门")]
        public int? ParentID { get; set; }
		
        public Dept Parent { get; set; }


        
        public List<Dept> Children { get; set; }


        public List<User> Users { get; set; }




    }
}