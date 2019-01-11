namespace System.IO
{
	internal static partial class FileSystem
	{
		public static void CopyFile(string sourceFullPath, string destFullPath, bool overwrite) => throw new PlatformNotSupportedException ();
		public static void ReplaceFile(string sourceFullPath, string destFullPath, string destBackupFullPath, bool ignoreMetadataErrors) => throw new PlatformNotSupportedException ();
		public static void MoveFile(string sourceFullPath, string destFullPath) => throw new PlatformNotSupportedException ();
		public static void DeleteFile(string fullPath) => throw new PlatformNotSupportedException ();
		public static void CreateDirectory(string fullPath) => throw new PlatformNotSupportedException ();
		public static void MoveDirectory(string sourceFullPath, string destFullPath) => throw new PlatformNotSupportedException ();
		public static void RemoveDirectory(string fullPath, bool recursive) => throw new PlatformNotSupportedException ();
		public static bool DirectoryExists(string fullPath)  => throw new PlatformNotSupportedException ();
		public static bool DirectoryExists(ReadOnlySpan<char> fullPath)  => throw new PlatformNotSupportedException ();
		public static bool FileExists(string fullPath) => throw new PlatformNotSupportedException ();
		public static bool FileExists(ReadOnlySpan<char> fullPath) => throw new PlatformNotSupportedException ();
		public static FileAttributes GetAttributes(string fullPath) => throw new PlatformNotSupportedException ();
		public static void SetAttributes(string fullPath, FileAttributes attributes) => throw new PlatformNotSupportedException ();
		public static DateTimeOffset GetCreationTime(string fullPath) => throw new PlatformNotSupportedException ();
		public static void SetCreationTime(string fullPath, DateTimeOffset time, bool asDirectory) => throw new PlatformNotSupportedException ();
		public static DateTimeOffset GetLastAccessTime(string fullPath) => throw new PlatformNotSupportedException ();
		public static void SetLastAccessTime(string fullPath, DateTimeOffset time, bool asDirectory) => throw new PlatformNotSupportedException ();
		public static DateTimeOffset GetLastWriteTime(string fullPath) => throw new PlatformNotSupportedException ();
		public static void SetLastWriteTime(string fullPath, DateTimeOffset time, bool asDirectory) => throw new PlatformNotSupportedException ();
		public static string[] GetLogicalDrives() => throw new PlatformNotSupportedException ();
	}

}
