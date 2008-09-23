

using System;
using System.Text;

namespace MonoHttp
{
	class Utility
	{
	
		#region from System.Uri
		
		internal static bool MaybeUri (string s)
		{
			int p = s.IndexOf (':');
			if (p == -1)
				return false;

			if (p >= 10)
				return false;

			return IsPredefinedScheme (s.Substring (0, p));
		}
		
		private static bool IsPredefinedScheme (string scheme)
		{
			switch (scheme) {
			case "http":
			case "https":
			case "file":
			case "ftp":
			case "nntp":
			case "gopher":
			case "mailto":
			case "news":
#if NET_2_0
			case "net.pipe":
			case "net.tcp":
#endif
				return true;
			default:
				return false;
			}
		}
		
		#endregion
		
		#region from System.Net.Cookiie
		
		internal static string ToClientString (System.Net.Cookie cookie) 
		{
			if (cookie.Name.Length == 0) 
				return String.Empty;

			StringBuilder result = new StringBuilder (64);
	
			if (cookie.Version > 0) 
				result.Append ("Version=").Append (cookie.Version).Append (";");
				
			result.Append (cookie.Name).Append ("=").Append (cookie.Value);

			if (cookie.Path != null && cookie.Path.Length != 0)
				result.Append (";Path=").Append (QuotedString (cookie, cookie.Path));
				
			if (cookie.Domain != null && cookie.Domain.Length != 0)
				result.Append (";Domain=").Append (QuotedString (cookie, cookie.Domain));			
	
			if (cookie.Port != null && cookie.Port.Length != 0)
				result.Append (";Port=").Append (cookie.Port);	
						
			return result.ToString ();
		}

		// See par 3.6 of RFC 2616
  	    	static string QuotedString (System.Net.Cookie cookie, string value)
	    	{
			if (cookie.Version == 0 || IsToken (value))
				return value;
			else 
				return "\"" + value.Replace("\"", "\\\"") + "\"";
	    	}			    	    

	    	static bool IsToken (string value) 
	    	{
			int len = value.Length;
			for (int i = 0; i < len; i++) {
			    	char c = value [i];
				if (c < 0x20 || c >= 0x7f || tspecials.IndexOf (c) != -1)
			      		return false;
			}
			return true;
	    	}
	    	
	    	 static string tspecials = "()<>@,;:\\\"/[]?={} \t";   // from RFC 2965, 2068
	    	
		#endregion
	}
}
