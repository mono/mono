using System;
using System.Collections;
using System.Xml.Serialization;

namespace Mono.Doc.Core
{
	[XmlRoot(ElementName="monodoc")]
	public class MonodocFile
	{
		private string    language  = "en";
		private ArrayList types     = new ArrayList();

		public MonodocFile()
		{
		}

		[XmlAttribute(AttributeName="language")]
		public string Language
		{
			get { return language;  }
			set { language = value; }
		}

		// TODO: add structs and delegates as XmlArrayItems
		[XmlElement(Type = typeof(ClassDoc)), XmlElement(Type = typeof(InterfaceDoc))]
		public ArrayList Types
		{
			get { return types; }
		}
	}
}
