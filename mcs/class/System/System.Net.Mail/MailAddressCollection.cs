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

using System;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Net.Mail {
	public class MailAddressCollection : Collection<MailAddress>
	{
		#region Methods

		public void Add (string addresses)
		{
			foreach (string address in addresses.Split (','))
				Add (new MailAddress (address));
		}

		protected override void InsertItem (int index, MailAddress item)
		{
			if (item == null)
				throw new ArgumentNullException ();
			base.InsertItem (index, item);
		}
		
		protected override void SetItem (int index, MailAddress item)
		{
			if (item == null)
				throw new ArgumentNullException ();
			base.SetItem (index, item);
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

