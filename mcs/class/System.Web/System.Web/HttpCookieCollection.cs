//
// System.Web.HttpCookieCollection.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public sealed class HttpCookieCollection : NameObjectCollectionBase
        {
                public HttpCookieCollection();
                public string[] AllKeys
                {
                        get
                        {
                                return this.BaseGetAllKeys();
                        }
                }
                public HttpCookie this[int index]
                {
                        get
                        {
                                return this.BaseGet(index);
                        }
                }
                public HttpCookie this[string name]
                {
                        get
                        {
                                return this.BaseGet(name);
                        }
                }
                public void Add(HttpCookie cookie)
                {
                        this.BaseAdd(cookie.name, cookie);
                }
                public void Clear;
                {
                        this.BaseClear;
                }
                public void CopyTo(Array dest, int index)
                {
                        for(i=0; i<this.Count; i++)
                        {
                                dest[index+i]=this[i];
                        }
                }
                public string GetKey(int index)
                {
                        return this.BaseGetKey(index);
                }
                public void Remove(string name)
                {
                        this.BaseRemove(name);
                }
                public void Set(HttpCookie cookie)
                {
                        this.BaseSet(cookie.name, cookie);
                }
        }
}
