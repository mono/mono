// 
// System.Web.HttpModuleCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections.Specialized;

namespace System.Web {
   public sealed class HttpModuleCollection : NameObjectCollectionBase {
      private IHttpModule [] _Modules;
      private string [] _Keys;

      internal HttpModuleCollection() : base() {
      }

      internal void AddModule(string key, IHttpModule m) {
         _Modules = null;
         _Keys = null;

         BaseAdd(key, m);
      }

      public void CopyTo(Array dest, int index) {
         if (null == _Modules) {
            _Modules = new IHttpModule[Count];

            for (int i = 0; i != Count; i++) {
               _Modules[i] = Get(i);
            }
         }

         if (null != _Modules) {
            _Modules.CopyTo(dest, index);
         }
      }

      public IHttpModule Get(string key) {
         return (IHttpModule) BaseGet(key);
      }
      
      public IHttpModule Get(int index) {
         return (IHttpModule) BaseGet(index);
      }

      public string GetKey(int index) {
         return BaseGetKey(index);
      }
      
      public string [] AllKeys {
         get {
            if (null == _Keys) {
               _Keys = BaseGetAllKeys();
            }

            return _Keys;
         }
      }   

      public IHttpModule this [string key] {
         get {
            return Get(key);
         }
      }

      public IHttpModule this [int index] {
         get {
            return Get(index);
         }
      }
   }
}
