namespace Hsf.EF.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("hsf.baidu_items")]
    public partial class baidu_items
    {
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(50)]
        public string Pid { get; set; }

        public int? byte_length { get; set; }

        public int? byte_offset { get; set; }

        [StringLength(10)]
        public string formal { get; set; }

        [StringLength(255)]
        public string item { get; set; }

        [StringLength(20)]
        public string ne { get; set; }

        [StringLength(20)]
        public string pos { get; set; }

        [StringLength(10)]
        public string uri { get; set; }

        [StringLength(10)]
        public string loc_details { get; set; }

        [StringLength(255)]
        public string basic_words { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
