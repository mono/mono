//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
namespace System.Web.SessionState 
{
	public static class SessionStateUtility
	{
		public static void AddHttpSessionStateToContext (HttpContext context, IHttpSessionState container)
		{
			if (context == null || container == null)
				return;
			if (context.Session != null)
				throw new HttpException ("An HttpSessionState object for the current session has already been added to the specified context.");
			
			HttpSessionState state = new HttpSessionState (container);
			context.SetSession (state);
		}

		public static IHttpSessionState GetHttpSessionStateFromContext (HttpContext context)
		{
			HttpSessionState session;
			if (context == null || (session = context.Session) == null)
				return null;
			
			return session.Container;
		}

		public static HttpStaticObjectsCollection GetSessionStaticObjects (HttpContext context)
		{
			HttpSessionState session;
			if (context == null || (session = context.Session) == null)
				return null;
			return session.Container.StaticObjects;
		}
		
		public static void RaiseSessionEnd (IHttpSessionState session, Object eventSource, EventArgs eventArgs)
		{
			HttpSessionState state = new HttpSessionState (session);
			HttpApplicationFactory.InvokeSessionEnd (state, eventSource, eventArgs);
		}

		public static void RemoveHttpSessionStateFromContext (HttpContext context)
		{
			if (context == null)
				return;
			context.SetSession (null);
		}
	}
}
