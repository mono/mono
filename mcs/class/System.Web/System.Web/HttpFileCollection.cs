// 
// System.Web.HttpFileCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections.Specialized;

namespace System.Web {
   public sealed class HttpFileCollection : NameObjectCollectionBase {
      private HttpPostedFile [] _AllFiles;
      private string [] _AllKeys;

      internal HttpFileCollection() : base() {
      }

      internal void AddFile(string name, HttpPostedFile file) {
         _AllFiles = null;
         _AllKeys = null;

         BaseAdd(name, file);
      }

      public void CopyTo(Array Dest, int index) {
         if (null == _AllFiles) {
            _AllFiles = new HttpPostedFile[Count];
            for (int i = 0; i != Count; i++) {
               _AllFiles[i] = Get(i);
            }
         }

         if (null != _AllFiles) {
            _AllFiles.CopyTo(Dest, index);
         }
      }

      public HttpPostedFile Get(string Name) {
         return (HttpPostedFile) BaseGet(Name);
      }

      public HttpPostedFile Get(int index) {
         return (HttpPostedFile) BaseGet(index);
      }

      public string GetKey(int index) {
         return BaseGetKey(index);
      }

      public string [] AllKeys {
         get {
            if (null == _AllKeys) {
               _AllKeys = BaseGetAllKeys();
            }

            return _AllKeys;
         }
      }
      
      public HttpPostedFile this [string name] {
         get {
            return Get(name);
         }
      }

      public HttpPostedFile this [int index] {
         get {
            return Get(index);
         }
      }
   
   }
}
