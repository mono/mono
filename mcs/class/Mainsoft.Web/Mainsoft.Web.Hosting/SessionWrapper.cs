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

		public static IHttpHandler WrapHandler (IHttpHandler handler) {
			Type type = (Type) ((IServiceProvider) handler).GetService (typeof (Type));

			if (IRequiresSessionStateType.IsAssignableFrom (type))
				return IReadOnlySessionStateType.IsAssignableFrom (type) ?
					(handler is IHttpExtendedHandler ? (IHttpHandler) new ReadOnlySessionWrapperExtendedHandler ((IHttpExtendedHandler) handler) : new ReadOnlySessionWrapperHandler (handler)) :
					(handler is IHttpExtendedHandler ? new SessionWrapperExtendedHandler ((IHttpExtendedHandler) handler) : new SessionWrapperHandler (handler));
			return handler;
		}

		#region SessionWrappers

		class SessionWrapperHandler : IHttpHandler, IRequiresSessionState, IServiceProvider
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

			public object GetService (Type serviceType) {
				return ((IServiceProvider) _handler).GetService (serviceType);
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
