namespace System.Runtime.Remoting
{
	public sealed class ObjectHandle : MarshalByRefObject
	{
#region Keep this code, it is used by the runtime
#pragma warning disable 169, 649
		private object _wrapped;
#pragma warning restore 169, 649
#endregion

		public ObjectHandle (object o)
		{
			throw new PlatformNotSupportedException ();
		}

		public override object InitializeLifetimeService ()
		{
			throw new PlatformNotSupportedException ();
		}

		public object Unwrap ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
