
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
using Mono.Xml.Schema;

namespace System.Xml.Schema
{
	internal struct QNameValueType
	{
		XmlQualifiedName value;

		public QNameValueType (XmlQualifiedName value)
		{
			this.value = value;
		}

		public XmlQualifiedName Value {
			get { return value; }
		}

		public static bool operator == (QNameValueType v1, QNameValueType v2)
		{
			return v1.Value == v2.Value;
		}

		public static bool operator != (QNameValueType v1, QNameValueType v2)
		{
			return v1.Value != v2.Value;
		}

		public override bool Equals (object obj)
		{
			if (obj is QNameValueType)
				return (QNameValueType) obj == this;
			else
				return false;
		}

		public override int GetHashCode () 
		{
			return value.GetHashCode ();
		}
	}

	internal struct StringValueType
	{
		string value;

		public StringValueType (string value)
		{
			this.value = value;
		}

		public string Value {
			get { return value; }
		}

		public static bool operator == (StringValueType v1, StringValueType v2)
		{
			return v1.Value == v2.Value;
		}

		public static bool operator != (StringValueType v1, StringValueType v2)
		{
			return v1.Value != v2.Value;
		}

		public override bool Equals (object obj)
		{
			if (obj is StringValueType)
				return (StringValueType) obj == this;
			else
				return false;
		}

		public override int GetHashCode () 
		{
			return value.GetHashCode ();
		}
	}

	internal struct UriValueType
	{
		XmlSchemaUri value;

		public UriValueType (XmlSchemaUri value)
		{
			this.value = value;
		}

		public XmlSchemaUri Value {
			get { return value; }
		}

		public static bool operator == (UriValueType v1, UriValueType v2)
		{
			return v1.Value == v2.Value;
		}

		public static bool operator != (UriValueType v1, UriValueType v2)
		{
			return v1.Value != v2.Value;
		}

		public override bool Equals (object obj)
		{
			if (obj is UriValueType)
				return (UriValueType) obj == this;
			else
				return false;
		}

		public override int GetHashCode () 
		{
			return value.GetHashCode ();
		}

		public override string ToString ()
		{
			return value.ToString ();
		}
	}

	internal struct StringArrayValueType
	{
		string [] value;

		public StringArrayValueType (string [] value)
		{
			this.value = value;
		}

		public string [] Value {
			get { return value; }
		}

		public static bool operator == (StringArrayValueType v1, StringArrayValueType v2)
		{
			return v1.Value == v2.Value;
		}

		public static bool operator != (StringArrayValueType v1, StringArrayValueType v2)
		{
			return v1.Value != v2.Value;
		}

		public override bool Equals (object obj)
		{
			if (obj is StringArrayValueType)
				return (StringArrayValueType) obj == this;
			else
				return false;
		}

		public override int GetHashCode () 
		{
			return value.GetHashCode ();
		}
	}
}
