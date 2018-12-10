using System;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO
{
	public partial class Path
    {
        internal const int MAX_PATH = Interop.Kernel32.MAX_PATH;
		internal static readonly char[] PathSeparatorChars = new char[] { PathInternal.DirectorySeparatorChar, PathInternal.AltDirectorySeparatorChar, PathInternal.VolumeSeparatorChar };
		internal static readonly string DirectorySeparatorStr = PathInternal.DirectorySeparatorChar.ToString ();

		//Used by System.IO.File
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
#if WIN_PLATFORM
			if (Environment.IsRunningOnWindows) {
				int idx = path.IndexOf (':');
				if (idx >= 0 && idx != 1)
					throw new ArgumentException (parameterName);
			}
#endif
		}

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

		// insecure - do not call directly
		internal static string InsecureGetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (path.Trim ().Length == 0) {
				string msg = Locale.GetText ("The specified path is not of a legal form (empty).");
				throw new ArgumentException (msg);
			}
#if WIN_PLATFORM
			// adjust for drives, i.e. a special case for windows
			if (Environment.IsRunningOnWindows)
				path = WindowsDriveAdjustment (path);
#endif
			// if the supplied path ends with a separator...
			char end = path [path.Length - 1];

			var canonicalize = true;
			if (path.Length >= 2 &&
				PathInternal.IsDirectorySeparator (path [0]) &&
				PathInternal.IsDirectorySeparator (path [1])) {
				if (path.Length == 2 || path.IndexOf (path [0], 2) < 0)
					throw new ArgumentException ("UNC paths should be of the form \\\\server\\share.");

				if (path [0] != DirectorySeparatorChar)
					path = path.Replace (AltDirectorySeparatorChar, DirectorySeparatorChar);

			} else {
				if (!IsPathRooted (path)) {
					
					// avoid calling expensive CanonicalizePath when possible
					if (!Environment.IsRunningOnWindows) {
						var start = 0;
						while ((start = path.IndexOf ('.', start)) != -1) {
							if (++start == path.Length || path [start] == DirectorySeparatorChar || path [start] == AltDirectorySeparatorChar)
								break;
						}
						canonicalize = start > 0;
					}

					var cwd = Directory.InsecureGetCurrentDirectory();
					if (cwd [cwd.Length - 1] == DirectorySeparatorChar)
						path = cwd + path;
					else
						path = cwd + DirectorySeparatorChar + path;					
				} else if (DirectorySeparatorChar == '\\' &&
					path.Length >= 2 &&
					PathInternal.IsDirectorySeparator (path [0]) &&
					!PathInternal.IsDirectorySeparator (path [1])) { // like `\abc\def'
					string current = Directory.InsecureGetCurrentDirectory();
					if (current [1] == VolumeSeparatorChar)
						path = current.Substring (0, 2) + path;
					else
						path = current.Substring (0, current.IndexOf ('\\', current.IndexOfUnchecked ("\\\\", 0, current.Length) + 1));
				}
			}
			
			if (canonicalize)
			    path = CanonicalizePath (path);

			// if the original ended with a [Alt]DirectorySeparatorChar then ensure the full path also ends with one
			if (PathInternal.IsDirectorySeparator (end) && (path [path.Length - 1] != DirectorySeparatorChar))
				path += DirectorySeparatorChar;

			return path;
		}

		internal static String GetFullPathInternal(String path)
		{
			return InsecureGetFullPath (path);
		}

#if WIN_PLATFORM
		// http://msdn.microsoft.com/en-us/library/windows/desktop/aa364963%28v=vs.85%29.aspx
		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern int GetFullPathName(string path, int numBufferChars, StringBuilder buffer, ref IntPtr lpFilePartOrNull); 

		internal static string GetFullPathName(string path)
		{
			const int MAX_PATH = 260;
			StringBuilder buffer = new StringBuilder(MAX_PATH);
			IntPtr ptr = IntPtr.Zero;
			int length = GetFullPathName(path, MAX_PATH, buffer, ref ptr);
			if (length == 0)
			{
				int error = Marshal.GetLastWin32Error();
				throw new IOException("Windows API call to GetFullPathName failed, Windows error code: " + error);
			}
			else if (length > MAX_PATH)
			{
				buffer = new StringBuilder(length);
				GetFullPathName(path, length, buffer, ref ptr);
			}

			return buffer.ToString();
		}

		internal static string WindowsDriveAdjustment (string path)
		{

			// three special cases to consider when a drive is specified
			if (path.Length < 2) {
				if (path.Length == 1 && (path[0] == '\\' || path[0] == '/'))
					return Path.GetPathRoot(Directory.GetCurrentDirectory());
				return path;
			}
			if ((path [1] != ':') || !Char.IsLetter (path [0]))
				return path;

			string current = Directory.InsecureGetCurrentDirectory ();
			// first, only the drive is specified
			if (path.Length == 2) {
				// then if the current directory is on the same drive
				if (current [0] == path [0])
					path = current; // we return it
				else
					path = GetFullPathName(path); // we have to use the GetFullPathName Windows API
			} else if ((path [2] != Path.DirectorySeparatorChar) && (path [2] != Path.AltDirectorySeparatorChar)) {
				// second, the drive + a directory is specified *without* a separator between them (e.g. C:dir).
				// If the current directory is on the specified drive...
				if (current [0] == path [0]) {
					// then specified directory is appended to the current drive directory
					path = Path.Combine (current, path.Substring (2, path.Length - 2));
				} else {
					// we have to use the GetFullPathName Windows API
					path = GetFullPathName(path);
				}
			}
			return path;
		}
#endif

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
				root.Length > 2 && PathInternal.IsDirectorySeparator (root[0]) && PathInternal.IsDirectorySeparator (root[1]);

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
#if WIN_PLATFORM
					// append leading '\' of the UNC path that was lost in STEP 3.
					if (isUnc)
						ret = Path.DirectorySeparatorStr + ret;

					if (!PathInternal.AreRootsEqual (root, ret, StringComparison.Ordinal))
						ret = root + ret;

					if (isUnc) {
						return ret;
					} else if (!PathInternal.IsDirectorySeparator (path[0]) && PathInternal.AreRootsEqual (root, path, StringComparison.Ordinal)) {
						if (ret.Length <= 2 && !ret.EndsWith (DirectorySeparatorStr)) // '\' after "c:"
							ret += Path.DirectorySeparatorChar;
						return ret;
					} else {
						string current = Directory.GetCurrentDirectory ();
						if (current.Length > 1 && current[1] == Path.VolumeSeparatorChar) {
							// DOS local file path
							if (ret.Length == 0 || PathInternal.IsDirectorySeparator (ret[0]))
								ret += '\\';
							return current.Substring (0, 2) + ret;
						} else if (PathInternal.IsDirectorySeparator (current[current.Length - 1]) && PathInternal.IsDirectorySeparator (ret[0]))
							return current + ret.Substring (1);
						else
							return current + ret;
					}
#endif
				} else {
					if (root != "" && ret.Length > 0 && ret [0] != '/')
						ret = root + ret;
				}
				return ret;
			}
		}
	}
}