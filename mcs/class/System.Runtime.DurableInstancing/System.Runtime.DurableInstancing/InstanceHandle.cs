using System;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceHandle
	{
		internal InstanceHandle (bool isValid)
		{
			IsValid = isValid;
		}

		public bool IsValid { get; private set; }

		public void Free ()
		{
			throw new NotImplementedException ();
		}
	}
}
