using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using _Configuration = System.Configuration.Configuration;

class T31
{
	static void Main(string[] args)
	{
		try
		{
			_Configuration cfg = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.None);

			CompilationSection section = (CompilationSection)cfg.GetSection ("system.web/compilation");

			section = (CompilationSection)cfg.GetSection ("system.web/compilation");

			Console.WriteLine ("there are {0} assemblies listed in the section", section.Assemblies.Count);
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
