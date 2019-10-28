using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	internal static class NetworkStreamHelpers
	{
		/*
		 * These overloads are in the API reference, but our System.dll internals are visible
		 * to the tests.  This causes the compiler to incorrectly resolve these against the
		 * overloaded version.
		 *
		 * The API reference has this in corlib:
		 *
		 *    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
		 *    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
		 *
		 * In System.dll, we override this as
		 *
		 *    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
		 *
		 * Without System.dll internals being visible, the compiler would correctly resolve the call using the
		 * API reference, but we need this custom extension for the tests.
		 */
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
