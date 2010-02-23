//
// System.Web.Caching.CachedVaryBy
//
// Authors:
//  Jackson Harper (jackson@ximian.com)
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2003-2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Web.Util;

namespace System.Web.Caching
{
#if NET_4_0
	[Serializable]
#endif
	sealed class CachedVaryBy
	{
		string[] prms;
		string[] headers;
		string custom;
		string key;
		List <string> item_list;
		bool wildCardParams;
		
		internal CachedVaryBy (HttpCachePolicy policy, string key)
		{
			prms = policy.VaryByParams.GetParamNames ();
			headers = policy.VaryByHeaders.GetHeaderNames (policy.OmitVaryStar);
			custom = policy.GetVaryByCustom ();
			this.key = key;
			item_list = new List <string> ();
			wildCardParams = policy.VaryByParams ["*"];
		}

		internal List <string> ItemList {
			get { return item_list; }
		}

		internal string Key {
			get { return key; }
		}
		
		internal string CreateKey (string file_path, HttpContext context)
		{
			if (String.IsNullOrEmpty (file_path))
				throw new ArgumentNullException ("file_path");

			StringBuilder builder = new StringBuilder ("vbk"); // VaryBy Key
			HttpRequest request = context != null ? context.Request : null;
			string name, value;
			
			builder.Append (file_path);
			if (request == null)
				return builder.ToString ();
			
			builder.Append (request.HttpMethod);
			
			if (wildCardParams) {
				builder.Append ("WQ"); // Wildcard, Query
				foreach (string p in request.QueryString) {
					if (p == null)
						continue;
					
					builder.Append ('N'); // Name
					builder.Append (p.ToLowerInvariant ());
					value = request.QueryString [p];
					if (String.IsNullOrEmpty (value))
						continue;
					
					builder.Append ('V'); // Value
					builder.Append (value);
				}

				builder.Append ('F'); // Form
				foreach (string p in request.Form) {
					if (p == null)
						continue;
					
					builder.Append ('N'); // Name
					builder.Append (p.ToLowerInvariant ());

					value = request.Form [p];
					if (String.IsNullOrEmpty (value))
						continue;
					
					builder.Append ('V'); // Value
					builder.Append (value);
				}
			} else if (prms != null) {
				StringBuilder fprms = null;
				builder.Append ("SQ"); // Specified, Query
				
				for (int i = 0; i < prms.Length; i++) {
					name = prms [i];
					if (String.IsNullOrEmpty (name))
						continue;

					value = request.QueryString [name];
					if (value != null) {
						builder.Append ('N'); // Name
						builder.Append (name.ToLowerInvariant ());

						if (value.Length > 0) {
							builder.Append ('V'); // Value
							builder.Append (value);
						}
					}

					value = request.Form [name];
					if (value != null) {
						if (fprms == null)
							fprms = new StringBuilder ('F'); // Form
						
						builder.Append ('N'); // Name
						builder.Append (name.ToLowerInvariant ());
						if (value.Length > 0) {
							builder.Append ('V'); // Value
							builder.Append (value);
						}
					}
				}
				if (fprms != null)
					builder.Append (fprms.ToString ());
			}
			
			if (headers != null) {
				builder.Append ('H'); // Headers
				
				for (int i=0; i < headers.Length; i++) {
					builder.Append ('N'); // Name

					name = headers [i];
					builder.Append (name.ToLowerInvariant ());

					value = request.Headers [name];
					if (String.IsNullOrEmpty (value))
						continue;
					
					builder.Append ('V'); // Value
					builder.Append (value);
				}
			}

			if (custom != null) {
				builder.Append ('C'); // Custom
				string s = context.ApplicationInstance.GetVaryByCustomString (context, custom);
				builder.Append ('N'); // Name
				builder.Append (custom);
				builder.Append ('V'); // Value
				builder.Append (s != null ? s : "__null__");
			}
			
			return builder.ToString ();
		}
	}
}

