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
//                 Sebastien Pouliot  <sebastien@ximian.com>
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace System.IO {

	[ComVisible (true)]
	public static class Path {

		[Obsolete ("see GetInvalidPathChars and GetInvalidFileNameChars methods.")]
		public static readonly char[] InvalidPathChars;
		public static readonly char AltDirectorySeparatorChar;
		public static readonly char DirectorySeparatorChar;
		public static readonly char PathSeparator;
		internal static readonly string DirectorySeparatorStr;
		public static readonly char VolumeSeparatorChar;

		internal static readonly char[] PathSeparatorChars;
		private static readonly bool dirEqualsVolume;

		// class methods
		public static string ChangeExtension (string path, string extension)
		{
			if (path == null)
				return null;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

			int iExt = findExtension (path);

			if (extension == null)
				return iExt < 0 ? path : path.Substring (0, iExt);
			else if (extension.Length == 0)
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

			if (path1.Length == 0)
				return path2;

			if (path2.Length == 0)
				return path1;

			if (path1.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

			if (path2.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

			//TODO???: UNC names
			if (IsPathRooted (path2))
				return path2;
			
			char p1end = path1 [path1.Length - 1];
			if (p1end != DirectorySeparatorChar && p1end != AltDirectorySeparatorChar && p1end != VolumeSeparatorChar)
				return path1 + DirectorySeparatorStr + path2;

			return path1 + path2;
		}
	
		//
		// This routine:
		//   * Removes duplicat path separators from a string
		//   * If the string starts with \\, preserves the first two (hostname on Windows)
		//   * Removes the trailing path separator.
		//   * Returns the DirectorySeparatorChar for the single input DirectorySeparatorChar or AltDirectorySeparatorChar
		//
		// Unlike CanonicalizePath, this does not do any path resolution
		// (which GetDirectoryName is not supposed to do).
		//
		internal static string CleanPath (string s)
		{
			int l = s.Length;
			int sub = 0;
			int start = 0;

			// Host prefix?
			char s0 = s [0];
			if (l > 2 && s0 == '\\' && s [1] == '\\'){
				start = 2;
			}

			// We are only left with root
			if (l == 1 && (s0 == DirectorySeparatorChar || s0 == AltDirectorySeparatorChar))
				return s;

			// Cleanup
			for (int i = start; i < l; i++){
				char c = s [i];
				
				if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar)
					continue;
				if (i+1 == l)
					sub++;
				else {
					c = s [i + 1];
					if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar)
						sub++;
				}
			}

			if (sub == 0)
				return s;

			char [] copy = new char [l-sub];
			if (start != 0){
				copy [0] = '\\';
				copy [1] = '\\';
			}
			for (int i = start, j = start; i < l && j < copy.Length; i++){
				char c = s [i];

				if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar){
					copy [j++] = c;
					continue;
				}

				// For non-trailing cases.
				if (j+1 != copy.Length){
					copy [j++] = DirectorySeparatorChar;
					for (;i < l-1; i++){
						c = s [i+1];
						if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar)
							break;
					}
				}
			}
			return new String (copy);
		}

		public static string GetDirectoryName (string path)
		{
			// LAMESPEC: For empty string MS docs say both
			// return null AND throw exception.  Seems .NET throws.
			if (path == String.Empty)
				throw new ArgumentException("Invalid path");

			if (path == null || GetPathRoot (path) == path)
				return null;

			if (path.Trim ().Length == 0)
				throw new ArgumentException ("Argument string consists of whitespace characters only.");

			if (path.IndexOfAny (System.IO.Path.InvalidPathChars) > -1)
				throw new ArgumentException ("Path contains invalid characters");

			int nLast = path.LastIndexOfAny (PathSeparatorChars);
			if (nLast == 0)
				nLast++;

			if (nLast > 0) {
				string ret = path.Substring (0, nLast);
				int l = ret.Length;

				if (l >= 2 && DirectorySeparatorChar == '\\' && ret [l - 1] == VolumeSeparatorChar)
					return ret + DirectorySeparatorChar;
				else {
					//
					// Important: do not use CanonicalizePath here, use
					// the custom CleanPath here, as this should not
					// return absolute paths
					//
					return CleanPath (ret);
				}
			}

			return String.Empty;
		}

		public static string GetExtension (string path)
		{
			if (path == null)
				return null;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

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
			if (path == null || path.Length == 0)
				return path;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

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
			string fullpath = InsecureGetFullPath (path);

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

#if !NET_2_1
			if (SecurityManager.SecurityEnabled) {
				new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fullpath).Demand ();
			}
#endif
			return fullpath;
		}

		internal static string WindowsDriveAdjustment (string path)
		{
			// two special cases to consider when a drive is specified
			if (path.Length < 2)
				return path;
			if ((path [1] != ':') || !Char.IsLetter (path [0]))
				return path;

			string current = Directory.GetCurrentDirectory ();
			// first, only the drive is specified
			if (path.Length == 2) {
				// then if the current directory is on the same drive
				if (current [0] == path [0])
					path = current; // we return it
				else
					path += '\\';
			} else if ((path [2] != Path.DirectorySeparatorChar) && (path [2] != Path.AltDirectorySeparatorChar)) {
				// second, the drive + a directory is specified *without* a separator between them (e.g. C:dir).
				// If the current directory is on the specified drive...
				if (current [0] == path [0]) {
					// then specified directory is appended to the current drive directory
					path = Path.Combine (current, path.Substring (2, path.Length - 2));
				} else {
					// if not, then just pretend there was a separator (Path.Combine won't work in this case)
					path = String.Concat (path.Substring (0, 2), DirectorySeparatorStr, path.Substring (2, path.Length - 2));
				}
			}
			return path;
		}

		// insecure - do not call directly
		internal static string InsecureGetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Trim ().Length == 0) {
				string msg = Locale.GetText ("The specified path is not of a legal form (empty).");
				throw new ArgumentException (msg);
			}

			// adjust for drives, i.e. a special case for windows
			if (Environment.IsRunningOnWindows)
				path = WindowsDriveAdjustment (path);

			// if the supplied path ends with a separator...
			char end = path [path.Length - 1];

			var canonicalize = true;
			if (path.Length >= 2 &&
				IsDsc (path [0]) &&
				IsDsc (path [1])) {
				if (path.Length == 2 || path.IndexOf (path [0], 2) < 0)
					throw new ArgumentException ("UNC paths should be of the form \\\\server\\share.");

				if (path [0] != DirectorySeparatorChar)
					path = path.Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);

			} else {
				if (!IsPathRooted (path)) {
					
					// avoid calling expensive CanonicalizePath when possible
					var start = 0;
					while ((start = path.IndexOf ('.', start)) != -1) {
						if (++start == path.Length || path [start] == DirectorySeparatorChar || path [start] == AltDirectorySeparatorChar)
							break;
					}
					canonicalize = start > 0;
					
					path = Directory.GetCurrentDirectory () + DirectorySeparatorStr + path;
				} else if (DirectorySeparatorChar == '\\' &&
					path.Length >= 2 &&
					IsDsc (path [0]) &&
					!IsDsc (path [1])) { // like `\abc\def'
					string current = Directory.GetCurrentDirectory ();
					if (current [1] == VolumeSeparatorChar)
						path = current.Substring (0, 2) + path;
					else
						path = current.Substring (0, current.IndexOf ('\\', current.IndexOf ("\\\\") + 1));
				}
			}
			
			if (canonicalize)
			    path = CanonicalizePath (path);

			// if the original ended with a [Alt]DirectorySeparatorChar then ensure the full path also ends with one
			if (IsDsc (end) && (path [path.Length - 1] != DirectorySeparatorChar))
				path += DirectorySeparatorChar;

			return path;
		}

		static bool IsDsc (char c) {
			return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
		}
		
		public static string GetPathRoot (string path)
		{
			if (path == null)
				return null;

			if (path.Trim ().Length == 0)
				throw new ArgumentException ("The specified path is not of a legal form.");

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
					if (len < path.Length) {
						len++;
						while (len < path.Length && !IsDsc (path [len])) len++;
					}

					return DirectorySeparatorStr +
						DirectorySeparatorStr +
						path.Substring (2, len - 2).Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);
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

		// FIXME: Further limit the assertion when imperative Assert is implemented
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
		public static string GetTempFileName ()
		{
			FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = Path.Combine (GetTempPath(), "tmp" + num.ToString("x") + ".tmp");

				try {
					f = new FileStream (path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read,
							    8192, false, (FileOptions) 1);
				}
				catch (SecurityException) {
					// avoid an endless loop
					throw;
				}
				catch (UnauthorizedAccessException) {
					// This can happen if we don't have write permission to /tmp
					throw;
				}
				catch (DirectoryNotFoundException) {
					// This happens when TMPDIR does not exist
					throw;
				}
				catch {
				}
			} while (f == null);
			
			f.Close();
			return path;
		}

		[EnvironmentPermission (SecurityAction.Demand, Unrestricted = true)]
		public static string GetTempPath ()
		{
			SecurityManager.EnsureElevatedPermissions (); // this is a no-op outside moonlight

			string p = get_temp_path ();
			if (p.Length > 0 && p [p.Length - 1] != DirectorySeparatorChar)
				return p + DirectorySeparatorChar;

			return p;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern string get_temp_path ();

		public static bool HasExtension (string path)
		{
			if (path == null || path.Trim ().Length == 0)
				return false;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

			int pos = findExtension (path);
			return 0 <= pos && pos < path.Length - 1;
		}

		public static bool IsPathRooted (string path)
		{
			if (path == null || path.Length == 0)
				return false;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path.");

			char c = path [0];
			return (c == DirectorySeparatorChar 	||
				c == AltDirectorySeparatorChar 	||
				(!dirEqualsVolume && path.Length > 1 && path [1] == VolumeSeparatorChar));
		}

		public static char[] GetInvalidFileNameChars ()
		{
			// return a new array as we do not want anyone to be able to change the values
			if (Environment.IsRunningOnWindows) {
				return new char [41] { '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
					'\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F', '\x10', '\x11', '\x12', 
					'\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', 
					'\x1E', '\x1F', '\x22', '\x3C', '\x3E', '\x7C', ':', '*', '?', '\\', '/' };
			} else {
				return new char [2] { '\x00', '/' };
			}
		}

		public static char[] GetInvalidPathChars ()
		{
			// return a new array as we do not want anyone to be able to change the values
			if (Environment.IsRunningOnWindows) {
				return new char [36] { '\x22', '\x3C', '\x3E', '\x7C', '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
					'\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F', '\x10', '\x11', '\x12', 
					'\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', 
					'\x1E', '\x1F' };
			} else {
				return new char [1] { '\x00' };
			}
		}

		public static string GetRandomFileName ()
		{
			// returns a 8.3 filename (total size 12)
			StringBuilder sb = new StringBuilder (12);
			// using strong crypto but without creating the file
			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			byte [] buffer = new byte [11];
			rng.GetBytes (buffer);

			for (int i = 0; i < buffer.Length; i++) {
				if (sb.Length == 8)
					sb.Append ('.');

				// restrict to length of range [a..z0..9]
				int b = (buffer [i] % 36);
				char c = (char) (b < 26 ? (b + 'a') : (b - 26 + '0'));
				sb.Append (c);
			}

			return sb.ToString ();
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

		static Path ()
		{
			VolumeSeparatorChar = MonoIO.VolumeSeparatorChar;
			DirectorySeparatorChar = MonoIO.DirectorySeparatorChar;
			AltDirectorySeparatorChar = MonoIO.AltDirectorySeparatorChar;

			PathSeparator = MonoIO.PathSeparator;
			// this copy will be modifiable ("by design")
			InvalidPathChars = GetInvalidPathChars ();
			// internal fields

			DirectorySeparatorStr = DirectorySeparatorChar.ToString ();
			PathSeparatorChars = new char [] {
				DirectorySeparatorChar,
				AltDirectorySeparatorChar,
				VolumeSeparatorChar
			};

			dirEqualsVolume = (DirectorySeparatorChar == VolumeSeparatorChar);
		}

		// returns the server and share part of a UNC. Assumes "path" is a UNC.
		static string GetServerAndShare (string path)
		{
			int len = 2;
			while (len < path.Length && !IsDsc (path [len])) len++;

			if (len < path.Length) {
				len++;
				while (len < path.Length && !IsDsc (path [len])) len++;
			}

			return path.Substring (2, len - 2).Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);
		}

		// assumes Environment.IsRunningOnWindows == true
		static bool SameRoot (string root, string path)
		{
			// compare root - if enough details are available
			if ((root.Length < 2) || (path.Length < 2))
				return false;

			// UNC handling
			if (IsDsc (root[0]) && IsDsc (root[1])) {
				if (!(IsDsc (path[0]) && IsDsc (path[1])))
					return false;

				string rootShare = GetServerAndShare (root);
				string pathShare = GetServerAndShare (path);

				return String.Compare (rootShare, pathShare, true, CultureInfo.InvariantCulture) == 0;
			}
			
			// same volume/drive
			if (!root [0].Equals (path [0]))
				return false;
			// presence of the separator
			if (path[1] != Path.VolumeSeparatorChar)
				return false;
			if ((root.Length > 2) && (path.Length > 2)) {
				// but don't directory compare the directory separator
				return (IsDsc (root[2]) && IsDsc (path[2]));
			}
			return true;
		}

		static string CanonicalizePath (string path)
		{
			// STEP 1: Check for empty string
			if (path == null)
				return path;
			if (Environment.IsRunningOnWindows)
				path = path.Trim ();

			if (path.Length == 0)
				return path;

			// STEP 2: Check to see if this is only a root
			string root = Path.GetPathRoot (path);
			// it will return '\' for path '\', while it should return 'c:\' or so.
			// Note: commenting this out makes the need for the (target == 1...) check in step 5
			//if (root == path) return path;

			// STEP 3: split the directories, this gets rid of consecutative "/"'s
			string[] dirs = path.Split (Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			// STEP 4: Get rid of directories containing . and ..
			int target = 0;

			bool isUnc = Environment.IsRunningOnWindows &&
				root.Length > 2 && IsDsc (root[0]) && IsDsc (root[1]);

			// Set an overwrite limit for UNC paths since '\' + server + share
			// must not be eliminated by the '..' elimination algorithm.
			int limit = isUnc ? 3 : 0;

			for (int i = 0; i < dirs.Length; i++) {
				// WIN32 path components must be trimmed
				if (Environment.IsRunningOnWindows)
					dirs[i] = dirs[i].TrimEnd ();
				
				if (dirs[i] == "." || (i != 0 && dirs[i].Length == 0))
					continue;
				else if (dirs[i] == "..") {
					// don't overwrite path segments below the limit
					if (target > limit)
						target--;
				} else
					dirs[target++] = dirs[i];
			}

			// STEP 5: Combine everything.
			if (target == 0 || (target == 1 && dirs[0] == ""))
				return root;
			else {
				string ret = String.Join (DirectorySeparatorStr, dirs, 0, target);
				if (Environment.IsRunningOnWindows) {
					// append leading '\' of the UNC path that was lost in STEP 3.
					if (isUnc)
						ret = Path.DirectorySeparatorStr + ret;

					if (!SameRoot (root, ret))
						ret = root + ret;

					if (isUnc) {
						return ret;
					} else if (!IsDsc (path[0]) && SameRoot (root, path)) {
						if (ret.Length <= 2 && !ret.EndsWith (DirectorySeparatorStr)) // '\' after "c:"
							ret += Path.DirectorySeparatorChar;
						return ret;
					} else {
						string current = Directory.GetCurrentDirectory ();
						if (current.Length > 1 && current[1] == Path.VolumeSeparatorChar) {
							// DOS local file path
							if (ret.Length == 0 || IsDsc (ret[0]))
								ret += '\\';
							return current.Substring (0, 2) + ret;
						} else if (IsDsc (current[current.Length - 1]) && IsDsc (ret[0]))
							return current + ret.Substring (1);
						else
							return current + ret;
					}
				}
				return ret;
			}
		}

		// required for FileIOPermission (and most proibably reusable elsewhere too)
		// both path MUST be "full paths"
		static internal bool IsPathSubsetOf (string subset, string path)
		{
			if (subset.Length > path.Length)
				return false;

			// check that everything up to the last separator match
			int slast = subset.LastIndexOfAny (PathSeparatorChars);
			if (String.Compare (subset, 0, path, 0, slast) != 0)
				return false;

			slast++;
			// then check if the last segment is identical
			int plast = path.IndexOfAny (PathSeparatorChars, slast);
			if (plast >= slast) {
				return String.Compare (subset, slast, path, slast, path.Length - plast) == 0;
			}
			if (subset.Length != path.Length)
				return false;

			return String.Compare (subset, slast, path, slast, subset.Length - slast) == 0;
		}

#if NET_4_0 || MOONLIGHT
		public static string Combine (params string [] paths)
		{
			if (paths == null)
				throw new ArgumentNullException ("paths");

			bool need_sep;
			var ret = new StringBuilder ();
			int pathsLen = paths.Length;
			int slen;
			foreach (var s in paths) {
				need_sep = false;
				if (s == null)
					throw new ArgumentNullException ("One of the paths contains a null value", "paths");
				if (s.IndexOfAny (InvalidPathChars) != -1)
					throw new ArgumentException ("Illegal characters in path.");
				
				pathsLen--;
				if (IsPathRooted (s))
					ret.Length = 0;
				
				ret.Append (s);
				slen = s.Length;
				if (slen > 0 && pathsLen > 0) {
					char p1end = s [slen - 1];
					if (p1end != DirectorySeparatorChar && p1end != AltDirectorySeparatorChar && p1end != VolumeSeparatorChar)
						need_sep = true;
				}
				
				if (need_sep)
					ret.Append (DirectorySeparatorStr);
			}

			return ret.ToString ();
		}

		public static string Combine (string path1, string path2, string path3)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");

			if (path2 == null)
				throw new ArgumentNullException ("path2");

			if (path3 == null)
				throw new ArgumentNullException ("path3");
			
			return Combine (new string [] { path1, path2, path3 });
		}

		public static string Combine (string path1, string path2, string path3, string path4)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");

			if (path2 == null)
				throw new ArgumentNullException ("path2");

			if (path3 == null)
				throw new ArgumentNullException ("path3");

			if (path4 == null)
				throw new ArgumentNullException ("path4");
			
			return Combine (new string [] { path1, path2, path3, path4 });
		}
#endif

		internal static void Validate (string path)
		{
			Validate (path, "path");
		}

		internal static void Validate (string path, string parameterName)
		{
			if (path == null)
				throw new ArgumentNullException (parameterName);
			if (String.IsNullOrWhiteSpace (path))
				throw new ArgumentException (Locale.GetText ("Path is empty"));
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException (Locale.GetText ("Path contains invalid chars"));
#if MOONLIGHT
			// On Moonlight (SL4+) there are some limitations in "Elevated Trust"
			if (SecurityManager.HasElevatedPermissions) {
			}
#endif
		}
	}
}
