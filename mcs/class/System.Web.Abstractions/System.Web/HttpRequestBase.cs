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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Web.Caching;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpRequestBase
	{
		[MonoTODO]
		public virtual string [] AcceptTypes { get; private set; }
		[MonoTODO]
		public virtual string AnonymousID { get; private set; }
		[MonoTODO]
		public virtual string ApplicationPath { get; private set; }
		[MonoTODO]
		public virtual string AppRelativeCurrentExecutionFilePath { get; private set; }
		[MonoTODO]
		public virtual HttpBrowserCapabilitiesBase Browser { get; private set; }
		[MonoTODO]
		public virtual HttpClientCertificate ClientCertificate { get; private set; }
		[MonoTODO]
		public virtual Encoding ContentEncoding { get; set; }
		[MonoTODO]
		public virtual int ContentLength { get; private set; }
		[MonoTODO]
		public virtual string ContentType { get; set; }
		[MonoTODO]
		public virtual HttpCookieCollection Cookies { get; private set; }
		[MonoTODO]
		public virtual string CurrentExecutionFilePath { get; private set; }
		[MonoTODO]
		public virtual string FilePath { get; private set; }
		[MonoTODO]
		public virtual HttpFileCollectionBase Files { get; private set; }
		[MonoTODO]
		public virtual Stream Filter { get; set; }
		[MonoTODO]
		public virtual NameValueCollection Form { get; private set; }
		[MonoTODO]
		public virtual NameValueCollection Headers { get; private set; }
		[MonoTODO]
		public virtual string HttpMethod { get; private set; }
		[MonoTODO]
		public virtual Stream InputStream { get; private set; }
		[MonoTODO]
		public virtual bool IsAuthenticated { get; private set; }
		[MonoTODO]
		public virtual bool IsLocal { get; private set; }
		[MonoTODO]
		public virtual bool IsSecureConnection { get; private set; }
		[MonoTODO]
		public virtual string this [string key] {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual WindowsIdentity LogonUserIdentity { get; private set; }
		[MonoTODO]
		public virtual NameValueCollection Params { get; private set; }
		[MonoTODO]
		public virtual string Path { get; private set; }
		[MonoTODO]
		public virtual string PathInfo { get; private set; }
		[MonoTODO]
		public virtual string PhysicalApplicationPath { get; private set; }
		[MonoTODO]
		public virtual string PhysicalPath { get; private set; }
		[MonoTODO]
		public virtual NameValueCollection QueryString { get; private set; }
		[MonoTODO]
		public virtual string RawUrl { get; private set; }
		[MonoTODO]
		public virtual string RequestType { get; set; }
		[MonoTODO]
		public virtual NameValueCollection ServerVariables { get; private set; }
		[MonoTODO]
		public virtual int TotalBytes { get; private set; }
		[MonoTODO]
		public virtual Uri Url { get; private set; }
		[MonoTODO]
		public virtual Uri UrlReferrer { get; private set; }
		[MonoTODO]
		public virtual string UserAgent { get; private set; }
		[MonoTODO]
		public virtual string UserHostAddress { get; private set; }
		[MonoTODO]
		public virtual string UserHostName { get; private set; }
		[MonoTODO]
		public virtual string [] UserLanguages { get; private set; }

		[MonoTODO]
		public virtual byte [] BinaryRead (int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int [] MapImageCoordinates (string imageFieldName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string MapPath (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string MapPath (string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SaveAs (string filename, bool includeHeaders)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ValidateInput ()
		{
			throw new NotImplementedException ();
		}
	}
}
