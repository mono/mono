using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
	partial class Stream
	{
		public virtual int Read (Span<byte> destination)
		{
			throw new NotImplementedException ();
		}

		public virtual void Write (ReadOnlySpan<byte> source)
		{
			throw new NotImplementedException ();
		}

		public virtual ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (destination.TryGetArray (out ArraySegment<byte> array))
				return new ValueTask<int> (ReadAsync (array.Array, array.Offset, array.Count, cancellationToken));

			throw new NotImplementedException ();
		}

		public virtual Task WriteAsync (ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException ();
		}
	}
}