//
// System.Runtime.InteropServices/RuntimeEnvironment.cs
//
// Authors:
// 	Dominik Fretz (roboto@gmx.net)
//
// (C) 2003 Dominik Fretz
//

using System;
using System.IO;
using System.Reflection;

namespace System.Runtime.InteropServices
{
	public class RuntimeEnvironment
	{
		public RuntimeEnvironment ()
		{
		}

		[MonoTODO]
		public static string SystemConfigurationFile 
		{
			get { return String.Empty; }
		}

		
		[MonoTODO]
		public static bool FromGlobalAccessCache (Assembly a)
		{
			throw new NotImplementedException ();
		}

	
		public static string GetRuntimeDirectory ()
		{
			return Path.GetDirectoryName (typeof (int).Assembly.Location);	
		}

		[MonoTODO]
		public static string GetSystemVersion ()
		{
			//TODO We give back the .Net (1.1) version nummber. Probabbly Environment.Version should also return this.

			// We probably want to return the mono runtime version here -Gonzalo
			return "v1.1.4322";
		}
		
	}
}

