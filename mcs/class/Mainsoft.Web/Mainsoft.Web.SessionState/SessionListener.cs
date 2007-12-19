//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Web.SessionState;
using System.Reflection;
using javax.servlet.http;
using Mainsoft.Web.Hosting;
using System.Diagnostics;

namespace Mainsoft.Web.SessionState
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class SessionListener : javax.servlet.http.HttpSessionListener
	{

		public void sessionCreated (HttpSessionEvent se) {
		}

		public void sessionDestroyed (HttpSessionEvent se) {
			bool setDomain = vmw.@internal.EnvironmentUtils.getAppDomain () == null;
			if (setDomain) {
				AppDomain servletDomain = (AppDomain) se.getSession ().getServletContext ().getAttribute (J2EEConsts.APP_DOMAIN);
				if (servletDomain == null)
					return;
				vmw.@internal.EnvironmentUtils.setAppDomain (servletDomain);
			}
			try {
				HttpSessionStateContainer container =
					ServletSessionStateStoreProvider.CreateContainer (se.getSession ());

				SessionStateUtility.RaiseSessionEnd (container, this, EventArgs.Empty);
			}
			catch (Exception e) {
				Debug.WriteLine (e.Message);
				Debug.WriteLine (e.StackTrace);
			}
			finally {
				if (setDomain) {
					vmw.@internal.EnvironmentUtils.clearAppDomain ();
				}
			}
		}
	}
}

namespace System.Web.GH
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class SessionListener : Mainsoft.Web.SessionState.SessionListener
	{
	}
}

namespace System.Web.J2EE
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// </summary>
	public class SessionListener : Mainsoft.Web.SessionState.SessionListener
	{
	}
}
