//
// Utilities.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Lluis Sanchez Gual <lluis@novell.com>
//   Michael Hutchinson <mhutchinson@novell.com>
//
// (C) 2005 Marek Sieradzki
// Copyright (c) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Microsoft.Build.BuildEngine {
	public static class Utilities {
	
		static Hashtable charsToEscape;
	
		static Utilities ()
		{
			charsToEscape = new Hashtable ();
			
			charsToEscape.Add ('$', null);
			charsToEscape.Add ('%', null);
			charsToEscape.Add ('\'', null);
			charsToEscape.Add ('(', null);
			charsToEscape.Add (')', null);
			charsToEscape.Add ('*', null);
			charsToEscape.Add (';', null);
			charsToEscape.Add ('?', null);
			charsToEscape.Add ('@', null);
		}
	
		public static string Escape (string unescapedExpression)
		{
			StringBuilder sb = new StringBuilder ();
			
			foreach (char c in unescapedExpression) {
				if (charsToEscape.Contains (c))
					sb.AppendFormat ("%{0:x2}", (int) c);
				else
					sb.Append (c);
			}
			
			return sb.ToString ();
		}
		
		// FIXME: add tests for this
		internal static string Unescape (string escapedExpression)
		{
			StringBuilder sb = new StringBuilder ();
			
			int i = 0;
			while (i < escapedExpression.Length) {
				sb.Append (Uri.HexUnescape (escapedExpression, ref i));
			}
			
			return sb.ToString ();
		}

		internal static string UnescapeFromXml (string text)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < text.Length; i++) {
				char c1 = text[i];
				if (c1 == '&') {
					int end = text.IndexOf (';', i);
					if (end == -1)
						throw new FormatException ("Unterminated XML entity.");
					string entity = text.Substring (i+1, end - i - 1);
					switch (entity) {
					case "lt":
						sb.Append ('<');
						break;
					case "gt":
						sb.Append ('>');
						break;
					case "amp":
						sb.Append ('&');
						break;
					case "apos":
						sb.Append ('\'');
						break;
					case "quot":
						sb.Append ('"');
						break;
					default:
						throw new FormatException ("Unrecogised XML entity '&" + entity + ";'.");
					}
					i = end;
				} else
					sb.Append (c1);
			}
			return sb.ToString ();
		}


		internal static string FromMSBuildPath (string relPath)
		{
			if (relPath == null || relPath.Length == 0)
				return null;

			bool is_windows = Path.DirectorySeparatorChar == '\\';
			string path = relPath;
			if (!is_windows)
				path = path.Replace ("\\", "/");

			// a path with drive letter is invalid/unusable on non-windows
			if (!is_windows && char.IsLetter (path [0]) && path.Length > 1 && path[1] == ':')
				return null;

			if (System.IO.File.Exists (path)){
				return Path.GetFullPath (path);
			}

			if (Path.IsPathRooted (path)) {

				// Windows paths are case-insensitive. When mapping an absolute path
				// we can try to find the correct case for the path.

				string[] names = path.Substring (1).Split ('/');
				string part = "/";

				for (int n=0; n<names.Length; n++) {
					string[] entries;

					if (names [n] == ".."){
						if (part == "/")
							return ""; // Can go further back. It's not an existing file
						part = Path.GetFullPath (part + "/..");
						continue;
					}

					entries = Directory.GetFileSystemEntries (part);

					string fpath = null;
					foreach (string e in entries) {
						if (string.Compare (Path.GetFileName (e), names[n], true) == 0) {
							fpath = e;
							break;
						}
					}
					if (fpath == null) {
						// Part of the path does not exist. Can't do any more checking.
						part = Path.GetFullPath (part);
						for (; n < names.Length; n++)
							part += "/" + names[n];
						return part;
					}

					part = fpath;
				}
				return Path.GetFullPath (part);
			} else {
				return Path.GetFullPath (path);
			}
		}
	}

}

#endif
