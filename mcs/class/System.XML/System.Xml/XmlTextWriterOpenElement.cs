//
// System.Xml.XmlTextWriterOpenElement
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//
//
//	Scope support for XmlLang and XmlSpace in XmlTextWriter.
//
using System;

namespace System.Xml
{
	internal class XmlTextWriterOpenElement
	{
		#region Fields

		string prefix;
		string localName;
		string xmlLang;
		XmlSpace xmlSpace;
		bool indentingOverriden = false;

		#endregion

		#region Constructors

		public XmlTextWriterOpenElement (string prefix, string localName)
		{
			Reset (prefix, localName);
		}

		#endregion

		public void Reset (string prefix, string localName)
		{
			this.prefix = prefix;
			this.localName = localName;
		}

		#region Properties

		public string LocalName {
			get { return localName; }
		}

		public string Prefix {
			get { return prefix; }
		}

		public bool IndentingOverriden {
			get { return indentingOverriden; }
			set { indentingOverriden = value; }
		}

		public string XmlLang {
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace {
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		#endregion
	}
}
