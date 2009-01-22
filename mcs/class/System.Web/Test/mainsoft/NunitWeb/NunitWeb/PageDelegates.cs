using System;
using System.Web;
using System.Web.UI;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Delegates to a user callback invoked during different
	/// <see cref="System.Web.UI.Page"/> lifecycle events and passing the page. Used
	/// with <see cref="PageDelegate"/>.
	/// </summary>
	/// <param name="page"></param>
	/// <seealso cref="System.Web.UI.Page"/>
	/// <seealso cref="PageDelegate"/>
	public delegate void PageDelegate (Page page);
	/// <summary>
	/// This structure holds callbacks for all page events, callback for
	/// <see cref="IHttpHandler.ProcessRequest"/> and user data
	/// passed to these callbacks.
	/// </summary>
	[Serializable]
	public struct PageDelegates
	{
#if NET_2_0
		/// <summary>
		/// <see cref="Page.LoadComplete"/> event callback.
		/// </summary>
		public PageDelegate LoadComplete;
		/// <summary>
		/// <see cref="Page.PreInit"/> event callback.
		/// </summary>
		public PageDelegate PreInit;
		/// <summary>
		/// <see cref="Page.PreLoad"/> event callback.
		/// </summary>
		public PageDelegate PreLoad;
		/// <summary>
		/// <see cref="Page.PreRenderComplete"/> event callback.
		/// </summary>
		public PageDelegate PreRenderComplete;
		/// <summary>
		/// <see cref="Page.InitComplete"/> event callback.
		/// </summary>
		public PageDelegate InitComplete;
		/// <summary>
		/// <see cref="Page.SaveStateComplete"/> event callback.
		/// </summary>
		public PageDelegate SaveStateComplete;
#endif
		/// <summary>
		/// <see cref="TemplateControl.CommitTransaction"/> event callback.
		/// </summary>
		public PageDelegate CommitTransaction;
		/// <summary>
		/// <see cref="TemplateControl.AbortTransaction"/> event callback.
		/// </summary>
		public PageDelegate AbortTransaction;
		/// <summary>
		/// <see cref="TemplateControl.Error"/> event callback.
		/// </summary>
		public PageDelegate Error;
		/// <summary>
		/// <see cref="Control.Disposed"/> event callback.
		/// </summary>
		public PageDelegate Disposed;
		/// <summary>
		/// <see cref="Control.DataBinding"/> event callback.
		/// </summary>
		public PageDelegate DataBinding;
		/// <summary>
		/// <see cref="Control.Init"/> event callback.
		/// </summary>
		public PageDelegate Init;
		/// <summary>
		/// <see cref="Control.Load"/> event callback.
		/// </summary>
		public PageDelegate Load;
		/// <summary>
		/// <see cref="Control.PreRender"/> event callback.
		/// </summary>
		public PageDelegate PreRender;
		/// <summary>
		/// <see cref="Control.Unload"/> event callback.
		/// </summary>
		public PageDelegate Unload;
	}
}
