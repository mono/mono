//
// HttpSessionStateWrapper.cs
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
using System.Web.SessionState;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpSessionStateWrapper : HttpSessionStateBase
	{
		HttpSessionState w;

		public HttpSessionStateWrapper (HttpSessionState httpSessionState)
		{
			if (httpSessionState == null)
				throw new ArgumentNullException ("httpSessionState");
			w = httpSessionState;
		}

		public override int CodePage {
			get { return w.CodePage; }
			set { w.CodePage = value; }
		}

		public override HttpSessionStateBase Contents {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override HttpCookieMode CookieMode {
			//get { return w.CookieMode; }
			get { throw new NotImplementedException (); }
		}

		public override int Count {
			get { return w.Count; }
		}

		public override bool IsCookieless {
			get { return w.IsCookieless; }
		}

		public override bool IsNewSession {
			get { return w.IsNewSession; }
		}

		public override bool IsReadOnly {
			get { return w.IsReadOnly; }
		}

		public override bool IsSynchronized {
			get { return w.IsSynchronized; }
		}

		public override object this [int index] {
			get { return w [index]; }
			set { w [index] = value; }
		}

		public override object this [string name] {
			get { return w [name]; }
			set { w [name] = value; }
		}

		public override NameObjectCollectionBase.KeysCollection Keys {
			get { return w.Keys; }
		}

		public override int LCID {
			get { return w.LCID; }
			set { w.LCID = value; }
		}

		public override SessionStateMode Mode {
			get { return w.Mode; }
		}

		public override string SessionID {
			get { return w.SessionID; }
		}

		public override HttpStaticObjectsCollectionBase StaticObjects {
			get { throw new NotImplementedException (); }
		}

		public override object SyncRoot {
			get { return w.SyncRoot; }
		}

		public override int Timeout {
			get { return w.Timeout; }
			set { w.Timeout = value; }
		}

		public override void Abandon ()
		{
			w.Abandon ();
		}

		public override void Add (string name, object value)
		{
			w.Add (name, value);
		}

		public override void Clear ()
		{
			w.Clear ();
		}

		public override void CopyTo (Array array, int index)
		{
			w.CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return w.GetEnumerator ();
		}

		public override void Remove (string name)
		{
			w.Remove (name);
		}

		public override void RemoveAll ()
		{
			w.RemoveAll ();
		}

		public override void RemoveAt (int index)
		{
			w.RemoveAt (index);
		}
	}
}
