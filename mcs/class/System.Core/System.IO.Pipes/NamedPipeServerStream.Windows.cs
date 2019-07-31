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
	}
}
