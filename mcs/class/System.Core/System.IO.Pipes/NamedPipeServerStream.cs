using System.Runtime.InteropServices;

namespace System.IO.Pipes
{
	public sealed partial class NamedPipeServerStream
	{
		private static bool s_isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

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
				throw new ArgumentNullException(nameof(pipeName));
			}
			if (pipeName.Length == 0){
				throw new ArgumentException(SR.Argument_NeedNonemptyPipeName);
			}
			if ((options & ~(PipeOptions.WriteThrough | PipeOptions.Asynchronous)) != 0) {
				throw new ArgumentOutOfRangeException(nameof(options), SR.ArgumentOutOfRange_OptionsInvalid);
			}
			if (inBufferSize < 0) {
				throw new ArgumentOutOfRangeException(nameof(inBufferSize), SR.ArgumentOutOfRange_NeedNonNegNum);
			}
			if ((maxNumberOfServerInstances < 1 || maxNumberOfServerInstances > 254) && (maxNumberOfServerInstances != MaxAllowedServerInstances)) {
				throw new ArgumentOutOfRangeException(nameof(maxNumberOfServerInstances), SR.ArgumentOutOfRange_MaxNumServerInstances);
			}
			if (inheritability < HandleInheritability.None || inheritability > HandleInheritability.Inheritable) {
				throw new ArgumentOutOfRangeException(nameof(inheritability), SR.ArgumentOutOfRange_HandleInheritabilityNoneOrInheritable);
			}
			if ((additionalAccessRights & ~(PipeAccessRights.ChangePermissions | PipeAccessRights.TakeOwnership | PipeAccessRights.AccessSystemSecurity)) != 0) {
				throw new ArgumentOutOfRangeException(nameof(additionalAccessRights), SR.ArgumentOutOfRange_AdditionalAccessLimited);
			}

			if (s_isWindows) {
				options = (PipeOptions)((int)options | (int)additionalAccessRights);
			} else {
				if (additionalAccessRights != 0)
					throw new PlatformNotSupportedException();
				if (pipeSecurity != null)
					throw new PlatformNotSupportedException();
			}

			Create(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, inheritability);

			if (s_isWindows) {
				pipeSecurity.Persist(SafePipeHandle);
			}
		}
	}
}
