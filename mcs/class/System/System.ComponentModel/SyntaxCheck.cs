//
// System.ComponentModel.SyntaxCheck
//
// Author:
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System.IO;

namespace System.ComponentModel
{
	public class SyntaxCheck
	{
		private SyntaxCheck ()
		{
		}

		[MonoTODO]
		public static bool CheckMachineName (string value)
		{
			if (value == null || value.Trim () == "")
				return false;

			return Environment.MachineName.Equals (value);
		}

		[MonoTODO]
		public static bool CheckPath (string value)
		{
			if (value == null || value.Trim () == "")
				return false;

			try {
				Path.GetFullPath (value);
			} catch {
				return false;
			}
			return true;
		}

		[MonoTODO]
		public static bool CheckRootedPath (string value)
		{
			if (value == null || value.Trim () == "")
				return false;

			return Path.IsPathRooted (value);
		}
	}
}

