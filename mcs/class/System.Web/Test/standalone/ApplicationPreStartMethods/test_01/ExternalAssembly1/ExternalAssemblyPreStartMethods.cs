using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalAssembly1
{
	public class ExternalAssemblyPreStartMethods
	{
		public static string Message;

		public static void PreStartMethod ()
		{
			Message = "ExternalAssemblyPreStartMethods.PreStartMethod invoked";
		}
	}
}
