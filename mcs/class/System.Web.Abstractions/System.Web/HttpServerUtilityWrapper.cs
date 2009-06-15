//
// HttpServerUtilityWrapper.cs
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
	public class HttpServerUtilityWrapper : HttpServerUtilityBase
	{
		HttpServerUtility w;

		public HttpServerUtilityWrapper (HttpServerUtility httpServerUtility)
		{
			if (httpServerUtility == null)
				throw new ArgumentNullException ("httpServerUtility");
			w = httpServerUtility;
		}

		public override string MachineName {
			get { return w.MachineName; }
		}

		public override int ScriptTimeout {
			get { return w.ScriptTimeout; }
			set { w.ScriptTimeout = value; }
		}

		public override void ClearError ()
		{
			w.ClearError ();
		}

		public override object CreateObject (string progID)
		{
			return w.CreateObject (progID);
		}

		public override object CreateObject (Type type)
		{
			return w.CreateObject (type);
		}

		public override object CreateObjectFromClsid (string clsid)
		{
			return w.CreateObjectFromClsid (clsid);
		}

		public override void Execute (string path)
		{
			w.Execute (path);
		}

		public override void Execute (string path, bool preserveForm)
		{
			w.Execute (path, preserveForm);
		}

		public override void Execute (string path, TextWriter writer)
		{
			w.Execute (path, writer);
		}

		public override void Execute (string path, TextWriter writer, bool preserveForm)
		{
			w.Execute (path, writer, preserveForm);
		}

		public override void Execute (IHttpHandler handler, TextWriter writer, bool preserveForm)
		{
			w.Execute (handler, writer, preserveForm);
		}

		public override Exception GetLastError ()
		{
			return w.GetLastError ();
		}

		public override string HtmlDecode (string s)
		{
			return w.HtmlDecode (s);
		}

		public override void HtmlDecode (string s, TextWriter output)
		{
			w.HtmlDecode (s, output);
		}

		public override string HtmlEncode (string s)
		{
			return w.HtmlEncode (s);
		}

		public override void HtmlEncode (string s, TextWriter output)
		{
			w.HtmlEncode (s, output);
		}

		public override string MapPath (string path)
		{
			return w.MapPath (path);
		}

		public override void Transfer (string path)
		{
			w.Transfer (path);
		}

		public override void Transfer (string path, bool preserveForm)
		{
			w.Transfer (path, preserveForm);
		}

		public override void Transfer (IHttpHandler handler, bool preserveForm)
		{
			w.Transfer (handler, preserveForm);
		}

		[MonoTODO]
		public override void TransferRequest (string path)
		{
			// return TransferRequest (path);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TransferRequest (string path, bool preserveForm)
		{
			// return TransferRequest (path, preserveForm);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void TransferRequest (string path, bool preserveForm, string method, NameValueCollection headers)
		{
			// return TransferRequest (path, preserveForm, method, headers);
			throw new NotImplementedException ();
		}

		public override string UrlDecode (string s)
		{
			return w.UrlDecode (s);
		}

		public override void UrlDecode (string s, TextWriter output)
		{
			w.UrlDecode (s, output);
		}

		public override string UrlEncode (string s)
		{
			return w.UrlEncode (s);
		}

		public override void UrlEncode (string s, TextWriter output)
		{
			w.UrlEncode (s, output);
		}

		public override string UrlPathEncode (string s)
		{
			return w.UrlPathEncode (s);
		}

		[MonoTODO]
		public override byte [] UrlTokenDecode (string input)
		{
			// return w.UrlTokenDecode (input);
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string UrlTokenEncode (byte [] input)
		{
			// return w.UrlTokenEncode (input);
			throw new NotImplementedException ();
		}
	}
}
