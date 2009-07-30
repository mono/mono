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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Profile;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpServerUtilityBase
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual string MachineName { get { NotImplemented (); return null; } }

		public virtual int ScriptTimeout { get { NotImplemented (); return 0; } set { NotImplemented (); } }


		public virtual void ClearError ()
		{
			NotImplemented ();
		}

		public virtual object CreateObject (string progID)
		{
			NotImplemented ();
			return null;
		}

		public virtual object CreateObject (Type type)
		{
			NotImplemented ();
			return null;
		}

		public virtual object CreateObjectFromClsid (string clsid)
		{
			NotImplemented ();
			return null;
		}

		public virtual void Execute (string path)
		{
			NotImplemented ();
		}

		public virtual void Execute (string path, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual void Execute (string path, TextWriter writer)
		{
			NotImplemented ();
		}

		public virtual void Execute (string path, TextWriter writer, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual Exception GetLastError ()
		{
			NotImplemented ();
			return null;
		}

		public virtual string HtmlDecode (string s)
		{
			NotImplemented ();
			return null;
		}

		public virtual void HtmlDecode (string s, TextWriter output)
		{
			NotImplemented ();
		}

		public virtual string HtmlEncode (string s)
		{
			NotImplemented ();
			return null;
		}

		public virtual void HtmlEncode (string s, TextWriter output)
		{
			NotImplemented ();
		}

		public virtual string MapPath (string path)
		{
			NotImplemented ();
			return null;
		}

		public virtual void Transfer (string path)
		{
			NotImplemented ();
		}

		public virtual void Transfer (string path, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual void Transfer (IHttpHandler handler, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual void TransferRequest (string path)
		{
			NotImplemented ();
		}

		public virtual void TransferRequest (string path, bool preserveForm)
		{
			NotImplemented ();
		}

		public virtual void TransferRequest (string path, bool preserveForm, string method, NameValueCollection headers)
		{
			NotImplemented ();
		}

		public virtual string UrlDecode (string s)
		{
			NotImplemented ();
			return null;
		}

		public virtual void UrlDecode (string s, TextWriter output)
		{
			NotImplemented ();
		}

		public virtual string UrlEncode (string s)
		{
			NotImplemented ();
			return null;
		}

		public virtual void UrlEncode (string s, TextWriter output)
		{
			NotImplemented ();
		}

		public virtual string UrlPathEncode (string s)
		{
			NotImplemented ();
			return null;
		}

		public virtual byte [] UrlTokenDecode (string input)
		{
			NotImplemented ();
			return null;
		}

		public virtual string UrlTokenEncode (byte [] input)
		{
			NotImplemented ();
			return null;
		}
	}
}
