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
using System.Web.SessionState;
using System.Web;
using System.Web.Hosting;
using javax.servlet;
using javax.servlet.http;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.SessionState
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// <para>Manages session state information using Java EE session API.</para>
	/// </summary>
	public sealed partial class ServletSessionStateStoreProvider : SessionStateStoreProviderBase
	{
		const int MAX_MINUTES_TIMEOUT = int.MaxValue / 60;
		#region Public Interface

		public override SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout) {

			// we ignore this timeout and use web.xml settings.
			//must set now as this can be a last chance for ro item
			//GetSession (context, false).setMaxInactiveInterval (timeout * 60);
			int javaTimeoutInSeconds = GetSession (context, false).getMaxInactiveInterval ();			
			timeout = GetIntervalInMinutes (javaTimeoutInSeconds);
			ServletSessionStateItemCollection sessionState = new ServletSessionStateItemCollection (context);
			return new SessionStateStoreData (
				sessionState,
				sessionState.StaticObjects,
				timeout);
		}

		public override void CreateUninitializedItem (HttpContext context, string id, int timeout) {
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
			HttpSession session = GetSession (context, false, false);
			if (session == null)
				return null;
			ServletSessionStateItemCollection sessionState = session.getAttribute (J2EEConsts.SESSION_STATE) as ServletSessionStateItemCollection;
			if (sessionState == null) //was not set
				sessionState = new ServletSessionStateItemCollection (context);
			return new SessionStateStoreData (
				sessionState,
				sessionState.StaticObjects,
				GetIntervalInMinutes(session.getMaxInactiveInterval ()));
		}

		public override SessionStateStoreData GetItemExclusive (HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions) {
			return GetItem (context, id, out locked, out lockAge, out lockId, out actions);
		}

		public override void InitializeRequest (HttpContext context) {
		}

		public override void ReleaseItemExclusive (HttpContext context, string id, object lockId) {
		}

		public override void RemoveItem (HttpContext context, string id, object lockId, SessionStateStoreData item) {
			GetSession (context, false).invalidate ();
		}

		public override void ResetItemTimeout (HttpContext context, string id) {
			if (context == null)
				throw new ArgumentNullException ("context");
			HttpSession session = GetSession (context, false);
			int current = session.getMaxInactiveInterval ();
			int requested = context.Session.Timeout * 60;
			if (current != requested)
				session.setMaxInactiveInterval (requested);
		}

		public override void SetAndReleaseItemExclusive (HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem) {
			if (id == null)
				return;

			if (item.Items.Dirty)
				GetSession(context, false).setAttribute (J2EEConsts.SESSION_STATE, item.Items);

			ReleaseItemExclusive (context, id, lockId);
		}

		public override bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback) {
			return true; //we call session.invalidate so our session listener will call Session_OnEnd
		}

		#endregion

		#region helpers

		internal static HttpSessionStateContainer CreateContainer (HttpSession session) {
			ServletSessionStateItemCollection sessionState = session.getAttribute (J2EEConsts.SESSION_STATE) as ServletSessionStateItemCollection;
			if (sessionState == null) //was not set
				sessionState = new ServletSessionStateItemCollection (null);

			return new HttpSessionStateContainer (session.getId (),
				sessionState, sessionState.StaticObjects,
				GetIntervalInMinutes (session.getMaxInactiveInterval ()),
				session.isNew (),
				HttpCookieMode.AutoDetect, SessionStateMode.Custom,
				true);
		}

		internal static HttpSession GetSession (HttpContext context, bool create) {
			return GetSession (context, create, true);
		}

		internal static HttpSession GetSession (HttpContext context, bool create, bool throwOnError) {
			HttpSession session = J2EEUtils.GetWorkerRequest (context).GetSession (create);
			if (session == null && throwOnError)
				throw new HttpException ("Session is not established");

			return session;
		}

		static int GetIntervalInMinutes (int seconds)
		{
			if (seconds == -1)
				return MAX_MINUTES_TIMEOUT;
			return (int) Math.Ceiling ((double) seconds / 60);		
		}

		#endregion
	}
}
