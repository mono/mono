//
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger <kostat@mainsoft.com>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.SessionState;
using javax.servlet.http;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.SessionState
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public sealed class ServletSessionIDManager : ISessionIDManager
	{
		#region ISessionIDManager Members

		public string CreateSessionID (HttpContext context) {
			return ServletSessionStateStoreProvider.GetSession (context, true).getId ();
		}

		public string GetSessionID (HttpContext context) {
			BaseWorkerRequest request = J2EEUtils.GetWorkerRequest (context);
			return request.IsRequestedSessionIdValid () ? request.GetRequestedSessionId () : null;
		}

		public void Initialize () {
		}

		public bool InitializeRequest (HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue) {
			supportSessionIDReissue = true;
			return false;
		}

		public void RemoveSessionID (HttpContext context) {
			ServletSessionStateStoreProvider.GetSession (context, false).invalidate ();
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
