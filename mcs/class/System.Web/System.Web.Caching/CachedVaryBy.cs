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

namespace System.Web.Caching {

	internal class CachedVaryBy {

		private string[] prms;
		private string[] headers;
		private string custom;

		internal CachedVaryBy (HttpCachePolicy policy)
		{
			prms = policy.VaryByParams.GetParamNames ();
		}
		
		internal string CreateKey (string file_path, HttpRequest request)
		{
			StringBuilder builder = new StringBuilder ();

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

			return builder.ToString ();
		}
	}	 
}

