// 
// System.Web.HttpCacheVaryByHeaders
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;

namespace System.Web {
   public sealed class HttpCacheVaryByHeaders {
      private Hashtable _Items;
      private bool _Dirty;
      private bool _Wildcard;

      // TODO: We need internal methods here to communicate with CachePolicy

      internal HttpCacheVaryByHeaders() {
      }

      public void VaryByUnspecifiedParameters() {
         _Dirty = true;
         _Wildcard = true;
         _Items = null;
      }

      public bool AcceptTypes {
         get {
            return this["Accept"];
         }

         set {
            this["Accept"] = value;
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
               VaryByUnspecifiedParameters();
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

      public bool UserAgent {
         get {
            return this["User-Agent"];
         }

         set {
            this["User-Agent"] = value;
         }
      }

      public bool UserCharSet {
         get {
            return this["Accept-Charset"];
         }

         set {
            this["Accept-Charset"] = value;
         }
      }   

      public bool UserLanguage {
         get {
            return this["Accept-Language"];
         }

         set {
            this["Accept-Language"] = value;
         }
      }   

	internal string [] GetHeaderNames ()
	   {
		   if (_Items == null)
			   return null;

		   string[] headers = new string [_Items.Count];
		   int i = 0;
		   foreach (string header in _Items.Keys)
			   headers [i++] = header;
		   
		   return headers;
	   }
   }
}
