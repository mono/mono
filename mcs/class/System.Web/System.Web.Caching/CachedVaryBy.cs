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
using System.Globalization;
using System.Text;
using System.Collections;
using System.Web.Util;

namespace System.Web.Caching {

	internal sealed class CachedVaryBy {

		string[] prms;
		string[] headers;
		string custom;
		string key;
		ArrayList item_list;
		bool wildCardParams;
		
		internal CachedVaryBy (HttpCachePolicy policy, string key)
		{
			prms = policy.VaryByParams.GetParamNames ();
			headers = policy.VaryByHeaders.GetHeaderNames (policy.OmitVaryStar);
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
			string newLine = Environment.NewLine;
			
			builder.Append ("CachedRawResponse" + newLine);
			builder.Append (file_path);
			builder.Append (newLine);
			builder.Append ("METHOD:" + request.HttpMethod);
			builder.Append (newLine);

			if (wildCardParams) {
				foreach (string p in request.QueryString) {
					// FIXME: QueryString might contain a null key if a page gets called like this: page.aspx?arg (w/out the "=")
					if (p == null) continue;
					builder.Append ("VPQ:");
					builder.Append (p.ToLower (Helpers.InvariantCulture));
					builder.Append ('=');
					builder.Append (request.QueryString [p]);
					builder.Append (newLine);
				}
				foreach (string p in request.Form) {
					// FIXME: can this be null, too?
					if (p == null) continue;
					builder.Append ("VPF:");
					builder.Append (p.ToLower (Helpers.InvariantCulture));
					builder.Append ('=');
					builder.Append (request.Form [p]);
					builder.Append (newLine);
				}
			} else if (prms != null) {
				for (int i=0; i<prms.Length; i++) {
					if (request.QueryString [prms [i]] != null) {
						builder.Append ("VPQ:");
						builder.Append (prms [i].ToLower (Helpers.InvariantCulture));
						builder.Append ('=');
						builder.Append (request.QueryString [prms [i]]);
						builder.Append (newLine);
					}
					if (request.Form [prms [i]] != null) {
						builder.Append ("VPF:");
						builder.Append (prms [i].ToLower (Helpers.InvariantCulture));
						builder.Append ('=');
						builder.Append (request.Form [prms [i]]);
						builder.Append (newLine);
					}
				}
			}
			
			if (headers != null) {
				for (int i=0; i<headers.Length; i++) {
					builder.Append ("VH:");
					builder.Append (headers [i].ToLower (Helpers.InvariantCulture));
					builder.Append ('=');
					builder.Append (request.Headers [headers [i]]);
					builder.Append (newLine);
				}
			}

			if (custom != null) {
				string s = app.GetVaryByCustomString (context, custom);
				builder.Append ("VC:");
				builder.Append (custom);
				builder.Append ('=');
				builder.Append (s != null ? s : "__null__");
				builder.Append (newLine);
			}

			return builder.ToString ();
		}
	}
}

