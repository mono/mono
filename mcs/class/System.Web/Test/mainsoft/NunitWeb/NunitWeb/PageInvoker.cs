#if TARGET_JVM
#define BUG_78521_FIXED
#endif

using System;
using System.Web.UI;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// This class is used to pass and invoke the user callbacks to all possible
	/// <see cref="System.Web.UI.Page"/> lifecycle events. For the most
	/// used events <see cref="System.Web.UI.Control.Init"/> and
	/// <see cref="System.Web.UI.Control.Load"/> there are convenience
	/// creation methods <see cref="CreateOnInit"/> and <see cref="CreateOnLoad"/>.
	/// In .NET 2.0 there same applies to PreInit event.
	/// </summary>
	/// <seealso cref="System.Web.UI.Control.Init"/>
	/// <seealso cref="System.Web.UI.Control.Load"/>
	/// <seealso cref="CreateOnInit"/>
	/// <seealso cref="CreateOnLoad"/>
	[Serializable]
	public class PageInvoker:BaseInvoker
	{
		/// <summary>
		/// The constructor method.
		/// </summary>
		/// <param name="delegates">Value which initializes <see cref="Delegates"/> property.</param>
		/// <seealso cref="Delegates"/>
		public PageInvoker (PageDelegates delegates)
		{
			Delegates = delegates;
		}

		PageDelegates _delegates;
		/// <summary>
		/// Set or get the <see cref="PageDelegates"/> collection.
		/// </summary>
		/// <seealso cref="PageDelegates"/>
		public PageDelegates Delegates
		{
			get { return _delegates; }
			set { _delegates = value; }
		}

#if NET_2_0
		/// <summary>
		/// Create a new <see cref="PageInvoker"/> which Delegates contain the
		/// given callback for PreInit event.
		/// </summary>
		/// <param name="callback">The user callback.</param>
		/// <returns>A new <see cref="PageInvoker"/> instance.</returns>
		public static PageInvoker CreateOnPreInit (PageDelegate callback)
		{
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = callback;
			PageInvoker pi = new PageInvoker (pd);
			return pi;
		}
#endif

		/// <summary>
		/// Create a new <see cref="PageInvoker"/> which Delegates contain the
		/// given callback for Init event.
		/// </summary>
		/// <param name="callback">The user callback.</param>
		/// <returns>A new <see cref="PageInvoker"/> instance.</returns>
		public static PageInvoker CreateOnInit (PageDelegate callback)
		{
			PageDelegates pd = new PageDelegates ();
			pd.Init = callback;
			PageInvoker pi = new PageInvoker (pd);
			return pi;
		}

		/// <summary>
		/// Create a new <see cref="PageInvoker"/> which Delegates contain the
		/// given callback for Load event.
		/// </summary>
		/// <param name="callback">The user callback.</param>
		/// <returns>A new <see cref="PageInvoker"/> instance.</returns>
		public static PageInvoker CreateOnLoad (PageDelegate callback)
		{
			PageDelegates pd = new PageDelegates ();
			pd.Load = callback;
			PageInvoker pi = new PageInvoker (pd);
			return pi;
		}

		[NonSerialized]
		Page _page;

		/// <summary>
		/// Add the callbacks contained in <see cref="Delegates"/> to
		/// the given page's events.
		/// </summary>
		/// <param name="parameters">Must contain one parameter of type
		/// <see cref="System.Web.UI.Page"/></param>
		/// <seealso cref="Delegates"/>
		public override void DoInvoke (params object [] parameters)
		{
			base.DoInvoke (parameters);
			if (parameters.Length != 1 || !(parameters[0] is Page))
				throw new ArgumentException ("A single parameter with type System.Web.UI.Page is expected");

			_page = (Page) parameters[0];

#if NET_2_0
#if BUG_78521_FIXED
			_page.PreInit += OnPreInit;
#else
			OnPreInit (null, null);
#endif
			_page.LoadComplete += OnLoadComplete;
			_page.PreLoad += OnPreLoad;
			_page.PreRenderComplete += OnPreRenderComplete;
			_page.InitComplete += OnInitComplete;
			_page.SaveStateComplete += OnSaveStateComplete;
#endif
			_page.CommitTransaction += new EventHandler (OnCommitTransaction);
			_page.AbortTransaction += new EventHandler (OnAbortTransaction);
			_page.Error += new EventHandler (OnError);
			_page.Disposed += new EventHandler (OnDisposed);
			_page.DataBinding += new EventHandler (OnDataBinding);
			_page.Init += new EventHandler (OnInit);
			_page.Load += new EventHandler (OnLoad);
			_page.PreRender += new EventHandler (OnPreRender);
			_page.Unload += new EventHandler (OnUnload);
		}

		#region Handlers

#if NET_2_0
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnLoadComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.LoadComplete);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnPreInit (object sender, EventArgs a)
		{
			Invoke (Delegates.PreInit);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnPreLoad (object sender, EventArgs a)
		{
			Invoke (Delegates.PreLoad);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnPreRenderComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.PreRenderComplete);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnInitComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.InitComplete);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnSaveStateComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.SaveStateComplete);
		}
#endif
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnCommitTransaction (object sender, EventArgs a)
		{
			Invoke (Delegates.CommitTransaction);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnAbortTransaction (object sender, EventArgs a)
		{
			Invoke (Delegates.AbortTransaction);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnError (object sender, EventArgs a)
		{
			Invoke (Delegates.Error);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnDisposed (object sender, EventArgs a)
		{
			Invoke (Delegates.Disposed);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnDataBinding (object sender, EventArgs a)
		{
			Invoke (Delegates.DataBinding);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnInit (object sender, EventArgs a)
		{
			Invoke (Delegates.Init);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnLoad (object sender, EventArgs a)
		{
			Invoke (Delegates.Load);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnPreRender (object sender, EventArgs a)
		{
			Invoke (Delegates.PreRender);
		}
		/// <summary>
		/// This must be made private as soon as Mono allows using private methods for delegates
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="a"></param>
		public void OnUnload (object sender, EventArgs a)
		{
			Invoke (Delegates.Unload);
		}
		#endregion

		void Invoke (PageDelegate callback)
		{
			try {
				if (callback != null)
					callback (_page);
			}
			catch (Exception e) {
				WebTest.RegisterException (e);
				throw;
			}
		}

		/// <summary>
		/// Returns the URL of a generic empty page.
		/// </summary>
		/// <returns>The default URL.</returns>
		public override string GetDefaultUrl ()
		{
			return StandardUrl.EMPTY_PAGE;
		}
	}
}
