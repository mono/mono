// 
// System.Web.HttpModuleCollection
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
