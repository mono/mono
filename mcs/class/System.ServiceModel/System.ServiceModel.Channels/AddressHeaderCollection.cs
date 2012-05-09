//
// System.ServiceModel.AddressHeaderCollection.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	public sealed class AddressHeaderCollection : ReadOnlyCollection<AddressHeader>
	{
		static readonly AddressHeader [] empty = new AddressHeader [0];

		static IList<AddressHeader> GetList (IEnumerable<AddressHeader> arg)
		{
			IList<AddressHeader> list = arg as IList<AddressHeader>;
			return list != null ? list : new List<AddressHeader> (arg);
		}

		public AddressHeaderCollection ()
			: base (empty)
		{
		}

		public AddressHeaderCollection (IEnumerable<AddressHeader> headers)
			: base (GetList (headers))
		{
		}

		public void AddHeadersTo (Message message)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			foreach (AddressHeader header in this)
				message.Headers.Add (header.ToMessageHeader ());
		}

		public AddressHeader FindHeader (string name, string ns)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (ns == null)
				throw new ArgumentNullException ("ns");
			foreach (AddressHeader header in this)
				if (header.Name == name && header.Namespace == ns)
					return header;

			return null;
		}

		public AddressHeader[] FindAll (string name, string ns)
		{
			throw new NotImplementedException ();
		}
	}
}