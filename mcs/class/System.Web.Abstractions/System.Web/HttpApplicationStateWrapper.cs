//
// HttpApplicationStateWrapper.cs
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
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpApplicationStateWrapper : HttpApplicationStateBase
	{
		HttpApplicationState w;

		public HttpApplicationStateWrapper (HttpApplicationState httpApplicationState)
		{
			if (httpApplicationState == null)
				throw new ArgumentNullException ("httpApplicationState");
			w = httpApplicationState;
		}

		public override string [] AllKeys {
			get { return w.AllKeys; }
		}

		public override HttpApplicationStateBase Contents {
			get { return new HttpApplicationStateWrapper (w.Contents); }
		}

		public override int Count {
			get { return w.Count; }
		}

		public override bool IsSynchronized {
			get { return ((ICollection) this).IsSynchronized; }
		}

		public override object this [int index] {
			get { return Get (index); }
		}

		public override object this [string name] {
			get { return Get (name); }
			set { Set (name, value); }
		}

		public override NameObjectCollectionBase.KeysCollection Keys {
			get { return w.Keys; }
		}

		public override HttpStaticObjectsCollectionBase StaticObjects {
			get { return new HttpStaticObjectsCollectionWrapper (w.StaticObjects); }
		}

		public override object SyncRoot {
			get { return ((ICollection) this).SyncRoot; }
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
			((ICollection) this).CopyTo (array, index);
		}

		public override object Get (int index)
		{
			return w.Get (index);
		}

		public override object Get (string name)
		{
			return w.Get (name);
		}

		public override IEnumerator GetEnumerator ()
		{
			return w.GetEnumerator ();
		}

		public override string GetKey (int index)
		{
			return w.GetKey (index);
		}

		[MonoTODO]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			w.GetObjectData (info, context);

			throw new NotImplementedException ();
		}

		public override void Lock ()
		{
			w.Lock ();
		}

		public override void OnDeserialization (object sender)
		{
			w.OnDeserialization (sender);
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

		public override void Set (string name, object value)
		{
			w.Set (name, value);
		}

		public override void UnLock ()
		{
			w.UnLock ();
		}
	}
}
