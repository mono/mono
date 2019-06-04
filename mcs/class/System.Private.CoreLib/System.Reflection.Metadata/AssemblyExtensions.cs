namespace System.Reflection.Metadata
{
	public static class AssemblyExtensions
	{
		[CLSCompliant(false)]
		public static unsafe bool TryGetRawMetadata(this Assembly assembly, out byte* blob, out int length) => throw new NotImplementedException ();
	}
}