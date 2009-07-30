//
// HttpFileCollectionBase.cs
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
	public abstract class HttpFileCollectionBase : NameObjectCollectionBase, ICollection, IEnumerable
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual string [] AllKeys { get { NotImplemented (); return null; } }

		public override int Count {
			get { NotImplemented (); return 0; }
		}

		public virtual bool IsSynchronized { get { NotImplemented (); return false; } }

		public virtual HttpPostedFileBase this [int index] {
			get { return Get (index); }
		}

		public virtual HttpPostedFileBase this [string name] {
			get { return Get (name); }
		}

		public virtual object SyncRoot { get { NotImplemented (); return null; } }

		public virtual void CopyTo (Array dest, int index)
		{
			NotImplemented ();
		}

		public virtual HttpPostedFileBase Get (int index)
		{
			NotImplemented ();
			return null;
		}

		public virtual HttpPostedFileBase Get (string name)
		{
			NotImplemented ();
			return null;
		}

		public override IEnumerator GetEnumerator ()
		{
			NotImplemented ();
			return null;
		}

		public virtual string GetKey (int index)
		{
			NotImplemented ();
			return null;
		}
	}
}
