namespace System.IO
{
	partial class FileSystemInfo
	{
		protected FileSystemInfo() { }
		internal void Invalidate() => throw new PlatformNotSupportedException ();
		internal bool ExistsCore => throw new PlatformNotSupportedException ();
		internal long LengthCore => throw new PlatformNotSupportedException ();
		public void Refresh() => throw new PlatformNotSupportedException ();
		internal string NormalizedPath => throw new PlatformNotSupportedException ();
		public FileAttributes Attributes
		{
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		internal DateTimeOffset CreationTimeCore
		{
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		internal DateTimeOffset LastAccessTimeCore
		{
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		internal DateTimeOffset LastWriteTimeCore
		{
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}
	}

}
