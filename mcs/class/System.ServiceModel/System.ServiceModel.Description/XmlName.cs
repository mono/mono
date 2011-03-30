//
// XmlName.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Xml;

namespace System.ServiceModel.Description
{
	// Introduced for silverlight/moonlight compatibility
	internal class XmlName
	{
		public XmlName (string name)
			: this (name, false)
		{
		}

		public XmlName (string name, bool allowNull)
		{
			ValidateEncodedName (name, allowNull);
			encoded_name = name;
		}

		string encoded_name, decoded_name;

		public bool IsEmpty {
			get { return String.IsNullOrEmpty (encoded_name); }
		}

		public string DecodedName {
			get {
				if (decoded_name == null && encoded_name != null)
					decoded_name = XmlConvert.DecodeName (encoded_name);
				return decoded_name;
			}
		}

		public string EncodedName {
			get { return encoded_name; }
		}

		public void ValidateEncodedName (string name, bool allowNull)
		{
			try {
				if (!allowNull || name != null)
					XmlConvert.VerifyNCName (name);
			} catch (XmlException ex) {
				throw new ArgumentException ("Invalid XML name", ex);
			}
		}

		public override string ToString ()
		{
			return encoded_name;
		}

		public static bool operator == (XmlName a, XmlName b)
		{
			throw new NotImplementedException ();
		}

		public static bool operator != (XmlName a, XmlName b)
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}
		
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

	}
}
