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
		public XsdIdentityPath ()
		{
		}

		public XsdIdentityStep [] OrderedSteps;
		public bool Descendants;

		public bool IsAttribute {
			get {
				return OrderedSteps.Length == 0 ?
					false :
					OrderedSteps [OrderedSteps.Length - 1].IsAttribute;
			}
		}
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
