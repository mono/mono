using System;
using System.Collections.Generic;
using System.Text;
using System.Web.SessionState;
using System.Web;
using System.Web.Hosting;
using javax.servlet;
using javax.servlet.http;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.SessionState
{
	public sealed partial class ServletSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout) {
			ServletSessionStateItemCollection sessionState = new ServletSessionStateItemCollection (context);
			return new SessionStateStoreData (
				sessionState,
				sessionState.StaticObjects,
				timeout);
			//return new SessionStateStoreData (new SessionStateItemCollection (),
			//                  SessionStateUtility.GetSessionStaticObjects (context),
			//                  timeout);
		}

		static internal ServletWorkerRequest GetWorkerRequest (HttpContext context) {
			IServiceProvider sp = (IServiceProvider) context;
			return (ServletWorkerRequest) sp.GetService (typeof (HttpWorkerRequest));
		}

		public override void CreateUninitializedItem (HttpContext context, string id, int timeout) {
			//HttpSession session = GetWorker(context).ServletRequest.getSession (false); //.setMaxInactiveInterval (timeout * 60);
		}

		public override void Dispose () {
			
		}

		public override void EndRequest (HttpContext context) {
		}

		public override SessionStateStoreData GetItem (HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions) {
			locked = false;
			lockAge = TimeSpan.Zero;
			lockId = null;
			actions = SessionStateActions.None;
			if (id == null)
				return null;
			ServletSessionStateItemCollection sessionState = (ServletSessionStateItemCollection) GetWorkerRequest (context).ServletRequest.getSession (false).getAttribute (J2EEConsts.SESSION_STATE);
			if (sessionState == null)
				return null;
			return new SessionStateStoreData (
				sessionState,
				sessionState.StaticObjects,
				10);
		}

		public override SessionStateStoreData GetItemExclusive (HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions) {
			return GetItem (context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override void InitializeRequest (HttpContext context) {
		}

		public override void ReleaseItemExclusive (HttpContext context, string id, object lockId) {
		}

		public override void RemoveItem (HttpContext context, string id, object lockId, SessionStateStoreData item) {
			GetWorkerRequest (context).ServletRequest.getSession (false).setAttribute (J2EEConsts.SESSION_STATE, null);
		}

		public override void ResetItemTimeout (HttpContext context, string id) {
			//Java does this for us
		}

		public override void SetAndReleaseItemExclusive (HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem) {
			GetWorkerRequest (context).ServletRequest.getSession (false).setAttribute (J2EEConsts.SESSION_STATE, item.Items);
		}

		public override bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback) {
			return true;
			//throw new Exception ();
		}
	}
}
