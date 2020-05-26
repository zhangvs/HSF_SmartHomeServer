using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.DataCenter
{
    /// <summary>
    /// 一条消息的记录
    /// </summary>
    public class SmartModel
    {
        /// <summary>
        /// 每条分配个唯一Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 来源编号
        /// </summary>
        public string FromId { get; set; }
        /// <summary>
        /// 目标编号
        /// </summary>
        public string ToId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 消息时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 消息状态  0未发送 1已发送待确认  2确认收到
        /// </summary>
        public int State { get; set; }
    }
}
