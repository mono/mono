using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.J2EE;
using System.Web.SessionState;

namespace Mainsoft.Web.Hosting
{
	public static class SessionWrapper
	{
		static readonly Type IRequiresSessionStateType = typeof (IRequiresSessionState);
		static readonly Type IReadOnlySessionStateType = typeof (IReadOnlySessionState);

		public static IHttpHandler WrapHandler (IHttpHandler handler, HttpContext context, string url) {
			Type type = PageMapper.GetObjectType (context, url);

			if (IRequiresSessionStateType.IsAssignableFrom (type))
				return IReadOnlySessionStateType.IsAssignableFrom (type) ?
					new ReadOnlySessionWrapperHandler (handler) : new SessionWrapperHandler (handler);
			return handler;
		}
		public static IHttpHandler WrapHandler (IHttpExtendedHandler handler, HttpContext context, string url) {
			Type type = PageMapper.GetObjectType (context, url);

			if (IRequiresSessionStateType.IsAssignableFrom (type))
				return IReadOnlySessionStateType.IsAssignableFrom (type) ?
					new ReadOnlySessionWrapperExtendedHandler (handler) : new SessionWrapperExtendedHandler (handler);
			return handler;
		}

		#region SessionWrappers

		class SessionWrapperHandler : IHttpHandler, IRequiresSessionState
		{
			protected readonly IHttpHandler _handler;

			public SessionWrapperHandler (IHttpHandler handler) {
				_handler = handler;
			}

			public bool IsReusable {
				get { return _handler.IsReusable; }
			}

			public void ProcessRequest (HttpContext context) {
				_handler.ProcessRequest (context);
			}
		}

		sealed class ReadOnlySessionWrapperHandler : SessionWrapperHandler, IReadOnlySessionState
		{
			public ReadOnlySessionWrapperHandler (IHttpHandler handler) : base (handler) { }
		}

		#endregion

		#region ExtendedSessionWrappers

		class SessionWrapperExtendedHandler : SessionWrapperHandler, IHttpExtendedHandler
		{
			public SessionWrapperExtendedHandler (IHttpExtendedHandler handler)
				: base (handler) {
			}

			public bool IsCompleted {
				get { return ((IHttpExtendedHandler) _handler).IsCompleted; }
			}
		}

		sealed class ReadOnlySessionWrapperExtendedHandler : SessionWrapperExtendedHandler, IReadOnlySessionState
		{
			public ReadOnlySessionWrapperExtendedHandler (IHttpExtendedHandler handler) : base (handler) { }
		}

		#endregion
	}
}
