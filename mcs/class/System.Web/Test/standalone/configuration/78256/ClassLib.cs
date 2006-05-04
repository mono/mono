using System;
using System.Configuration;
using System.Web.Configuration;

namespace ClassLib
{
	public class Host:MarshalByRefObject
	{
		public void Run ()
		{
			PagesSection c = (PagesSection) System.Web.Configuration.WebConfigurationManager.GetSection ("system.web/pages");
			if (c == null)
				Console.WriteLine ("null");
			else
				Console.WriteLine (c.StyleSheetTheme);
		}
	}
}

