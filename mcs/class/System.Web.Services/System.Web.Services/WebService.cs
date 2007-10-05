 // 
// System.Web.Services.WebService.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.ComponentModel;
using System.Security.Principal;
using System.Web;
using System.Web.Services.Protocols;
using System.Web.SessionState;

namespace System.Web.Services {
	public class WebService : MarshalByValueComponent {

		#region Fields

		HttpContext _context;

#if NET_2_0
		SoapProtocolVersion _soapVersion;
#endif

		#endregion // Fields

		#region Constructors

		public WebService ()
		{
			_context = HttpContext.Current;
#if NET_2_0
			object o = _context != null ? _context.Items ["WebServiceSoapVersion"] : null;
			if (o is SoapProtocolVersion)
				_soapVersion = (SoapProtocolVersion) o;
#endif
		}
		
		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[Description ("The ASP.NET application object for the current request.")]
		[WebServicesDescription ("The ASP.NET application object for the current request.")]
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

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[System.Runtime.InteropServices.ComVisible (false)]
		public System.Web.Services.Protocols.SoapProtocolVersion SoapVersion {
			get { return _soapVersion; }
		}
#endif

		#endregion // Properties
	}
}
