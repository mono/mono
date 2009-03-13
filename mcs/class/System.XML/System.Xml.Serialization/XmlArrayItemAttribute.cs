//
// XmlArrayItemAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
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

using System.Xml.Schema;
using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlArrayItemAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
	public class XmlArrayItemAttribute : Attribute
	{
		private string dataType;
		private string elementName;
		private XmlSchemaForm form;
		private string ns;
		private bool isNullable = false, isNullableSpecified;
		private int nestingLevel;
		private Type type;

		public XmlArrayItemAttribute ()
		{
		}
		public XmlArrayItemAttribute (string elementName)
		{
			this.elementName = elementName;
		}

		public XmlArrayItemAttribute (Type type)
		{
			this.type = type;
		}

		public XmlArrayItemAttribute (string elementName, Type type)
		{
			this.elementName = elementName;
			this.type = type;
		}

		public string DataType {
			get {
				if (dataType == null) {
					return string.Empty;
				}
				return dataType;
			}
			set { dataType = value; }
		}

		public string ElementName {
			get {
				if (elementName == null) {
					return string.Empty;
				}
				return elementName;
			}
			set { elementName = value; }
		}

		public XmlSchemaForm Form {
			get { return form; }
			set { form = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public bool IsNullable {
			get { return isNullable; } 
			set {
				isNullableSpecified = true;
				isNullable = value;
			}
		}

		internal bool IsNullableSpecified {
			get { return isNullableSpecified; }
		}

		public Type Type {
			get { return type; }
			set { type = value; }
		}

		public int NestingLevel {
			get { return nestingLevel; }
			set { nestingLevel = value; }
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XAIA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, elementName);
			KeyHelper.AddField (sb, 3, form.ToString(), XmlSchemaForm.None.ToString());
			KeyHelper.AddField (sb, 4, isNullable, true);
			KeyHelper.AddField (sb, 5, dataType);
			KeyHelper.AddField (sb, 6, nestingLevel, 0);
			KeyHelper.AddField (sb, 7, type);
			sb.Append ('|');
		}
	}
}
