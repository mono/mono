//
// XmlAttributeAttribute.cs: 
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
	/// Summary description for XmlAttributeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlAttributeAttribute : Attribute
	{
		private string attributeName;
		private string dataType;
		private Type type;
		private XmlSchemaForm form;
		private string ns;

		public XmlAttributeAttribute ()
		{
		}

		public XmlAttributeAttribute (string attributeName)
		{
			AttributeName = attributeName;
		}
		
		public XmlAttributeAttribute (Type type)
		{
			Type = type;
		}

		public XmlAttributeAttribute (string attributeName, Type type)
		{
			AttributeName = attributeName;
			Type = type;
		}

		public string AttributeName {
			get {
				return attributeName;
			}
			set {
				attributeName = value;
			}
		}
		public string DataType {
			get {
				return dataType;
			}
			set {
				dataType = value;
			}
		}
		public XmlSchemaForm Form {
			get {
				return form;
			}
			set {
				form = value;
			}
		}
		public string Namespace {
			get {
				return ns;
			}
			set {
				ns = value;
			}
		}

		public Type Type
		{
			get 
			{
				return type;
			}
			set 
			{
				type = value;
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XAA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, attributeName);
			KeyHelper.AddField (sb, 3, form.ToString(), XmlSchemaForm.None.ToString());
			KeyHelper.AddField (sb, 4, dataType);
			KeyHelper.AddField (sb, 5, type);
			sb.Append ('|');
		}
	}
}
