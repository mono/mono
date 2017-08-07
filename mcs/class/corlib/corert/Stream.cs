namespace System.IO
{
	partial class Stream
	{
		public virtual int Read (Span<byte> destination)
		{
			throw new NotImplementedException ();
		}

		public virtual void Write(ReadOnlySpan<byte> source)
		{
			throw new NotImplementedException ();
		}
	}
}