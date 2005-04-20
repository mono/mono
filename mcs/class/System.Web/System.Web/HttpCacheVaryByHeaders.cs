// 
// System.Web.HttpCacheVaryByHeaders
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
using System.Collections;

namespace System.Web {
   public sealed class HttpCacheVaryByHeaders {
      private Hashtable _Items;
      private bool _Wildcard;

      // TODO: We need internal methods here to communicate with CachePolicy

      internal HttpCacheVaryByHeaders() {
      }

      public void VaryByUnspecifiedParameters() {
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
