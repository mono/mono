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
		}
		catch (ConfigurationErrorsException e) {
			Console.WriteLine ("configuration exception thrown.");
			return;
		}
		Console.WriteLine ("configuration exception not thrown.");
	}
}
