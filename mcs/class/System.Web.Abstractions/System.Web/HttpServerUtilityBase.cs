//
// HttpServerUtilityBase.cs
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
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Profile;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpServerUtilityBase
	{
		[MonoTODO]
		public virtual string MachineName { get; private set; }
		[MonoTODO]
		public virtual int ScriptTimeout { get; set; }

		[MonoTODO]
		public virtual void ClearError ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Object CreateObject (string progID)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Object CreateObject (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Object CreateObjectFromClsid (string clsid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Execute (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Execute (string path, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Execute (string path, TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Execute (string path, TextWriter writer, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Exception GetLastError ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string HtmlDecode (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void HtmlDecode (string s, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string HtmlEncode (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void HtmlEncode (string s, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string MapPath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Transfer (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Transfer (string path, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Transfer (IHttpHandler handler, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void TransferRequest (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void TransferRequest (string path, bool preserveForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void TransferRequest (string path, bool preserveForm, string method, NameValueCollection headers)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string UrlDecode (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void UrlDecode (string s, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string UrlEncode (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void UrlEncode (string s, TextWriter output)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string UrlPathEncode (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual byte [] UrlTokenDecode (string input)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string UrlTokenEncode (byte [] input)
		{
			throw new NotImplementedException ();
		}
	}
}
