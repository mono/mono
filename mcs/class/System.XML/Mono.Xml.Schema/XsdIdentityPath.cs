//
// Mono.Xml.Schema.XsdIdentityPath.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
//
// These classses represents XML Schema's fake XPath, which 
// W3C specification calls "XPath", although it is very different
// language from XPath.
//
//
using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Xml.Schema
{
	internal class XsdIdentitySelector
	{
		XsdIdentityPath [] selectorPaths;

		ArrayList fields = new ArrayList ();
		XsdIdentityField [] cachedFields;

		public XsdIdentitySelector (XmlSchemaXPath selector)
		{
			selectorPaths = selector.CompiledExpression;
		}

		public XsdIdentityPath [] Paths {
			get { return selectorPaths; }
		}

		public void AddField (XsdIdentityField field)
		{
			cachedFields = null;
			fields.Add (field);
		}

		public XsdIdentityField [] Fields {
			get {
				if (cachedFields == null)
					cachedFields = fields.ToArray (typeof (XsdIdentityField)) as XsdIdentityField [];
				return cachedFields;
			}
		}
	}

	internal class XsdIdentityField
	{
		XsdIdentityPath [] fieldPaths;
		int index;

		public XsdIdentityField (XmlSchemaXPath field, int index)
		{
			this.index = index;
			fieldPaths = field.CompiledExpression;
		}

		public XsdIdentityPath [] Paths {
			get { return fieldPaths; }
		}

		public int Index {
			get { return index; }
		}
	}

	internal class XsdIdentityPath
	{
		public XsdIdentityStep [] OrderedSteps;
		public bool Descendants;
		// For selectors, it should be always null
		public string AttributeName;
	}

	internal class XsdIdentityStep
	{
		public bool IsCurrent;
		public bool IsAttribute;
		public bool IsAnyName;
		public string NsName;
		public string Name;
		public string Namespace = "";
	}
}
