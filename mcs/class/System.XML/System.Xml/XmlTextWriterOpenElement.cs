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

		string name;
		string xmlLang;
		XmlSpace xmlSpace;
		bool indentingOverriden = false;

		#endregion

		#region Constructors

		public XmlTextWriterOpenElement (string name)
		{
			this.name = name;
		}

		#endregion

		#region Properties

		public string Name 
		{
			get { return name; }
		}

		public bool IndentingOverriden 
		{
			get { return indentingOverriden; }
			set { indentingOverriden = value; }
		}

		public string XmlLang
		{
			get { return xmlLang; }
			set { xmlLang = value; }
		}

		public XmlSpace XmlSpace
		{
			get { return xmlSpace; }
			set { xmlSpace = value; }
		}

		#endregion

		#region Methods

		public override string ToString ()
		{
			return name;
		}

		#endregion
	}
}
