using System;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class BaseInvoker
	{
		bool _invokeDone = false;
		public virtual void DoInvoke (object param)
		{
			_invokeDone = true;
		}
		public virtual string GetDefaultUrl ()
		{
			return null;
		}
		public virtual void CheckInvokeDone ()
		{
			if (!_invokeDone)
				throw new Exception ("Invoker was not activated");
		}
	}
}
