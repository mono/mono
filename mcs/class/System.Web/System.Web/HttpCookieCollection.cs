//
// System.Web.HttpCookieCollection.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Collections.Specialized;

namespace System.Web {
   public sealed class HttpCookieCollection : NameObjectCollectionBase {
      public HttpCookieCollection() {}
      public string[] AllKeys {
         get {
            return this.BaseGetAllKeys();
         }
      }
      public HttpCookie this[int index] {
         get {
            return (HttpCookie)(this.BaseGet(index));
         }
      }
      public HttpCookie this[string name] {
         get {
            return (HttpCookie)(this.BaseGet(name));
         }
      }
      public void Add(HttpCookie cookie) {
         this.BaseAdd(cookie.Name, cookie);
      }
      public void Clear() {
         this.BaseClear();
      }
      public void CopyTo(Array dest, int index) {
         int i;
         HttpCookie cookie;
         for(i=0; i<this.Count; i++) {
            cookie=this[i];
            dest.SetValue(new HttpCookie(cookie.Name, cookie.Value), index+i);
         }
      }
      public string GetKey(int index) {
         return this.BaseGetKey(index);
      }
      public void Remove(string name) {
         this.BaseRemove(name);
      }
      public void Set(HttpCookie cookie) {
         this.BaseSet(cookie.Name, cookie);
      }
   }
}
