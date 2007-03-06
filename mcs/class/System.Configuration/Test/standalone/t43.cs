using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;

class T1
{
	static void Main(string[] args)
	{
		try
		{
			NameValueCollection AppSettings = ConfigurationManager.AppSettings;

			for (int i = 0; i < AppSettings.Count; ++i) {
				Console.WriteLine ("AppSettings[{0}] = {1}", i, AppSettings[i]);
			}
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
