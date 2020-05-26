using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotServer.DataCenter
{
    public class SmartDataManager
    {
        /// <summary>
        /// key是用户id
        /// List 这个用户的全部消息
        /// </summary>
        private static Dictionary<string, List<SmartModel>> Dictionary = new Dictionary<string, List<SmartModel>>();

        public static void Add(string userId, SmartModel model)
        {
            if (Dictionary.ContainsKey(userId))
            {
                Dictionary[userId].Add(model);
            }
            else
            {
                Dictionary[userId] = new List<SmartModel>() { model };
            }
        }
        public static void Remove(string userId, string modelId)
        {
            if (Dictionary.ContainsKey(userId))
            {
                Dictionary[userId] = Dictionary[userId].Where(m => m.Id != modelId).ToList();
            }
        }

        public static void SendLogin(string userId, Action<SmartModel> action)
        {
            if (Dictionary.ContainsKey(userId))
            {
                foreach (var item in Dictionary[userId])
                {
                    action.Invoke(item);
                    item.State = 1;
                }
            }
        }
    }
}
