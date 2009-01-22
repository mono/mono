using System;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace GHTTests
{
	public class UrlTestUtils
	{
		public static string FixUrlForDirectoriesTest (string url)
		{
			if (url == null)
				return null;

			return url.Replace (HttpContext.Current.Request.ApplicationPath, "root");
		}
	}
}
