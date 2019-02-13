using System.Runtime.Serialization;

namespace System
{
	partial class Exception
	{
		protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState;

		internal readonly struct DispatchState
		{
		}

		internal DispatchState CaptureDispatchState ()
		{
			throw new NotImplementedException ();
		}

		internal void RestoreDispatchState (DispatchState state)
		{
		}
	}
}