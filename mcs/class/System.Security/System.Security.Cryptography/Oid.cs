//
// Oid.cs - System.Security.Cryptography.Oid
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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

namespace System.Security.Cryptography {

	public sealed class Oid {

		private string _value;
		private string _name;

		// constructors

		public Oid ()
		{
		}

		public Oid (string oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid;
			_name = GetName (oid);
		}

		public Oid (string value, string friendlyName)
		{
			_value = value;
			_name = friendlyName;
		}

		public Oid (Oid oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid.Value;
			_name = oid.FriendlyName;
		}

		// properties

		public string FriendlyName {
			get { return _name; }
			set { 
				_name = value;
				_value = GetValue (_name);
			}
		}

		public string Value { 
			get { return _value; }
			set { 
				_value = value; 
				_name = GetName (_value);
			}
		}

		// private methods

		// TODO - find the complete list
		private string GetName (string oid) 
		{
			if (oid == null)
				return null;
			switch (oid) {
				case "1.2.840.113549.1.1.1":
					return "RSA";
				case "1.2.840.113549.1.7.1":
					return "PKCS 7 Data";
				case "1.2.840.113549.1.9.5":
					return "Signing Time";
				case "1.2.840.113549.3.7":
					return "3des";
				default:
					return _name;
			}
		}

		// TODO - find the complete list
		private string GetValue (string name) 
		{
			if (name == null)
				return null;
			switch (name) {
				case "RSA":
					return "1.2.840.113549.1.1.1";
				case "PKCS 7 Data":
					return "1.2.840.113549.1.7.1";
				case "Signing Time":
					return "1.2.840.113549.1.9.5";
				case "3des":
					return "1.2.840.113549.3.7";
				default:
					return _value;
			}
		}
	}
}

#endif
