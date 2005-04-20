// 
// System.Web.HttpCacheVaryByParams
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
using System.Text;
using System.Collections;

namespace System.Web {
   public sealed class HttpCacheVaryByParams {
      private Hashtable _Items;
      private bool _IgnoreParams;
      private bool _Wildcard;

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

            _IgnoreParams = value;
         }
      }

      internal HttpResponseHeader GetResponseHeader ()
      {
	      if (_IgnoreParams)
		      throw new Exception ("Can not get VaryByParams Header when params are ignored.");

	      if (_Wildcard)
		      return new HttpResponseHeader ("Vary", "*");

              if (_Items == null)
                      return null;

	      StringBuilder builder = new StringBuilder ();
	      foreach (string item in _Items.Keys) {
		      if (!(bool) _Items [item])
			      continue;
		      builder.Append (item + ";");
	      }

	      return new HttpResponseHeader ("Vary", builder.ToString ());
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

	   internal string [] GetParamNames ()
	   {
		   if (_Items == null)
			   return null;

		   string[] prms = new string [_Items.Count];
		   int i = 0;
		   foreach (string prm in _Items.Keys)
			   prms [i++] = prm;
		   
		   return prms;
	   }
   }
}
