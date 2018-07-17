using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.IO.Pipes
{
	public sealed partial class NamedPipeServerStream
	{
		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, HandleInheritability.None)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability)
			: this (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, inheritability)
		{
		}

		public NamedPipeServerStream (string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize, PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
			: base (direction, transmissionMode, outBufferSize)
		{
			if (pipeName == null) {
				throw new ArgumentNullException (nameof(pipeName));
			}
			if (pipeName.Length == 0){
				throw new ArgumentException (SR.Argument_NeedNonemptyPipeName);
			}
			if ((options & ~(PipeOptions.WriteThrough | PipeOptions.Asynchronous)) != 0) {
				throw new ArgumentOutOfRangeException (nameof(options), SR.ArgumentOutOfRange_OptionsInvalid);
			}
			if (inBufferSize < 0) {
				throw new ArgumentOutOfRangeException (nameof(inBufferSize), SR.ArgumentOutOfRange_NeedNonNegNum);
			}
			if ((maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) && (maxNumberOfServerInstances != MaxAllowedServerInstances)) {
				throw new ArgumentOutOfRangeException (nameof(maxNumberOfServerInstances), SR.ArgumentOutOfRange_MaxNumServerInstances);
			}
			if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
				throw new ArgumentOutOfRangeException (nameof(inheritability), SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable);
			}
			if ((additionalAccessRights & ~(PipeAccessRights.ChangePermissions | PipeAccessRights.TakeOwnership | PipeAccessRights.AccessSystemSecurity)) != 0) {
				throw new ArgumentOutOfRangeException (nameof(additionalAccessRights), SR.ArgumentOutOfRange_AdditionalAccessLimited);
			}

			Create (pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, pipeSecurity, inheritability, additionalAccessRights);
		}

		private void Create(string pipeName, PipeDirection direction, int maxNumberOfServerInstances,
				PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize,
				PipeSecurity pipeSecurity, HandleInheritability inheritability, PipeAccessRights additionalAccessRights)
		{
			Debug.Assert (pipeName != null && pipeName.Length != 0, "fullPipeName is null or empty");
			Debug.Assert (direction >= PipeDirection.In && direction <= PipeDirection.InOut, "invalid pipe direction");
			Debug.Assert (inBufferSize >= 0, "inBufferSize is negative");
			Debug.Assert (outBufferSize >= 0, "outBufferSize is negative");
			Debug.Assert ((maxNumberOfServerInstances >= 1 && maxNumberOfServerInstances <= 254) || (maxNumberOfServerInstances == MaxAllowedServerInstances), "maxNumberOfServerInstances is invalid");
			Debug.Assert (transmissionMode >= PipeTransmissionMode.Byte && transmissionMode <= PipeTransmissionMode.Message, "transmissionMode is out of range");

			string fullPipeName = Path.GetFullPath (@"\\.\pipe\" + pipeName);

			// Make sure the pipe name isn't one of our reserved names for anonymous pipes.
			if (String.Equals (fullPipeName, @"\\.\pipe\anonymous", StringComparison.OrdinalIgnoreCase)) {
				throw new ArgumentOutOfRangeException(nameof(pipeName), SR.ArgumentOutOfRange_AnonymousReserved);
			}

			if (IsCurrentUserOnly && pipeSecurity == null) {
				using (WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent ()) {
					SecurityIdentifier identifier = currentIdentity.Owner;

					// Grant full control to the owner so multiple servers can be opened.
					// Full control is the default per MSDN docs for CreateNamedPipe.
					PipeAccessRule rule = new PipeAccessRule (identifier, PipeAccessRights.FullControl, AccessControlType.Allow);
					pipeSecurity = new PipeSecurity ();

					pipeSecurity.AddAccessRule (rule);
					pipeSecurity.SetOwner (identifier);
				}

				// PipeOptions.CurrentUserOnly is special since it doesn't match directly to a corresponding Win32 valid flag.
				// Remove it, while keeping others untouched since historically this has been used as a way to pass flags to CreateNamedPipe
				// that were not defined in the enumeration.
				options &= ~PipeOptions.CurrentUserOnly;
			}

			int openMode = ((int)direction) |
						   (maxNumberOfServerInstances == 1 ? Interop.Kernel32.FileOperations.FILE_FLAG_FIRST_PIPE_INSTANCE : 0) |
						   (int)options |
						   (int)additionalAccessRights;

			// We automatically set the ReadMode to match the TransmissionMode.
			int pipeModes = (int)transmissionMode << 2 | (int)transmissionMode << 1;

			// Convert -1 to 255 to match win32 (we asserted that it is between -1 and 254).
			if (maxNumberOfServerInstances == MaxAllowedServerInstances) {
				maxNumberOfServerInstances = 255;
			}

			var pinningHandle = new GCHandle ();
			try
			{
				Interop.Kernel32.SECURITY_ATTRIBUTES secAttrs = PipeStream.GetSecAttrs (inheritability, pipeSecurity, ref pinningHandle);
				SafePipeHandle handle = Interop.Kernel32.CreateNamedPipe (fullPipeName, openMode, pipeModes,
					maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, ref secAttrs);

				if (handle.IsInvalid) {
					throw Win32Marshal.GetExceptionForLastWin32Error ();
				}

				InitializeHandle (handle, false, (options & PipeOptions.Asynchronous) != 0);
			}
			finally
			{
				if (pinningHandle.IsAllocated) {
					pinningHandle.Free ();
				}
			}
		}
	}
}
