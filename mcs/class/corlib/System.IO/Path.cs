//------------------------------------------------------------------------------
// 
// System.IO.Path.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2003 Ben Maurer
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
//                 Gonzalo Paniagua Javier (gonzalo@ximian.com)
//                 Ben Maurer (bmaurer@users.sourceforge.net)
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Runtime.CompilerServices;

namespace System.IO
{
	public sealed class Path
	{
		public static readonly char AltDirectorySeparatorChar;
		public static readonly char DirectorySeparatorChar;
		public static readonly char[] InvalidPathChars;
		public static readonly char PathSeparator;
		internal static readonly string DirectorySeparatorStr;
		public static readonly char VolumeSeparatorChar;

		private static readonly char[] PathSeparatorChars;
		private static readonly bool dirEqualsVolume;

		private Path ()
		{
		}

		// class methods
		public static string ChangeExtension (string path, string extension)
		{
			if (path == null)
				return null;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int iExt = findExtension (path);

			if (extension == null)
				return iExt < 0 ? path : path.Substring (0, iExt);
			else if (extension == String.Empty)
				return iExt < 0 ? path + '.' : path.Substring (0, iExt + 1);

			else if (path.Length != 0) {
				if (extension.Length > 0 && extension [0] != '.')
					extension = "." + extension;
			} else
				extension = String.Empty;
			
			if (iExt < 0) {
				return path + extension;
			} else if (iExt > 0) {
				string temp = path.Substring (0, iExt);
				return temp + extension;
			}

			return extension;
		}

		public static string Combine (string path1, string path2)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");

			if (path2 == null)
				throw new ArgumentNullException ("path2");

			if (path1 == String.Empty)
				return path2;

			if (path2 == String.Empty)
				return path1;

			if (path1.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path1");

			if (path2.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path2");

			//TODO???: UNC names
			// LAMESPEC: MS says that if path1 is not empty and path2 is a full path
			// it should throw ArgumentException
			if (IsPathRooted (path2))
				return path2;
			
			char p1end = path1 [path1.Length - 1];
			if (p1end != DirectorySeparatorChar && p1end != AltDirectorySeparatorChar && p1end != VolumeSeparatorChar)
				return path1 + DirectorySeparatorChar + path2;

			return path1 + path2;
		}

		public static string GetDirectoryName (string path)
		{
			// LAMESPEC: For empty string MS docs say both
			// return null AND throw exception.  Seems .NET throws.
			if (path == String.Empty)
				throw new ArgumentException();

			if (path == null || GetPathRoot (path) == path)
				return null;

			CheckArgument.WhitespaceOnly (path);
			CheckArgument.PathChars (path);

			int nLast = path.LastIndexOfAny (PathSeparatorChars);
			if (nLast == 0)
				nLast++;

			if (nLast > 0) {
				string ret = path.Substring (0, nLast);
				int l = ret.Length;
				if (l >= 2 && ret [l - 1] == VolumeSeparatorChar)
					return ret + DirectorySeparatorChar;
				else
					return ret;
			}

			return String.Empty;
		}

		public static string GetExtension (string path)
		{
			if (path == null)
				return null;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int iExt = findExtension (path);

			if (iExt > -1)
			{
				if (iExt < path.Length - 1)
					return path.Substring (iExt);
			}
			return string.Empty;
		}

		public static string GetFileName (string path)
		{
			if (path == null || path == String.Empty)
				return path;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int nLast = path.LastIndexOfAny (PathSeparatorChars);
			if (nLast >= 0)
				return path.Substring (nLast + 1);

			return path;
		}

		public static string GetFileNameWithoutExtension (string path)
		{
			return ChangeExtension (GetFileName (path), null);
		}

		public static string GetFullPath (string path)
		{
			if (path == null)
				throw (new ArgumentNullException (
					"path",
					"You must specify a path when calling System.IO.Path.GetFullPath"));

			if (path.Trim () == String.Empty)
				throw new ArgumentException ("The path is not of a legal form", "path");

			if (path.Length >= 2 &&
				IsDsc (path [0]) &&
				IsDsc (path [1])) {
				if (path.Length == 2 || path.IndexOf (path [0], 2) < 0)
					throw new ArgumentException ("UNC pass should be of the form \\\\server\\share.");
				else
					if (path [0] == DirectorySeparatorChar)
						return path; // UNC
					else
						return path.Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);
			}

			if (!IsPathRooted (path))
				path = Directory.GetCurrentDirectory () + DirectorySeparatorStr + path;
			else if (DirectorySeparatorChar == '\\' &&
				path.Length >= 2 &&
				IsDsc (path [0]) &&
				!IsDsc (path [1])) { // like `\abc\def'
				string current = Directory.GetCurrentDirectory ();
				if (current [1] == VolumeSeparatorChar)
					path = current.Substring (0, 2) + path;
				else
					path = current.Substring (0, current.IndexOf ('\\', current.IndexOf ("\\\\") + 1));
			}
			return CanonicalizePath (path);
		}

		static bool IsDsc (char c) {
			return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
		}
		
		public static string GetPathRoot (string path)
		{
			if (path == null)
				return null;

			if (path == String.Empty)
				throw new ArgumentException ("This specified path is invalid.");

			if (!IsPathRooted (path))
				return String.Empty;
			
			if (DirectorySeparatorChar == '/') {
				// UNIX
				return IsDsc (path [0]) ? DirectorySeparatorStr : String.Empty;
			} else {
				// Windows
				int len = 2;

				if (path.Length == 1 && IsDsc (path [0]))
					return DirectorySeparatorStr;
				else if (path.Length < 2)
					return String.Empty;

				if (IsDsc (path [0]) && IsDsc (path[1])) {
					// UNC: \\server or \\server\share
					// Get server
					while (len < path.Length && !IsDsc (path [len])) len++;
					// Get share
					while (len < path.Length && !IsDsc (path [len])) len++;
					return DirectorySeparatorStr +
						DirectorySeparatorStr +
						path.Substring (2).Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);
				} else if (IsDsc (path [0])) {
					// path starts with '\' or '/'
					return DirectorySeparatorStr;
				} else if (path[1] == VolumeSeparatorChar) {
					// C:\folder
					if (path.Length >= 3 && (IsDsc (path [2]))) len++;
				} else
					return Directory.GetCurrentDirectory ().Substring (0, 2);// + path.Substring (0, len);
				return path.Substring (0, len);
			}
		}

		public static string GetTempFileName ()
		{
			FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = Path.Combine (GetTempPath(), "tmp" + num.ToString("x"));

				try {
					f = new FileStream (path, FileMode.CreateNew);
				} catch {
				}
			} while (f == null);
			
			f.Close();
			return path;
		}

		/// <summary>
		/// Returns the path of the current systems temp directory
		/// </summary>
		public static string GetTempPath ()
		{
			string p = get_temp_path ();
			if (p.Length > 0 && p [p.Length - 1] != DirectorySeparatorChar)
				return p + DirectorySeparatorChar;

			return p;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern string get_temp_path ();

		public static bool HasExtension (string path)
		{  
			if (path == null || path.Trim () == String.Empty)
				return false;

			int pos = findExtension (path);
			return 0 <= pos && pos < path.Length - 1;
		}

		public static bool IsPathRooted (string path)
		{
			if (path == null || path.Length == 0)
				return false;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			char c = path [0];
			return (c == DirectorySeparatorChar 	||
				c == AltDirectorySeparatorChar 	||
				(!dirEqualsVolume && path.Length > 1 && path [1] == VolumeSeparatorChar));
		}

		// private class methods

		private static int findExtension (string path)
		{
			// method should return the index of the path extension
			// start or -1 if no valid extension
			if (path != null){
				int iLastDot = path.LastIndexOf ('.');
				int iLastSep = path.LastIndexOfAny ( PathSeparatorChars );

				if (iLastDot > iLastSep)
					return iLastDot;
			}
			return -1;
		}

		static Path () {
			VolumeSeparatorChar = MonoIO.VolumeSeparatorChar;
			DirectorySeparatorChar = MonoIO.DirectorySeparatorChar;
			AltDirectorySeparatorChar = MonoIO.AltDirectorySeparatorChar;

			PathSeparator = MonoIO.PathSeparator;
			InvalidPathChars = MonoIO.InvalidPathChars;

			// internal fields

			DirectorySeparatorStr = DirectorySeparatorChar.ToString ();
			PathSeparatorChars = new char [] {
				DirectorySeparatorChar,
				AltDirectorySeparatorChar,
				VolumeSeparatorChar
			};

			dirEqualsVolume = (DirectorySeparatorChar == VolumeSeparatorChar);
		}
		
		
		static string CanonicalizePath (string path) {
			
			// STEP 1: Check for empty string
			if (path == null) return path;
			path = path.Trim ();
			if (path == String.Empty) return path;
			
			// STEP 2: Check to see if this is only a root
			string root = GetPathRoot (path);
			// it will return '\' for path '\', while it should return 'c:\' or so.
			// Note: commenting this out makes the ened for the (target == 1...) check in step 5
			//if (root == path) return path;
				
			// STEP 3: split the directories, this gets rid of consecutative "/"'s
			string [] dirs = path.Split (DirectorySeparatorChar, AltDirectorySeparatorChar);
			// STEP 4: Get rid of directories containing . and ..
			int target = 0;
			
			for (int i = 0; i < dirs.Length; i++) {
				if (dirs [i] == "." || (i != 0 && dirs [i] == String.Empty)) continue;
				else if (dirs [i] == "..") {
					if (target != 0) target--;
				}
				else
					dirs [target++] = dirs [i];
			}

			// STEP 5: Combine everything.
			if (target == 0 || (target == 1 && dirs [0] == ""))
				return root;
			else {
				string ret = String.Join (DirectorySeparatorStr, dirs, 0, target);
				switch (DirectorySeparatorChar) {
				case '\\': // Windows
					// In GetFullPath(), it is assured that here never comes UNC. So this must only applied to such path that starts with '\', without drive specification.
					if (path [0] != DirectorySeparatorChar && path.StartsWith (root)) {
						if (ret.Length <= 2 && !ret.EndsWith (DirectorySeparatorStr)) // '\' after "c:"
							ret += DirectorySeparatorChar;
						return ret;
					} else {
						string current = Directory.GetCurrentDirectory ();
						if (current.Length > 1 && current [1] == VolumeSeparatorChar) {
							// DOS local file path
							if (ret.Length == 0 || IsDsc (ret [0]))
								ret += '\\';
							return current.Substring (0, 2) + ret;
						}
						else if (IsDsc (current [current.Length - 1]) && IsDsc (ret [0]))
							return current + ret.Substring (1);
						else
							return current + ret;
					}
				default: // Unix/Mac
					return ret;
				}
			}
		}
	}
}
