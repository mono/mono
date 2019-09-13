using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	internal static class NetworkStreamHelpers
	{
		internal static ValueTask<int> ReadAsync (this NetworkStream stream, Memory<byte> buffer)
		{
			return stream.ReadAsync (buffer, default);
		}

		internal static ValueTask WriteAsync (this NetworkStream stream, ReadOnlyMemory<byte> buffer)
		{
			return stream.WriteAsync (buffer, default);
		}
	}
}
