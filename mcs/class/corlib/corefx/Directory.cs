using System;
using System.Security.AccessControl;

namespace System.IO
{
	public partial class Directory
	{
		public static DirectoryInfo CreateDirectory (string path, System.Security.AccessControl.DirectorySecurity directorySecurity)
			=> CreateDirectory (path); //ignore directorySecurity

		public static DirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
		{
			return new DirectorySecurity (path, includeSections);
		}

		public static DirectorySecurity GetAccessControl (string path)
		{
			// AccessControlSections.Audit requires special permissions.
			return GetAccessControl (path,
						 AccessControlSections.Owner |
						 AccessControlSections.Group |
						 AccessControlSections.Access);
		}

		public static void SetAccessControl(string path, DirectorySecurity directorySecurity)
		{
			if (directorySecurity == null)
				throw new ArgumentNullException(nameof(directorySecurity));

			String fullPath = Path.GetFullPath(path);
			directorySecurity.PersistModifications(fullPath);
		}

		// Used by System.Environment
		internal static string InsecureGetCurrentDirectory()
		{
			MonoIOError error;
			string result = MonoIO.GetCurrentDirectory(out error);
			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException(error);
			return result;
		}

		internal static void InsecureSetCurrentDirectory(string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Trim ().Length == 0)
				throw new ArgumentException ("path string must not be an empty string or whitespace string");
			MonoIOError error;
			if (!Exists (path))
				throw new DirectoryNotFoundException ("Directory \"" +
									path + "\" not found.");
			MonoIO.SetCurrentDirectory (path, out error);
			if (error != MonoIOError.ERROR_SUCCESS)
				throw MonoIO.GetException (path, error);
		}
   }
}
