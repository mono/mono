// 
// System.Web.HttpCacheVaryByParams
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;

namespace System.Web {
   public sealed class HttpCacheVaryByParams {
      private Hashtable _Items;
      private bool _IgnoreParams;
      private bool _Wildcard;
      private bool _Dirty;

      // TODO: We need internal methods here to communicate with CachePolicy

      internal HttpCacheVaryByParams() {
      }

      public bool IgnoreParams {
         get {
            return _IgnoreParams;
         }

         set {
            if (_Wildcard || null != _Items) {
               return;
            }

            _Dirty = true;
            _IgnoreParams = value;
         }
      }
      
      public bool this[string header] {
         get {
            if (null == header) {
               throw new ArgumentNullException("header");
            }

            if (header == "*") {
               return _Wildcard;
            }

            if (null != _Items) {
               return _Items.ContainsKey(header);
            }

            return false;
         }

         set {
            if (null == header) {
               throw new ArgumentNullException("header");
            }

            if (!(value)) {
               return;
            }

            _Dirty = true;

            if (header == "*") {
               _Wildcard = true;
               _Items = null;
               return;
            }

            if (!_Wildcard) {
               if (null == _Items) {
                  _Items = new Hashtable();
               }

               _Items[header] = true;
            }
         }
      }
   }
}
