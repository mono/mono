// 
// System.Web.HttpCacheVaryByParams
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Text;
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
