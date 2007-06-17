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
		//Encoding displayNameEncoding;

		#endregion // Fields

		#region Constructors

		public MailAddress (string address) : this (address, null)
		{
		}

		public MailAddress (string address, string displayName) : this (address, displayName, Encoding.Default)
		{
		}

		public MailAddress (string address, string displayName, Encoding displayNameEncoding)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			// either displayname is enclosed in quotes, and/or e-mail address
			// is enclosed in less than / greater than characters
			int quoteStart = address.IndexOf ('"');
			if (quoteStart == 0) {
				int quoteEnd = address.IndexOf ('"', quoteStart + 1);
				if (quoteEnd == -1)
					throw CreateFormatException ();
				this.displayName = address.Substring (quoteStart + 1, quoteEnd - 1).Trim ();
				address = address.Substring (quoteEnd + 1);
			}

			int addressStart = address.IndexOf ('<');
			if (addressStart != -1) {
				if (addressStart + 1 >= address.Length)
					throw CreateFormatException ();
				int addressEnd = address.IndexOf ('>', addressStart + 1);
				if (addressEnd == -1)
					throw CreateFormatException ();
				if (this.displayName == null)
					this.displayName = address.Substring (0, addressStart).Trim ();
				address = address.Substring (++addressStart, addressEnd - addressStart);
			}

			// LAMESPEC: zero-length displayName should not override display name
			// specified in address
			// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=283163
			if (displayName != null)
				this.displayName = displayName.Trim ();

			this.address = address.Trim ();
			//this.displayNameEncoding = displayNameEncoding;
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
			get { return Address.Substring (address.IndexOf ("@") + 1); }
		}

		public string User {
			get { return Address.Substring (0, address.IndexOf ("@")); }
		}

#endregion // Properties

#region Methods
		
		public override bool Equals (object obj)
		{
			return Equals (obj as MailAddress);
		}

		bool Equals (MailAddress other)
		{
			return other != null && Address == other.Address;
		}

		public override int GetHashCode ()
		{
			return address.GetHashCode ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			if (DisplayName != null && DisplayName.Length > 0) {
				sb.Append ("\"");
				sb.Append (DisplayName);
				sb.Append ("\"");
				sb.Append (" ");
				sb.Append ("<");
				sb.Append (Address);
				sb.Append (">");
			}
			else {
				sb.Append (Address);
			}

			return sb.ToString ();
		}

		private static FormatException CreateFormatException () {
			return new FormatException ("The specified string is not in the "
				+ "form required for an e-mail address.");
		}

#endregion // Methods
	}
}

#endif // NET_2_0
