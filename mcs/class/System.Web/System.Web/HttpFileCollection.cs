// 
// System.Web.HttpFileCollection
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
