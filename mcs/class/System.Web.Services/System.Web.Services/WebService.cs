 // 
// System.Web.Services.WebService.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;

namespace System.Web.Services {
	public class WebService : MarshalByValueComponent {

		#region Fields

		HttpApplicationState application;
		HttpContext context;
		HttpServerUtility server;
		HttpSessionState session;
		IPrincipal user;

		#endregion // Fields

		#region Constructors

		public WebService ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		public HttpApplicationState Application {
			get { return application; }
		}

		public HttpContext Context {
			get { return context; }
		}

		public HttpServerUtility Server {
			get { return server; }
		}

		public HttpSessionState Session {
			get { return session; }
		}

		public IPrincipal User {
			get { return user; }
		}

		#endregion // Properties
	}
}
