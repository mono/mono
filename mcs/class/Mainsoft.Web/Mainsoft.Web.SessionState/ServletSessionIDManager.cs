using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;
using javax.servlet.http;

namespace Mainsoft.Web.SessionState
{
	public sealed class ServletSessionIDManager : ISessionIDManager
	{
		#region ISessionIDManager Members

		public string CreateSessionID (HttpContext context) {
			return ServletSessionStateStoreProvider.GetWorkerRequest (context).
				ServletRequest.getSession(true).getId();
		}

		public string GetSessionID (HttpContext context) {
			HttpServletRequest request = ServletSessionStateStoreProvider.GetWorkerRequest (context).ServletRequest;
			return request.isRequestedSessionIdValid () ? request.getRequestedSessionId () : null;
		}

		public void Initialize () {
		}

		public bool InitializeRequest (HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue) {
			supportSessionIDReissue = true;
			return false;
		}

		public void RemoveSessionID (HttpContext context) {
			ServletSessionStateStoreProvider.GetWorkerRequest (context).ServletRequest.getSession (false).invalidate ();
		}

		public void SaveSessionID (HttpContext context, string id, out bool redirected, out bool cookieAdded) {
			redirected = false;
			cookieAdded = false;
		}

		public bool Validate (string id) {
			return true;
		}

		#endregion
	}
}
