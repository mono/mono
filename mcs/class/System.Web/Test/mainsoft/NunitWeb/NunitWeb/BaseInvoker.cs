using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Base class for invokers. Can be used on its own when no user callbacks need
	/// to be executed in the web context. When a user callback need to be called, use
	/// one of <see cref="BaseInvoker"/> subclasses, the most common is
	/// <seealso cref="PageInvoker"/>.
	/// </summary>
	[Serializable]
	public class BaseInvoker
	{
		bool _invokeDone = false;
		/// <summary>
		/// This method is called to activate the invoker. When <see cref="BaseInvoker"/>
		/// is overriden, the subclasses should call the base class DoInvoke, if they want
		/// to use the default CheckInvokeDone implementation.
		/// </summary>
		/// <param name="parameters"></param>
		public virtual void DoInvoke (params object [] parameters)
		{
			_invokeDone = true;
		}

		/// <summary>
		/// This method returns the default URL specific to the invoker type. By default,
		/// there is no default URL.
		/// </summary>
		/// <returns></returns>
		public virtual string GetDefaultUrl ()
		{
			return null;
		}

		/// <summary>
		/// Check, if DoInvoke was called or not. If subclasses do not override this
		/// method, they have to call to <seealso cref="BaseInvoker.DoInvoke"/> to register the
		/// invocation.
		/// </summary>
		public virtual void CheckInvokeDone ()
		{
			if (!_invokeDone)
				throw new Exception ("Invoker was not activated");
		}
	}
}
