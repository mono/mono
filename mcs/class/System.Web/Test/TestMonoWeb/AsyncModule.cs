using System;
using System.Web;

namespace TestMonoWeb
{
	/// <summary>
	/// Summary description for AsyncModule.
	/// </summary>
	public class AsyncModule : IHttpModule
	{
		HttpApplication _app;

		public void Init(HttpApplication app) {
			app.AddOnPreRequestHandlerExecuteAsync(
				new BeginEventHandler(this.BeginPreHandlerExecute), 
				new EndEventHandler(this.EndPreHandlerExecute));

			_app = app;
		}

		IAsyncResult BeginPreHandlerExecute(Object source, EventArgs e, AsyncCallback cb, Object extraData) {
			((HttpApplication) source).Context.Response.Write("AsyncModule.BeginPreHandlerExecute()<br>\n");

			AsynchOperation asynch = new AsynchOperation(cb, _app.Context, extraData);
			asynch.StartAsyncWork();
			return asynch;
		}
		
		void EndPreHandlerExecute(IAsyncResult ar) {
			((AsynchOperation) ar).Context.Response.Write("AsyncModule.EndPreHandlerExecute()<br>\n");
		}		

		public void Dispose() {
		}
	}
}
