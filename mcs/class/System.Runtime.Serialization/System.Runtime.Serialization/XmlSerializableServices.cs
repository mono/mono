#if NET_2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;

namespace System.Runtime.Serialization
{
	public static class XmlSerializableServices
	{
		static Dictionary<QName, XmlSchemaSet> defaultSchemas
			= new Dictionary<QName, XmlSchemaSet> ();

		public static void AddDefaultSchema (
			XmlSchemaSet schemas,
			QName typeQName)
		{
			throw new NotImplementedException ();
		}

		public static XmlNode [] ReadNodes (XmlReader xmlReader)
		{
			throw new NotImplementedException ();
		}

		public static void WriteNodes (XmlWriter xmlWriter,
			XmlNode [] nodes)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
