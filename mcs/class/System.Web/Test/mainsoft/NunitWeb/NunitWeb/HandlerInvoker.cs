using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Delegates to a ser callback that is used with <see cref="HandlerInvoker"/>.
	/// It is invoked during <see cref="System.Web.IHttpHandler.ProcessRequest"/>.
	/// </summary>
	/// <seealso cref="HandlerInvoker"/>
	/// <seealso cref="System.Web.IHttpHandler.ProcessRequest"/>
	public delegate void HandlerDelegate ();

	/// <summary>
	/// This invoker calls a single user delegate of type <see cref="HandlerDelegate"/>.
	/// It's intended to be used with provided <see cref="System.Web.IHttpHandler"/> implementation
	/// to run callbacks in the web context <b>without</b> creating <see cref="System.Web.UI.Page"/>.
	/// This invoker is not widely used in favor of <see cref="PageInvoker"/>
	/// </summary>
	/// <seealso cref="HandlerDelegate"/>
	/// <seealso cref="System.Web.IHttpHandler"/>
	/// <seealso cref="System.Web.UI.Page"/>
	/// <seealso cref="PageInvoker"/>
	[Serializable]
	public class HandlerInvoker:BaseInvoker
	{
		HandlerDelegate callback;
		/// <summary>
		/// Create a new invoker with the given callback
		/// </summary>
		/// <param name="callback">The callback that is invoked during <System.Web.IHttpHandler.ProcessRequest/>.</param>
		public HandlerInvoker (HandlerDelegate callback)
		{
			if (callback == null)
				throw new ArgumentNullException ();
			this.callback = callback;
		}

		/// <summary>
		/// Overriden to call the user provided delegate.
		/// </summary>
		/// <param name="parameters">Ignored.</param>
		public override void DoInvoke (params object []parameters)
		{
			base.DoInvoke (parameters);
			callback ();
		}

		/// <summary>
		/// Returns the URL which maps to our <see cref="System.Web.IHttpHandler"/>
		/// implementation
		/// </summary>
		/// <returns>The default URL.</returns>
		/// <seealso cref="System.Web.IHttpHandler"/>
		public override string GetDefaultUrl ()
		{
			return StandardUrl.FAKE_PAGE;
		}
	}
}
