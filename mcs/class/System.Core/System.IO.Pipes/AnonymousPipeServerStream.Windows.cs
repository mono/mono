using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.IO.Pipes
{
	public sealed partial class AnonymousPipeServerStream
	{
		public AnonymousPipeServerStream (PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
		{
			if (direction == PipeDirection.InOut) {
				throw new NotSupportedException(SR.NotSupported_AnonymousPipeUnidirectional);
			}
			if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
				throw new ArgumentOutOfRangeException(nameof(inheritability), SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable);
			}

			Create(direction, inheritability, bufferSize, pipeSecurity);
		}

		// Creates the anonymous pipe.
		private void Create (PipeDirection direction, HandleInheritability inheritability, int bufferSize, PipeSecurity pipeSecurity)
		{
			Debug.Assert (direction != PipeDirection.InOut, "Anonymous pipe direction shouldn't be InOut");
			Debug.Assert (bufferSize >= 0, "bufferSize is negative");

			bool bSuccess;
			SafePipeHandle serverHandle;
			SafePipeHandle newServerHandle;

			// Create the two pipe handles that make up the anonymous pipe.
			var pinningHandle = new GCHandle();
			try {
				Interop.Kernel32.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs (inheritability, pipeSecurity, ref pinningHandle);

				if (direction == PipeDirection.In) {
					bSuccess = Interop.Kernel32.CreatePipe (out serverHandle, out _clientHandle, ref secAttrs, bufferSize);
				} else {
					bSuccess = Interop.Kernel32.CreatePipe (out _clientHandle, out serverHandle, ref secAttrs, bufferSize);
				}
			}
			finally
			{
				if (pinningHandle.IsAllocated) {
					pinningHandle.Free();
				}
			}

			if (!bSuccess) {
				throw Win32Marshal.GetExceptionForLastWin32Error();
			}

			// Duplicate the server handle to make it not inheritable.  Note: We need to do this so that the child 
			// process doesn't end up getting another copy of the server handle.  If it were to get a copy, the
			// OS wouldn't be able to inform the child that the server has closed its handle because it will see
			// that there is still one server handle that is open.  
			bSuccess = Interop.Kernel32.DuplicateHandle (Interop.Kernel32.GetCurrentProcess (), serverHandle, Interop.Kernel32.GetCurrentProcess (),
				out newServerHandle, 0, false, Interop.Kernel32.HandleOptions.DUPLICATE_SAME_ACCESS);

			if (!bSuccess) {
				throw Win32Marshal.GetExceptionForLastWin32Error();
			}

			// Close the inheritable server handle.
			serverHandle.Dispose ();

			InitializeHandle (newServerHandle, false, false);

			State = PipeState.Connected;
		}
	}
}
