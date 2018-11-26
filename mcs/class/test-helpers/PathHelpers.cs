using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoTests.Helpers {

	public static class PathHelpers
	{
		static string root;
		static uint last_number;

		static PathHelpers ()
		{
			// Create a root temporary directory that is to be the base of all other temporary directories we create.
			// Contains the assembly name to make the name somewhat intelligible,
			// and make it include the PID so that parallel execution won't stomp on eachother.
			root = Path.Combine (Path.GetTempPath (), Assembly.GetExecutingAssembly ().GetName ().Name + "_" + Process.GetCurrentProcess ().Id);
			// Clean up the directory if it already exists (which would be quite rare, because it would only happen if we happen to run with the same pid at a later moment)
			DeleteDirectory (root);
			Directory.CreateDirectory (root);

			// Try to clean up after us when the process exits.
			AppDomain.CurrentDomain.ProcessExit += Cleanup;
		}

		static void Cleanup (object sender, EventArgs args)
		{
			DeleteDirectory (root);
		}

		[DllImport ("libc", SetLastError = true)]
		static extern int mkdir (string path, ushort mode);

		[DllImport ("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool CreateDirectory (string path, IntPtr securityAttributes);

		static bool RunningOnWindows ()
		{
			int i = (int) Environment.OSVersion.Platform;
			return ((i != 4) && (i != 128));
		}

		// Tries to recursively delete the specified path.
		// Doesn't throw exceptions if path is null, empty, or doesn't exist.
		public static void DeleteDirectory (string path)
		{
			if (string.IsNullOrEmpty (path))
				return;

			if (Directory.Exists (path))
				Directory.Delete (path, true);
		}

		// Creates a unique temporary directory.
		//
		// The calling method's type/method name will be a part of the
		// returned path to make it somewhat nicer/more useful.
		//
		// This method is only meant for testing code, not production code (it
		// will leave some temporary directories behind).
		public static string CreateTemporaryDirectory ()
		{
			string name;
			var calling_method = new StackFrame (1).GetMethod ();
			if (calling_method != null) {
				name = calling_method.DeclaringType.FullName + "_" + calling_method.Name;
			} else {
				name = "unknown-test";
			}

			// We store the last number used to create a unique directory, to
			// avoid looping over the same numbers over and over again.
			//
			// The last number is stored in a static variable: which means
			// this is not thread-safe. We don't care: any number works fine,
			// even random garbage or overflowed/underflowed numbers.
			//
			// Note: directories will not be numbered sequentially (without
			// holes), since dir_root change change.
			var rv = Path.Combine (root, name + "_" + last_number.ToString ());
			for (var i = last_number; i < 10000 + last_number; i++) {
				// There's no way to know if Directory.CreateDirectory
				// created the directory or not (which would happen if the directory
				// already existed). Checking if the directory exists before
				// creating it would result in a race condition if multiple
				// threads create temporary directories at the same time.
				bool createResult;

				if (RunningOnWindows ()) {
					createResult = CreateDirectory (rv, IntPtr.Zero);
				} else {
					createResult = mkdir (rv, 511 /*Convert.ToUInt16 ("777", 8)*/) == 0;
				}
				if (createResult) {
					last_number = i;
					return rv;
				}

				rv = Path.Combine (root, name + "_" + i.ToString ());
			}

			// If we looped through 10000 potential candidates and couldn't
			// find a single unique name, something else probably went wrong.
			throw new Exception ("Failed to create temporary directory!");
		}
	}
}
