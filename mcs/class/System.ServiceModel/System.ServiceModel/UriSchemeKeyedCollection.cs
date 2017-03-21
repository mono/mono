//
// UriSchemeKeyedCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

namespace System.ServiceModel
{
	[MonoTODO ("It is untested.")]
	public class UriSchemeKeyedCollection
		: SynchronizedKeyedCollection<string, Uri>
	{
		public UriSchemeKeyedCollection (params Uri [] addresses)
			: base (new object ())
		{
			if (addresses == null)
				/* FIXME: masterinfo says, param name should be
				   baseAddresses */
				throw new ArgumentNullException ("addresses");

			for (int i = 0; i < addresses.Length; i ++) {
				if (!addresses [i].IsAbsoluteUri)
					throw new ArgumentException ("Only an absolute URI can be used as a base address");

				if (Contains (addresses [i].Scheme))
					throw new ArgumentException ("Collection already contains an address with scheme "+ addresses [i].Scheme);
				if (addresses [i].Query != String.Empty)
					throw new ArgumentException ("A base address cannot contain a query string.");

				InsertItem (i, addresses [i]);
			}
		}

		protected override string GetKeyForItem (Uri item)
		{
			return item.Scheme;
		}

		[MonoTODO ("hmm, what should I do further?")]
		protected override void InsertItem (int index, Uri item)
		{
			base.InsertItem (index, item);
		}

		[MonoTODO ("hmm, what should I do further?")]
		protected override void SetItem (int index, Uri item)
		{
			base.SetItem (index, item);
		}

		internal IList<Uri> InternalItems {
			get { return base.Items; }
		}
	}
}
