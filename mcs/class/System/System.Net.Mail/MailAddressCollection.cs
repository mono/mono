//
// System.Net.Mail.MailAddressCollection.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Net.Mail {
	[CLSCompliant (false)]
	public class MailAddressCollection : List<MailAddress>, ICollection, IEnumerable, IList, ICollection<MailAddress>, IEnumerable<MailAddress>, IList<MailAddress>
	{
		#region Constructors

		internal MailAddressCollection ()
		{
		}

		public MailAddressCollection (string addresses)
		{
			AddAddresses (addresses.Split (new char [1] {','}));
		}

		public MailAddressCollection (string[] addresses)
		{
			AddAddresses (addresses);
		}

		#endregion

		#region Methods

		private void AddAddresses (string[] addresses)
		{
			foreach (string address in addresses)
				Add (new MailAddress (address));
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < Count; i += 1) {
				if (i > 0)
					sb.Append (", ");
				sb.Append (this [i].ToString ());
			}
			return sb.ToString ();
		}

		#endregion
	}
}

#endif
