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
		string value;

		public UriValueType (string value)
		{
			this.value = value;
		}

		public string Value {
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