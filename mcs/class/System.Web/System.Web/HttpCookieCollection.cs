// 
// System.Web.HttpCookieCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   (First impl Bob Smith <bob@thestuff.net>)
//

using System;
using System.Web;
using System.Collections.Specialized;

namespace System.Web {
   public sealed class HttpCookieCollection : NameObjectCollectionBase {
      private HttpCookie [] _AllCookies;
      private string [] _AllKeys;
      
      private HttpResponse _Response;

      internal HttpCookieCollection(HttpResponse Response, bool ReadOnly) : base() {
         _Response = Response;
         IsReadOnly = ReadOnly;
      }

      public HttpCookieCollection() {
      }
      
      public string [] AllKeys {
         get {
            if (null == _AllKeys) {
               _AllKeys = BaseGetAllKeys();
            }

            return _AllKeys;
         }
      }
      
      public HttpCookie this[int index] {
         get {
            return Get(index);
         }
      }
      
      public HttpCookie this[string name] {
         get {
            return Get(name);
         }
      }

      public void Add(HttpCookie cookie) {
         if (null != _Response) {
            _Response.GoingToChangeCookieColl();
         }

         // empy performance cache
         _AllCookies = null;
         _AllKeys = null;

         BaseAdd(cookie.Name, cookie);

         if (null != _Response) {
            _Response.OnCookieAdd(cookie);
         }
      }

      public void Clear() {
         _AllCookies = null;
         _AllKeys = null;
         this.BaseClear();
      }

      public void CopyTo(Array dest, int index) {
         if (null == _AllCookies) {
            _AllCookies = new HttpCookie[Count];

            for (int i = 0; i != Count; i++) {
               _AllCookies[i] = Get(i);
            }
         }

         if (null != _AllCookies) {
            _AllCookies.CopyTo(dest, index);
         }
      }

      public HttpCookie Get(int index) {
         return (HttpCookie) BaseGet(index);
      }

      public HttpCookie Get(string name) {
         HttpCookie oRet = (HttpCookie) BaseGet(name);
         if (null == oRet && _Response != null) {
            _AllCookies = null;
            _AllKeys = null;

            _Response.GoingToChangeCookieColl();
            
            oRet = new HttpCookie(name);
            BaseAdd(name, oRet);

            _Response.OnCookieAdd(oRet);
         }

         return oRet;
      }

      public string GetKey(int index) {
         return this.BaseGetKey(index);
      }

      public void Remove(string name) {
         if (null != _Response) {
            _Response.GoingToChangeCookieColl();
         }

         _AllCookies = null;
         _AllKeys = null;
         this.BaseRemove(name);
         
         if (null != _Response) {
            _Response.ChangedCookieColl();
         }
      }

      public void Set(HttpCookie cookie) {
         if (null != _Response) {
            _Response.GoingToChangeCookieColl();
         }

         _AllCookies = null;
         _AllKeys = null;
         this.BaseSet(cookie.Name, cookie);

         if (null != _Response) {
            _Response.ChangedCookieColl();
         }
      }
   }
}
