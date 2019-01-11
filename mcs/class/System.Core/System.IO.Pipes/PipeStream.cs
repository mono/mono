using System.Security.AccessControl;

namespace System.IO.Pipes
{
	public partial class PipeStream
	{
		public PipeSecurity GetAccessControl ()
		{
			if (State == PipeState.Closed) {
				throw Error.GetPipeNotOpen ();
			}            

			// PipeState must be Disconnected, Connected, or Broken
			return new PipeSecurity (SafePipeHandle, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
		}

		public void SetAccessControl (PipeSecurity pipeSecurity)
		{
			if (pipeSecurity == null) {
				throw new ArgumentNullException (nameof(pipeSecurity));
			}

			// Checks that State != WaitingToConnect and State != Closed
			CheckPipePropertyOperations ();

			// PipeState must be either Disconected or Connected
			pipeSecurity.Persist (SafePipeHandle);
		}
	}
}
