//
// HttpFileCollectionWrapper.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpFileCollectionWrapper : HttpFileCollectionBase
	{
		HttpFileCollection w;

		public HttpFileCollectionWrapper (HttpFileCollection httpFileCollection)
		{
			if (httpFileCollection == null)
				throw new ArgumentNullException ("httpFileCollection");
			w = httpFileCollection;
		}

		public override string [] AllKeys {
			get { return w.AllKeys; }
		}

		public override int Count {
			get { return w.Count; }
		}

		public override bool IsSynchronized {
			get { return ((ICollection) w).IsSynchronized; }
		}

		public override HttpPostedFileBase this [int index] {
			get { return Get (index); }
		}

		public override HttpPostedFileBase this [string name] {
			get { return Get (name); }
		}

		public override NameObjectCollectionBase.KeysCollection Keys {
			get { return w.Keys; }
		}

		public override object SyncRoot {
			get { return ((ICollection) w).SyncRoot; }
		}

		public override void CopyTo (Array dest, int index)
		{
			w.CopyTo (dest, index);
		}

		public override HttpPostedFileBase Get (int index)
		{
			HttpPostedFile file = w.Get (index);
			if (file == null)
				return null;

			return new HttpPostedFileWrapper (file);
		}

		public override HttpPostedFileBase Get (string name)
		{
			HttpPostedFile file = w.Get (name);
			if (file == null)
				return null;

			return new HttpPostedFileWrapper (file);
		}

		public override IEnumerator GetEnumerator ()
		{
			return w.GetEnumerator ();
		}

		public override string GetKey (int index)
		{
			return w.GetKey (index);
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			w.GetObjectData (info, context);
		}

		public override void OnDeserialization (object sender)
		{
			w.OnDeserialization (sender);
		}
	}
}
