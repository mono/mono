using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Base class for invokers, which can be used on its own when no user callbacks need
	/// to be executed in the Web context. When a user callback needs to be called, use
	/// one of the <see cref="BaseInvoker"/> subclasses (the most common is
	/// <see cref="PageInvoker"/>).
	/// </summary>
	/// <seealso cref="PageInvoker"/>
	[Serializable]
	public class BaseInvoker
	{
		bool _invokeDone = false;
		/// <summary>
		/// This method is called to activate the invoker. When <see cref="BaseInvoker"/>
		/// is overriden, the subclasses should call the base class <c>DoInvoke</c>, if they want
		/// to use the default <see cref="CheckInvokeDone"/> implementation.
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
		/// Checks whether <c>DoInvoke</c> was called or not. If subclasses do not override this
		/// method, they have to call <see cref="BaseInvoker.DoInvoke"/> to register the
		/// invocation.
		/// </summary>
		/// <seealso cref="BaseInvoker.DoInvoke"/>
		public virtual void CheckInvokeDone ()
		{
			if (!_invokeDone)
				throw new Exception ("Invoker was not activated");
		}
	}
}
