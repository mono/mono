using System;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class BaseInvoker
	{
		public virtual void DoInvoke (object param)
		{
			// Do nothing in BaseInvoker
		}
		public virtual string GetDefaultUrl ()
		{
			return null;
		}
	}
}
