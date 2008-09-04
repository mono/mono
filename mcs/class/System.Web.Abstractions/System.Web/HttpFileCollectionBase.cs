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
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;

namespace System.Web
{
	[MonoTODO]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpFileCollectionBase : NameObjectCollectionBase, ICollection, IEnumerable
	{
		[MonoTODO]
		public virtual string [] AllKeys { get; private set; }
		[MonoTODO]
		public override int Count {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual bool IsSynchronized { get; private set; }
		[MonoTODO]
		public virtual HttpPostedFileBase this [int index] {
			get { return Get (index); }
		}
		[MonoTODO]
		public virtual HttpPostedFileBase this [string name] {
			get { return Get (name); }
		}
		[MonoTODO]
		public virtual object SyncRoot { get; private set; }

		[MonoTODO]
		public virtual void CopyTo (Array dest, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual HttpPostedFileBase Get (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual HttpPostedFileBase Get (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetKey (int index)
		{
			throw new NotImplementedException ();
		}
	}
}
