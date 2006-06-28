using System;
using System.Web.UI;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class PageInvoker:BaseInvoker
	{
		PageDelegates _delegates;
		public PageDelegates Delegates
		{
			get { return _delegates; }
			set { _delegates = value; }
		}

		public PageInvoker (PageDelegates delegates)
		{
			Delegates = delegates;
		}

		public static PageInvoker CreateOnPreInit (PageDelegate callback)
		{
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = callback;
			PageInvoker pi = new PageInvoker (pd);
			return pi;
		}

		public static PageInvoker CreateOnLoad (PageDelegate callback)
		{
			PageDelegates pd = new PageDelegates ();
			pd.Load = callback;
			PageInvoker pi = new PageInvoker (pd);
			return pi;
		}

		[NonSerialized]
		Page _page;

		public override void DoInvoke (object param)
		{
			base.DoInvoke (param);
			_page = (Page) param;
#if BUG_78521_FIXED
				_page.PreInit += OnPreInit;
#else
			OnPreInit (null, null);
#endif
#if NET_2_0
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
		public void OnLoadComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.LoadComplete);
		}
		public void OnPreInit (object sender, EventArgs a)
		{
			Invoke (Delegates.PreInit);
		}
		public void OnPreLoad (object sender, EventArgs a)
		{
			Invoke (Delegates.PreLoad);
		}
		public void OnPreRenderComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.PreRenderComplete);
		}
		public void OnInitComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.InitComplete);
		}
		public void OnSaveStateComplete (object sender, EventArgs a)
		{
			Invoke (Delegates.SaveStateComplete);
		}
		public void OnCommitTransaction (object sender, EventArgs a)
		{
			Invoke (Delegates.CommitTransaction);
		}
		public void OnAbortTransaction (object sender, EventArgs a)
		{
			Invoke (Delegates.AbortTransaction);
		}
		public void OnError (object sender, EventArgs a)
		{
			Invoke (Delegates.Error);
		}
		public void OnDisposed (object sender, EventArgs a)
		{
			Invoke (Delegates.Disposed);
		}
		public void OnDataBinding (object sender, EventArgs a)
		{
			Invoke (Delegates.DataBinding);
		}
		public void OnInit (object sender, EventArgs a)
		{
			Invoke (Delegates.Init);
		}
		public void OnLoad (object sender, EventArgs a)
		{
			Invoke (Delegates.Load);
		}
		public void OnPreRender (object sender, EventArgs a)
		{
			Invoke (Delegates.PreRender);
		}
		public void OnUnload (object sender, EventArgs a)
		{
			Invoke (Delegates.Unload);
		}
		#endregion

		void Invoke (PageDelegate callback)
		{
			if (callback != null)
				callback (_page);
		}

		public override string GetDefaultUrl ()
		{
			return StandardUrl.EMPTY_PAGE;
		}
	}
}
