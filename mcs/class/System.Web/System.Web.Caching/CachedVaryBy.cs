//
// System.Web.Caching.CachedVaryBy
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Caching {

	internal class CachedVaryBy {

		private string[] prms;
		private string[] headers;
		private string custom;
		private string key;
		private ArrayList item_list;
		private bool wildCardParams;
		
		internal CachedVaryBy (HttpCachePolicy policy, string key)
		{
			prms = policy.VaryByParams.GetParamNames ();
			headers = policy.VaryByHeaders.GetHeaderNames ();
			custom = policy.GetVaryByCustom ();
			this.key = key;
			item_list = new ArrayList ();
			wildCardParams = policy.VaryByParams ["*"];
		}

		internal ArrayList ItemList {
			get { return item_list; }
		}

		internal string Key {
			get { return key; }
		}
		
		internal string CreateKey (string file_path, HttpContext context)
		{
			StringBuilder builder = new StringBuilder ();
			HttpApplication app = context.ApplicationInstance;
			HttpRequest request = context.Request;

			builder.Append ("CachedRawResponse\n");
			builder.Append (file_path);
			builder.Append ('\n');
			builder.Append ("METHOD:" + request.HttpMethod);
			builder.Append ('\n');

			if (prms != null) {
				for (int i=0; i<prms.Length; i++) {
					if (request.Params [prms [i]] == null)
						continue;
					builder.Append ("VP:");
					builder.Append (prms [i]);
					builder.Append ('=');
					builder.Append (request.Params [prms [i]]);
					builder.Append ('\n');
				}
			} else if (wildCardParams) {
				foreach (string p in request.Params) {
					builder.Append ("VP:");
					builder.Append (p);
					builder.Append ('=');
					builder.Append (request.Params [p]);
					builder.Append ('\n');
				}
			}
			
			if (headers != null) {
				for (int i=0; i<headers.Length; i++) {
					builder.Append ("VH:");
					builder.Append (headers [i]);
					builder.Append ('=');
					builder.Append (request.Headers [headers [i]]);
					builder.Append ('\n');
				}
			}

			if (custom != null) {
				string s = app.GetVaryByCustomString (context, custom);
				builder.Append ("VC:");
				builder.Append (custom);
				builder.Append ('=');
				builder.Append (s != null ? s : "__null__");
				builder.Append ('\n');
			}

			return builder.ToString ();
		}
	}
}

