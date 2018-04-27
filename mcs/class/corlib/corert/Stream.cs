using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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

		public virtual ValueTask<int> ReadAsync (Memory<byte> destination, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (MemoryMarshal.TryGetArray (destination, out ArraySegment<byte> array))
				return new ValueTask<int> (ReadAsync (array.Array, array.Offset, array.Count, cancellationToken));

			throw new NotImplementedException ();
		}

		public virtual ValueTask WriteAsync (ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (MemoryMarshal.TryGetArray (source, out ArraySegment<byte> array))
				return new ValueTask (WriteAsync (array.Array, array.Offset, array.Count, cancellationToken));

			throw new NotImplementedException ();
		}
	}
}