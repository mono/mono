//
// System.Web.Util.PathUtil
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace System.Web.Util
{
	internal class PathUtil
	{
		static string appbase;
		static char [] separators;

		static PathUtil ()
		{
			// This hack is a workaround until AppDomainVirtualPath works... Gotta investigate it.
			Assembly entry = Assembly.GetEntryAssembly ();
			appbase = Path.GetDirectoryName (entry.Location);
			//

			if (Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar)
				separators = new char [] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
			else
				separators = new char [] {Path.DirectorySeparatorChar};
		}
		
		static string MakeAbsolute (string abspath)
		{
			string [] parts = abspath.Split (separators);
			ArrayList valid = new ArrayList ();

			int len = parts.Length;
			bool hasDots = false;
			for (int i = 0; i < len; i++) {
				if (parts [i] == ".") {
					hasDots = true;
					continue;
				}

				if (parts [i] == "..") {
					hasDots = true;
					if (valid.Count > 0)
						valid.RemoveAt (valid.Count - 1);
					continue;
				}

				valid.Add (parts [i]);
			}

			if (!hasDots)
				return abspath;

			parts = (String []) valid.ToArray (typeof (string));
			string result = String.Join (new String (Path.DirectorySeparatorChar, 1), parts);
			if (!Path.IsPathRooted (result))
				return Path.DirectorySeparatorChar + result;

			return result;
		}

		static public string Combine (string basepath, string relative)
		{
			if (relative == null || relative.Length == 0)
				throw new ArgumentException ("empty or null", "relative");

			char first = relative [0];
			if (first == '/' || first == '\\' || Path.IsPathRooted (relative))
				throw new ArgumentException ("'relative' is rooted", "relative");

			if (first == '~' && relative.Length > 1 && Array.IndexOf (separators, relative [1]) != -1)
				return Path.Combine (appbase, relative.Substring (2));

			if (basepath == null)
				basepath = appbase;

			return MakeAbsolute (Path.Combine (basepath, relative));
		}
	}
}

