using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hsf.Model
{
    public class SoundBodyRequest
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 每次会话id,唯一，由客户端生成
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 音箱id,唯一，由客户端生成
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 201 ,      //每次请求动作id，int类型
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 登录注册成功时，收到的返回
        /// </summary>
        public string token { get; set; }
        /// <summary>
        /// 音箱请求百度的文本
        /// </summary>
        public string questions { get; set; }
        /// <summary>
        /// 音箱数据源id
        /// </summary>
        public string sourceId { get; set; }
        /// <summary>
        /// 百度返回的处理结果 
        /// </summary>
        public object req { get; set; }
    }


    public class SoundBodyResult
    {
        /// <summary>
        /// //每次会话id,唯一，由客户端生成，字符串
        /// </summary>
        public string sessionId { get; set; }
        /// <summary>
        /// 音箱id,唯一，由客户端生成
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 音箱请求百度的文本
        /// </summary>
        public string questions { get; set; }
        /// <summary>
        ///音箱需要执行的动作id，
        ///2010为执行百度流程，
        ///2011播放url内容，播放完自动唤醒
        ///2012播放url内容，播放完结束本次会话
        ///2015指示灯特效
        ///2016语音合成req内容播放
        ///2020播放响应效果音可持续交流，
        ///2021播放响应效果音结束本次会话
        ///2025无法识别
        /// </summary>
        public string actionId { get; set; }
        /// <summary>
        /// 播放媒体地址，如果有值则读取，没有就继续原有流程
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 播放完url媒体资源后，是否自动唤醒
        /// </summary>
        public string blwakeup { get; set; }
        /// <summary>
        /// //服务器返回消息其他参数
        /// </summary>
        public string req { get; set; }


    }
}
