//
// System.Net.Mail.MailAddress.cs
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

using System.Text;

namespace System.Net.Mail {
	public class MailAddress 
	{
		#region Fields

		string address;
		string displayName;
		Encoding displayNameEncoding;

		#endregion // Fields

		#region Constructors

		public MailAddress (string address)
		{
			this.address = address;
		}

		public MailAddress (string address, string displayName)
		{
			this.address = address;
			this.displayName = displayName;
		}

		public MailAddress (string address, string name, Encoding displayNameEncoding)
		{
			this.address = address;
			this.displayName = displayName;
			this.displayNameEncoding = displayNameEncoding;
		}

		#endregion // Constructors

		#region Properties

		public string Address {	
			get { return address; }
		}

		public string DisplayName {
			get { return displayName; }
		}

		public string Host {
			get { return Address.Substring (address.IndexOf ("@") + 1); }
		}

		public string User {
			get { return Address.Substring (0, address.IndexOf ("@")); }
		}

		#endregion // Properties

		#region Methods

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			if (DisplayName != null && DisplayName.Length > 0) {
				sb.Append (DisplayName);
				sb.Append (" ");
			}
			sb.Append ("<");
			sb.Append (Address);
			sb.Append (">");

			return sb.ToString ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
