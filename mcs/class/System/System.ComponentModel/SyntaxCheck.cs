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

		public static bool CheckMachineName (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			return value.IndexOf ('\\') == -1;
		}

		public static bool CheckPath (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			return value.StartsWith (@"\\");
		}

		public static bool CheckRootedPath (string value)
		{
			if (value == null || value.Trim ().Length == 0)
				return false;

			return Path.IsPathRooted (value);
		}
	}
}

