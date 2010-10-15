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
		string host;
		string user;
		string to_string;
		//Encoding displayNameEncoding;

		#endregion // Fields

		#region Constructors

		public MailAddress (string address) : this (address, null)
		{
		}

		public MailAddress (string address, string displayName) : this (address, displayName, Encoding.UTF8)
		{
		}

		[MonoTODO ("We don't do anything with displayNameEncoding")]
		public MailAddress (string address, string displayName, Encoding displayNameEncoding)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			if (address.Length == 0)
				throw new ArgumentException ("address");

			if (displayName != null)
				this.displayName = displayName.Trim ();
			ParseAddress (address);
		}

		void ParseAddress (string address)
		{
			// 1. Quotes for display name
			address = address.Trim ();
			int idx = address.IndexOf ('"');
			if (idx != -1) {
				if (idx != 0 || address.Length == 1)
					throw CreateFormatException ();

				int closing = address.LastIndexOf ('"');
				if (closing == idx)
					throw CreateFormatException ();

				if (this.displayName == null)
					this.displayName = address.Substring (idx + 1, closing - idx - 1).Trim ();
				address = address.Substring (closing + 1).Trim ();
			}

			// 2. <email>
			idx = address.IndexOf ('<');
			if (idx >= 0) {
				if (this.displayName == null)
					this.displayName = address.Substring (0, idx).Trim ();
				if (address.Length - 1 == idx)
					throw CreateFormatException ();

				int end = address.IndexOf ('>', idx + 1);
				if (end == -1)
					throw CreateFormatException ();

				address = address.Substring (idx + 1, end - idx - 1).Trim ();
			}
			this.address = address;
			// 3. email
			idx = address.IndexOf ('@');
			if (idx <= 0)
				throw CreateFormatException ();
			if (idx != address.LastIndexOf ('@'))
				throw CreateFormatException ();

			this.user = address.Substring (0, idx).Trim ();
			if (user.Length == 0)
				throw CreateFormatException ();
			this.host = address.Substring (idx + 1).Trim ();
			if (host.Length == 0)
				throw CreateFormatException ();
		}

		#endregion // Constructors

#region Properties

		public string Address {
			get { return address; }
		}

		public string DisplayName {
			get {
				if (displayName == null)
					return string.Empty;
				return displayName;
			}
		}

		public string Host {
			get { return host; }
		}

		public string User {
			get { return user; }
		}

#endregion // Properties

#region Methods
		
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			return (0 == String.Compare (ToString (), obj.ToString (), StringComparison.OrdinalIgnoreCase));
		}

		public override int GetHashCode ()
		{
			return ToString ().GetHashCode ();
		}

		public override string ToString ()
		{
			if (to_string != null)
				return to_string;

			if (!String.IsNullOrEmpty (displayName))
				to_string = String.Format ("\"{0}\" <{1}>", DisplayName, Address);
			else
				to_string = address;

			return to_string;
		}

		private static FormatException CreateFormatException () {
			return new FormatException ("The specified string is not in the "
				+ "form required for an e-mail address.");
		}

#endregion // Methods
	}
}

#endif // NET_2_0
