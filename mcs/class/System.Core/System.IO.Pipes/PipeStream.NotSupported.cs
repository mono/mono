using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Pipes
{
	public abstract partial class PipeStream : Stream
	{
		internal const bool CheckOperationsRequiresSetHandle = false;

		// Blocks until the other end of the pipe has read in all written buffer.
		public void WaitForPipeDrain()
		{
			throw new PlatformNotSupportedException();
		}

		// Gets the transmission mode for the pipe.  This is virtual so that subclassing types can 
		// override this in cases where only one mode is legal (such as anonymous pipes)
		public virtual PipeTransmissionMode TransmissionMode
		{
			get { throw new PlatformNotSupportedException(); }
		}

		// Gets the buffer size in the inbound direction for the pipe. This checks if pipe has read
		// access. If that passes, call to GetNamedPipeInfo will succeed.
		public virtual int InBufferSize
		{
			get { throw new PlatformNotSupportedException(); }
		}

		// Gets the buffer size in the outbound direction for the pipe. This uses cached version 
		// if it's an outbound only pipe because GetNamedPipeInfo requires read access to the pipe.
		// However, returning cached is good fallback, especially if user specified a value in 
		// the ctor.
		public virtual int OutBufferSize
		{
			get { throw new PlatformNotSupportedException(); }
		}

		public virtual PipeTransmissionMode ReadMode
		{
			get { throw new PlatformNotSupportedException(); }
			set { throw new PlatformNotSupportedException(); }
		}

		/// <summary>Initializes the handle to be used asynchronously.</summary>
		/// <param name="handle">The handle.</param>
		private void InitializeAsyncHandle(SafePipeHandle handle)
		{
			throw new PlatformNotSupportedException();
		}

		internal virtual void DisposeCore(bool disposing)
		{
			// It's incorrect to throw PNSE here because the finalizer will invoke DisposeCore.
			// The finalizer can be hit if someone attempts to construct a PipeStream
			//  because the failed constructor invocation still creates an instance and registers
			//  it for finalization.
		}

		private unsafe int ReadCore(Span<byte> buffer)
		{
			throw new PlatformNotSupportedException();
		}

		private unsafe void WriteCore(ReadOnlySpan<byte> buffer)
		{
			throw new PlatformNotSupportedException();
		}

		private Task<int> ReadAsyncCore(Memory<byte> destination, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException();
		}

		private Task WriteAsyncCore(ReadOnlyMemory<byte> source, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException();
		}

		/// <summary>Throws an exception if the supplied handle does not represent a valid pipe.</summary>
		/// <param name="safePipeHandle">The handle to validate.</param>
		internal void ValidateHandleIsPipe(SafePipeHandle safePipeHandle)
		{
			throw new PlatformNotSupportedException();
		}

		internal static string GetPipePath(string serverName, string pipeName)
		{
			throw new PlatformNotSupportedException();
		}		
	}
}