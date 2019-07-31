using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MonoTests.Helpers {

	/// <summary>
	/// Represents a temporary directory.  Creating an instance creates a directory at the specified path,
	/// and disposing the instance deletes the directory.
	/// </summary>
	public sealed class TempDirectory : IDisposable
	{
		/// <summary>Gets the created directory's path.</summary>
		public string Path { get; private set; }

		public TempDirectory ()
			: this (CreateTemporaryDirectory ())
		{
		}

		public TempDirectory (string path)
		{
			Path = path;
		}

		~TempDirectory ()
		{
			Dispose ();
		}

		public void Dispose()
		{
			GC.SuppressFinalize (this);
			DeleteDirectory (Path);
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
		static string CreateTemporaryDirectory ()
		{
			var name = string.Empty;
			var calling_method = new StackFrame (2).GetMethod ();
			if (calling_method != null)
				name = calling_method.DeclaringType.FullName + "_" + calling_method.Name + "_";

			var rv = global::System.IO.Path.Combine (global::System.IO.Path.GetTempPath (), name + Guid.NewGuid ().ToString ());
			Directory.CreateDirectory (rv);
			return rv;
		}
	}
}
