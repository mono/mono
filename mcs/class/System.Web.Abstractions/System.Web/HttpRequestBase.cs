//
// HttpRequestBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Caching;

#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
using System.Web.Routing;
#endif

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpRequestBase
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual string [] AcceptTypes { get { NotImplemented (); return null; } }

		public virtual string AnonymousID { get { NotImplemented (); return null; } }

		public virtual string ApplicationPath { get { NotImplemented (); return null; } }

		public virtual string AppRelativeCurrentExecutionFilePath { get { NotImplemented (); return null; } }

		public virtual HttpBrowserCapabilitiesBase Browser { get { NotImplemented (); return null; } }

		public virtual HttpClientCertificate ClientCertificate { get { NotImplemented (); return null; } }

		public virtual Encoding ContentEncoding { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual int ContentLength { get { NotImplemented (); return 0; } }

		public virtual string ContentType { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual HttpCookieCollection Cookies { get { NotImplemented (); return null; } }

		public virtual string CurrentExecutionFilePath { get { NotImplemented (); return null; } }

		public virtual string FilePath { get { NotImplemented (); return null; } }

		public virtual HttpFileCollectionBase Files { get { NotImplemented (); return null; } }

		public virtual Stream Filter { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual NameValueCollection Form { get { NotImplemented (); return null; } }

		public virtual NameValueCollection Headers { get { NotImplemented (); return null; } }

		public virtual string HttpMethod { get { NotImplemented (); return null; } }

		public virtual Stream InputStream { get { NotImplemented (); return null; } }
#if NET_4_0
		// LAMESPEC: MSDN says NotImplementedException is thrown only when the request is
		// not IIS7WorkerRequest or ISAPIWorkerRequestInProc, but it is thrown always.
		public virtual ChannelBinding HttpChannelBinding { get { NotImplemented (); return null; } }
#endif
		public virtual bool IsAuthenticated { get { NotImplemented (); return false; } }

		public virtual bool IsLocal { get { NotImplemented (); return false; } }

		public virtual bool IsSecureConnection { get { NotImplemented (); return false; } }

		public virtual string this [string key] {
			get { throw new NotImplementedException (); }
		}

		public virtual WindowsIdentity LogonUserIdentity { get { NotImplemented (); return null; } }

		public virtual NameValueCollection Params { get { NotImplemented (); return null; } }

		public virtual string Path { get { NotImplemented (); return null; } }

		public virtual string PathInfo { get { NotImplemented (); return null; } }

		public virtual string PhysicalApplicationPath { get { NotImplemented (); return null; } }

		public virtual string PhysicalPath { get { NotImplemented (); return null; } }

		public virtual NameValueCollection QueryString { get { NotImplemented (); return null; } }

		public virtual string RawUrl { get { NotImplemented (); return null; } }

		public virtual string RequestType { get { NotImplemented (); return null; } set { NotImplemented (); } }
#if NET_4_0
		public virtual RequestContext RequestContext {
			get { NotImplemented (); return null; } set { NotImplemented (); } 
			internal set { NotImplemented (); }
		}
#endif
		public virtual NameValueCollection ServerVariables { get { NotImplemented (); return null; } }

		public virtual int TotalBytes { get { NotImplemented (); return 0; } }

		public virtual Uri Url { get { NotImplemented (); return null; } }

		public virtual Uri UrlReferrer { get { NotImplemented (); return null; } }

		public virtual string UserAgent { get { NotImplemented (); return null; } }

		public virtual string UserHostAddress { get { NotImplemented (); return null; } }

		public virtual string UserHostName { get { NotImplemented (); return null; } }

		public virtual string [] UserLanguages { get { NotImplemented (); return null; } }


		public virtual byte [] BinaryRead (int count)
		{
			NotImplemented ();
			return null;
		}

		public virtual int [] MapImageCoordinates (string imageFieldName)
		{
			NotImplemented ();
			return null;
		}

		public virtual string MapPath (string virtualPath)
		{
			NotImplemented ();
			return null;
		}

		public virtual string MapPath (string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			NotImplemented ();
			return null;
		}

		public virtual void SaveAs (string filename, bool includeHeaders)
		{
			NotImplemented ();
		}

		public virtual void ValidateInput ()
		{
			NotImplemented ();
		}
	}
}
