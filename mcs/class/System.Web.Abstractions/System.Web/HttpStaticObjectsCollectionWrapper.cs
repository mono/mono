//
// HttpStaticObjectsCollectionWrapper.cs
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
	public class HttpStaticObjectsCollectionWrapper : HttpStaticObjectsCollectionBase
	{
		HttpStaticObjectsCollection w;

		public HttpStaticObjectsCollectionWrapper (HttpStaticObjectsCollection httpStaticObjectsCollection)
		{
			if (httpStaticObjectsCollection == null)
				throw new ArgumentNullException ("httpStaticObjectsCollection");
			w = httpStaticObjectsCollection;
		}

		public override int Count {
			get { return w.Count; }
		}

		public override bool IsReadOnly {
			get { return w.IsReadOnly; }
		}

		public override bool IsSynchronized {
			get { return w.IsSynchronized; }
		}

		public override object this [string name] {
			get { return w [name]; }
		}

		public override bool NeverAccessed {
			get { return w.NeverAccessed; }
		}

		public override object SyncRoot {
			get { return w.SyncRoot; }
		}

		public override void CopyTo (Array array, int index)
		{
			w.CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return w.GetEnumerator ();
		}

		public override object GetObject (string name)
		{
			return w.GetObject (name);
		}

		public override void Serialize (BinaryWriter writer)
		{
			w.Serialize (writer);
		}
	}
}
