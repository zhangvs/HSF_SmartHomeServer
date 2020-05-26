namespace Hsf.EF.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("hsf.hsf_owner")]
    public partial class hsf_owner
    {
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(20)]
        public string telphone { get; set; }

        [StringLength(50)]
        public string password { get; set; }

        [Display(Name = "ÒµÖ÷ÐÕÃû")]
        [StringLength(50)]
        public string ownername { get; set; }

        [StringLength(20)]
        public string chinaname { get; set; }

        [StringLength(50)]
        public string residential { get; set; }

        [StringLength(10)]
        public string building { get; set; }

        [StringLength(10)]
        public string unit { get; set; }

        [StringLength(10)]
        public string floor { get; set; }

        [StringLength(10)]
        public string room { get; set; }

        [StringLength(50)]
        public string host { get; set; }

        [StringLength(50)]
        public string userid { get; set; }

        public DateTime? createtime { get; set; }

        public int? deletemark { get; set; }
    }
}
