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

		HttpContext _context;

		#endregion // Fields

		#region Constructors

		public WebService ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[Description ("The ASP.NET application object for the current request.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpApplicationState Application {
			get { return _context.Application; }
		}

		[Browsable (false)]
		[WebServicesDescription ("The ASP.NET context object for the current request.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpContext Context {
			get { return _context; }
		}

		[Browsable (false)]
		[WebServicesDescription ("The ASP.NET utility object for the current request.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpServerUtility Server {
			get { return _context.Server; }
		}

		[Browsable (false)]
		[WebServicesDescription ("The ASP.NET session object for the current request.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpSessionState Session {
			get { return _context.Session; }
		}

		[Browsable (false)]
		[WebServicesDescription ("The ASP.NET user object for the current request.  The object is used for authorization.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IPrincipal User {
			get { return _context.User; }
		}

		#endregion // Properties
	}
}
