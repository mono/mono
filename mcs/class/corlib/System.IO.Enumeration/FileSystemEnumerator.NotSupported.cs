using System.Collections;

namespace System.IO.Enumeration
{
	partial class FileSystemEnumerator<TResult>
	{
		public FileSystemEnumerator(string directory, EnumerationOptions options = null) { }
		bool IEnumerator.MoveNext() => throw new PlatformNotSupportedException ();
		private string _currentPath = null;
		private ICollection _pending = null;
		private TResult _current = default;
#pragma warning disable 414
		private bool _lastEntryFound = false;
		private int _entry = 0;
#pragma warning restore 414

		private void CloseDirectoryHandle() => throw new PlatformNotSupportedException ();
		private void DequeueNextDirectory() => throw new PlatformNotSupportedException ();
		private unsafe void FindNextEntry() => throw new PlatformNotSupportedException ();
		private void InternalDispose(bool disposing) { }

	}
}
