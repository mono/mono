using System;

#if NET_2_1

namespace System.Xml
{
	// note that they do not exist in SL
	public class XmlSchemaDatatype
	{
		public object ParseValue (string s, object o1, object o2)
		{
			throw new NotImplementedException ();
		}

		public string Normalize (string s)
		{
			throw new NotImplementedException ();
		}

		public XmlTokenizedType TokenizedType {
			get { throw new NotImplementedException (); }
		}

		public static XmlSchemaDatatype FromName (string name)
		{
			throw new NotImplementedException ();
		}
	}

	internal class XmlSchemaAttribute
	{
	}
}

#endif
