using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

class T30
{
	static void Main(string[] args)
	{
		try
		{
			CompilationSection section = (CompilationSection)ConfigurationManager.GetSection ("system.web/compilation");

			section = (CompilationSection)ConfigurationManager.GetSection ("system.web/compilation");

			Console.WriteLine ("there are {0} assemblies listed in the section", section.Assemblies.Count);
		}
		catch (Exception e)
		{
			// Error.
			Console.WriteLine(e.ToString());
		}
	}
}
