namespace Hsf.EF.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class HsfDBContext : DbContext
    {
        public HsfDBContext()
            : base("name=HsfDBContext")
        {
        }

        public virtual DbSet<baidu_items> baidu_items { get; set; }
        public virtual DbSet<baidu_terms> baidu_terms { get; set; }
        public virtual DbSet<host_account> host_account { get; set; }
        public virtual DbSet<host_device> host_device { get; set; }
        public virtual DbSet<host_room> host_room { get; set; }
        public virtual DbSet<sound_fail> sound_fail { get; set; }
        public virtual DbSet<sound_host> sound_host { get; set; }
        public virtual DbSet<smsinfo> smsinfo { get; set; }
        public virtual DbSet<hsf_owner> hsf_owner { get; set; }
        public virtual DbSet<hsf_outdevice> hsf_outdevice { get; set; }
        public virtual DbSet<hsf_residential> hsf_residential { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<baidu_items>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.Pid)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.formal)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.item)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.ne)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.pos)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.uri)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.loc_details)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_items>()
                .Property(e => e.basic_words)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Message)
                .IsUnicode(false);

            modelBuilder.Entity<baidu_terms>()
                .Property(e => e.Terms)
                .IsUnicode(false);

            modelBuilder.Entity<host_account>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<host_account>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<host_account>()
                .Property(e => e.Mac)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.deviceid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devtype)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devip)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devmac)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devport)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.cachekey)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devposition)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devchannel)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.powvalue)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate1)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.devstate2)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.account)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.mac)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.createuser)
                .IsUnicode(false);

            modelBuilder.Entity<host_device>()
                .Property(e => e.modifiyuser)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.id)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.posid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.postype)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.Account)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.Mac)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.CreateUser)
                .IsUnicode(false);

            modelBuilder.Entity<host_room>()
                .Property(e => e.ModifyUser)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.sessionId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.deviceId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.actionId)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.token)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.questions)
                .IsUnicode(false);

            modelBuilder.Entity<sound_fail>()
                .Property(e => e.IPAddress)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.id)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.classfid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.deviceid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devip)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devmac)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devport)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devposition)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devregcode)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devtype)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate1)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.devstate2)
                .IsUnicode(false);

            modelBuilder.Entity<sound_host>()
                .Property(e => e.token)
                .IsUnicode(false);

            modelBuilder.Entity<smsinfo>()
                .Property(e => e.Tel)
                .IsUnicode(false);

            modelBuilder.Entity<smsinfo>()
                .Property(e => e.Captcha)
                .IsUnicode(false);

            modelBuilder.Entity<smsinfo>()
                .Property(e => e.SmsContent)
                .IsUnicode(false);


            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.deviceid)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devtype)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devip)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devmac)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devport)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.cachekey)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devposition)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devchannel)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.imageid)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.userid)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.powvalue)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devstate)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devstate1)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.devstate2)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.account)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.mac)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.createuser)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.modifiyuser)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.residential)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.building)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_outdevice>()
                .Property(e => e.unit)
                .IsUnicode(false);
            


            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.telphone)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.residential)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.building)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.unit)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.floor)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.room)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.host)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.userid)
                .IsUnicode(false);
            




            modelBuilder.Entity<hsf_residential>()
                .Property(e => e.Id)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_residential>()
                .Property(e => e.chinaname)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_owner>()
                .Property(e => e.residential)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_residential>()
                .Property(e => e.createuser)
                .IsUnicode(false);

            modelBuilder.Entity<hsf_residential>()
                .Property(e => e.modifiyuser)
                .IsUnicode(false);
        }
    }
}
