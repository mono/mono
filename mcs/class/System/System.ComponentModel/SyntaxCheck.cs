//
// System.ComponentModel.SyntaxCheck.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System.IO;

namespace System.ComponentModel
{
	// LAMESPEC should be sealed or event internal?
	public class SyntaxCheck
	{
		private SyntaxCheck ()
		{
		}

		[MonoTODO ("Don't know what MS wants to do with this")]
		public static bool CheckMachineName (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			return Environment.MachineName.Equals (value);
		}

		[MonoTODO ("Don't know what MS wants to do with this")]
		public static bool CheckPath (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			try {
				Path.GetFullPath (value);
			} catch {
				return false;
			}
			return true;
		}

		public static bool CheckRootedPath (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			return Path.IsPathRooted (value);
		}
	}
}

