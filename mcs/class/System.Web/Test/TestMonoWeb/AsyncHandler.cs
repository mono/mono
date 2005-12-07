using System;
using System.Threading;
using System.Web;

namespace TestMonoWeb {
	/// <summary>
	/// Summary description for AsyncHandler.
	/// </summary>
	public class AsyncHandler : IHttpAsyncHandler {
		private HttpContext _context;
		public bool IsReusable {
			get {
				//To enable pooling, return true here.
				//This keeps the handler in memory.
				return false;
			}
		}

		public IAsyncResult BeginProcessRequest(HttpContext context,    AsyncCallback cb, Object extraData) {
			AsynchOperation asynch = new AsynchOperation(cb, context, null);
			asynch.StartAsyncWork();

			context.Response.Write("AsyncHandler.BeginProcessRequest<br>\n");
			context.Response.Flush();

			//Signal the application that asynchronous 
			//processing is being performed. 
			SomeResult asynchForBegin = new SomeResult();

			//Processing is not synchronous.
			asynchForBegin.SetSynch(false);

			//Processing is not complete.
			asynchForBegin.SetCompleted(false);

			_context = context;

			return new SomeResult();
		}

		public void EndProcessRequest(IAsyncResult result) {
			_context.Response.Write("AsyncHandler.EndProcessRequest<br>\n");
		}

		//This method is required but is not called.
		public void ProcessRequest(HttpContext context) {
		}

	}//end class

	public class SomeResult : IAsyncResult {

		/*
		An instance of this class is returned to the application.
		This class lets the application know how the BeginEventHandler method has been handled. The application checks the CompletedSynchronously method.
		*/

		private bool _blnIsCompleted = false;
		private Mutex myMutex = null;
		private Object myAsynchStateObject = null;
		private bool _blnCompletedSynchronously = false;

		public void SetCompleted(bool blnTrueOrFalse) {
			_blnIsCompleted = blnTrueOrFalse;
		}

		public void SetSynch(bool blnTrueOrFalse) {
			_blnCompletedSynchronously = blnTrueOrFalse;
		}

		public bool IsCompleted {
			/*
			  This is not called by the application. However, set it to true. 
			*/
			get {
				return _blnIsCompleted;
			}
		}

		public WaitHandle AsyncWaitHandle {
			//The application does not call this method.         
			get {
				return myMutex;
			}
		}

		public Object AsyncState {
			//The application does not call this method because
			//null is passed in as the last parameter to BeginEventHandler.
			get {
				return myAsynchStateObject;
			}
		}

		public bool CompletedSynchronously {
			//The application wants to know if this is synchronous.
			//Return true if the Begin method was called synchronously.
			get { 
				return _blnCompletedSynchronously;
			}
		}
	}	
}
