//
// System.Web.Caching.CachedVaryBy
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
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
		
		internal CachedVaryBy (HttpCachePolicy policy, string key)
		{
			prms = policy.VaryByParams.GetParamNames ();
			headers = policy.VaryByHeaders.GetHeaderNames ();
			custom = policy.GetVaryByCustom ();
			this.key = key;
			item_list = new ArrayList ();
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

