#if NET_2_0
using System;
using System.Web;
using System.Web.UI;

namespace NunitWeb
{
	/// <summary>
	/// This structure holds callbacks for all page events, callback for
	/// <see cref="IHttpHandler.ProcessRequest"/> and user data
	/// passed to these callbacks.
	/// </summary>
	[Serializable]
	public struct PageDelegates
	{
		/// <summary>
		/// <see cref="IHttpHandler.ProcessRequest"/> callback.
		/// </summary>
		public Helper.AnyMethod MyHandlerCallback;
		/// <summary>
		/// <see cref="Page.LoadComplete"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage LoadComplete;
		/// <summary>
		/// <see cref="Page.PreInit"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage PreInit;
		/// <summary>
		/// <see cref="Page.PreLoad"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage PreLoad;
		/// <summary>
		/// <see cref="Page.PreRenderComplete"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage PreRenderComplete;
		/// <summary>
		/// <see cref="Page.InitComplete"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage InitComplete;
		/// <summary>
		/// <see cref="Page.SaveStateComplete"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage SaveStateComplete;
		/// <summary>
		/// <see cref="TemplateControl.CommitTransaction"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage CommitTransaction;
		/// <summary>
		/// <see cref="TemplateControl.AbortTransaction"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage AbortTransaction;
		/// <summary>
		/// <see cref="TemplateControl.Error"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage Error;
		/// <summary>
		/// <see cref="Control.Disposed"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage Disposed;
		/// <summary>
		/// <see cref="Control.DataBinding"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage DataBinding;
		/// <summary>
		/// <see cref="Control.Init"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage Init;
		/// <summary>
		/// <see cref="Control.Load"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage Load;
		/// <summary>
		/// <see cref="Control.PreRender"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage PreRender;
		/// <summary>
		/// <see cref="Control.Unload"/> event callback
		/// </summary>
		public Helper.AnyMethodInPage Unload;
		/// <summary>
		/// User data passed to all callbacks.
		/// </summary>
		public object Param;
	}
}
#endif
