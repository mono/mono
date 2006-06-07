#if NET_2_0
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;
using System.IO;
using System.Configuration;
using System.Web.Configuration;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Reflection;
using System.Threading;

namespace NunitWeb
{
	internal class MyHost : MarshalByRefObject
	{
		public const string HELPER_INSTANCE_NAME = "mainsoft/NunitWeb/Helper";

		public void Initialize (Helper h)
		{
			AppDomain.CurrentDomain.SetData (HELPER_INSTANCE_NAME, h);
		}

		public AppDomain AppDomain
		{ get { return AppDomain.CurrentDomain; } }


		public string DoRun (string url, PageDelegates pd)
		{
			using (StringWriter tw = new StringWriter ()) {
				MyWorkerRequest wr = new MyWorkerRequest (pd, url, null, tw);
				HttpRuntime.ProcessRequest (wr);
				tw.Close ();
				string res = tw.ToString ();
				Exception inner = wr.Exception;
				RethrowException (inner);
				if (!wr.InitInvoked)
					throw new Exception ("internal error: ProcessRequest did not reach DelegateInvoker constructor; response was: "+res);
				//FIXME: check that all delegates were invoked
				return res;
			}
		}

		private static void RethrowException (Exception inner)
		{
			if (inner == null)
				return;

			Exception outer;
			try { //Try create a similar exception and keep the inner intact
				outer = (Exception) Activator.CreateInstance (inner.GetType (),
					inner.ToString(), inner);
			}
			catch { //Failed to create a similar, fallback to the inner, ruining the call stack
				throw inner;
			}
			throw outer;
		}

		public class DelegateInvoker
		{
			PageDelegates _pd;
			HttpContext _context;
			Page _page;
			public DelegateInvoker (HttpContext context, Page page)
			{
				MyWorkerRequest wr = GetMyWorkerRequest (context);
				wr.InitInvoked = true;
				_pd = wr.Delegates;
				_context = context;
				_page = page;
				_page.LoadComplete += OnLoadComplete;
#if BUG_78521_FIXED
				_page.PreInit += OnPreInit;
#else
				OnPreInit (null, null);
#endif
				_page.PreLoad += OnPreLoad;
				_page.PreRenderComplete += OnPreRenderComplete;
				_page.InitComplete += OnInitComplete;
				_page.SaveStateComplete += OnSaveStateComplete;
				_page.CommitTransaction += OnCommitTransaction;
				_page.AbortTransaction += OnAbortTransaction;
				_page.Error += OnError;
				_page.Disposed += OnDisposed;
				_page.DataBinding += OnDataBinding;
				_page.Init += OnInit;
				_page.Load += OnLoad;
				_page.PreRender += OnPreRender;
				_page.Unload += OnUnload;
			}
			#region Handlers
			public void OnLoadComplete (object sender, EventArgs a)
			{
				RunDelegate (_pd.LoadComplete);
			}
			public void OnPreInit (object sender, EventArgs a)
			{
				RunDelegate (_pd.PreInit);
			}
			public void OnPreLoad (object sender, EventArgs a)
			{
				RunDelegate (_pd.PreLoad);
			}
			public void OnPreRenderComplete (object sender, EventArgs a)
			{
				RunDelegate (_pd.PreRenderComplete);
			}
			public void OnInitComplete (object sender, EventArgs a)
			{
				RunDelegate (_pd.InitComplete);
			}
			public void OnSaveStateComplete (object sender, EventArgs a)
			{
				RunDelegate (_pd.SaveStateComplete);
			}
			public void OnCommitTransaction (object sender, EventArgs a)
			{
				RunDelegate (_pd.CommitTransaction);
			}
			public void OnAbortTransaction (object sender, EventArgs a)
			{
				RunDelegate (_pd.AbortTransaction);
			}
			public void OnError (object sender, EventArgs a)
			{
				RunDelegate (_pd.Error);
			}
			public void OnDisposed (object sender, EventArgs a)
			{
				RunDelegate (_pd.Disposed);
			}
			public void OnDataBinding (object sender, EventArgs a)
			{
				RunDelegate (_pd.DataBinding);
			}
			public void OnInit (object sender, EventArgs a)
			{
				RunDelegate (_pd.Init);
			}
			public void OnLoad (object sender, EventArgs a)
			{
				RunDelegate (_pd.Load);
			}
			public void OnPreRender (object sender, EventArgs a)
			{
				RunDelegate (_pd.PreRender);
			}
			public void OnUnload (object sender, EventArgs a)
			{
				RunDelegate (_pd.Unload);
			}
			#endregion
			void RunDelegate (Helper.AnyMethodInPage method)
			{
				RunAnyMethodInPage (_context, _page, method);
			}
		}

		public static void RunAnyMethod (HttpContext c)
		{
			MyWorkerRequest wr = GetMyWorkerRequest (c);
			//wr.DelegateInvoked = true;
			Helper.AnyMethod am = wr.Delegates.MyHandlerCallback;
			if (am == null)
				return;
			try {
				am (c, wr.Delegates.Param);
			}
			catch (Exception ex) {
				wr.Exception = ex;
				throw;
			}
		}

		public static void RunAnyMethodInPage (HttpContext context, Page page, Helper.AnyMethodInPage method)
		{
			MyWorkerRequest wr = GetMyWorkerRequest (context);
			//wr.DelegateInvoked = true;
			if (method == null)
				return;
			try {
				method (context, page, wr.Delegates.Param);
			}
			catch (Exception ex) {
				wr.Exception = ex;
				throw;
			}
		}

		private static MyWorkerRequest GetMyWorkerRequest (HttpContext c)
		{
			IServiceProvider isp = (IServiceProvider) c;
			return (MyWorkerRequest) (isp.GetService (typeof (HttpWorkerRequest)));
		}

	}
}
#endif
