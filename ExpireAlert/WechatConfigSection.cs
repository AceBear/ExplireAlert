using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ExpireAlert
{
    public class WechatConfigSection : ConfigurationSection
    {
        public static readonly WechatConfigSection Current =
            (WechatConfigSection)ConfigurationManager.GetSection("wechat");

        [ConfigurationProperty("NotifyTemplateId")]
        public string NotifyTemplateId
        {
            get { return (string)base["NotifyTemplateId"]; }
            set { base["NotifyTemplateId"] = value; }
        }


        [ConfigurationProperty("users", IsDefaultCollection=true)]
        [ConfigurationCollection(typeof(WechatUserCollection),
            AddItemName = "add", RemoveItemName = "remove", ClearItemsName = "clear")]
        public WechatUserCollection Users
        {
            get { return (WechatUserCollection)base["users"]; }
        }
    }

    public class WechatUser : ConfigurationElement
    {
        public WechatUser()
        {
        }

        public WechatUser(string openid)
        {
            this.OpenId = openid;
        }

        [ConfigurationProperty("openid", IsRequired=true, IsKey=true)]
        public string OpenId
        {
            get { return (string)base["openid"]; }
            set { base["openid"] = value; }
        }

        [ConfigurationProperty("remark", IsRequired = false, IsKey = false)]
        public string Remark
        {
            get { return (string)base["remark"]; }
            set { base["remark"] = value; }
        }
    }

    public class WechatUserCollection : ConfigurationElementCollection
    {
        public WechatUserCollection()
        {
        }

        public WechatUser this[int index]
        {
            get
            {
                return (WechatUser)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null) BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(WechatUser user)
        {
            BaseAdd(user);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WechatUser();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WechatUser)element).OpenId;
        }

        public void Remove(WechatUser user)
        {
            BaseRemove(user.OpenId);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}
