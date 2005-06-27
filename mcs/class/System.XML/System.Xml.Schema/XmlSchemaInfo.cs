//
// XmlSchemaInfo.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell, Inc.
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

#if NET_2_0
namespace System.Xml.Schema
{
	[MonoTODO]
	public class XmlSchemaInfo : IXmlSchemaInfo
	{
		bool isDefault;
		bool isNil;
		XmlSchemaSimpleType memberType;
		XmlSchemaAttribute attr;
		XmlSchemaElement elem;
		XmlSchemaType type;
		XmlSchemaValidity validity;

		public XmlSchemaInfo ()
		{
		}

		internal XmlSchemaInfo (IXmlSchemaInfo info)
		{
			isDefault = info.IsDefault;
			isNil = info.IsNil;
			memberType = info.MemberType;
			attr = info.SchemaAttribute;
			elem = info.SchemaElement;
			type = info.SchemaType;
			validity = info.Validity;
		}

		[MonoTODO]
		public bool IsDefault {
			get { return isDefault; }
			set { isDefault = value; }
		}

		[MonoTODO]
		public bool IsNil {
			get { return isNil; }
			set { isNil = value; }
		}

		[MonoTODO]
		public XmlSchemaSimpleType MemberType {
			get { return memberType; }
			set { memberType = value; }
		}

		[MonoTODO]
		public XmlSchemaAttribute SchemaAttribute {
			get { return attr; }
			set { attr = value; }
		}

		[MonoTODO]
		public XmlSchemaElement SchemaElement {
			get { return elem; }
			set { elem = value; }
		}

		[MonoTODO]
		public XmlSchemaType SchemaType {
			get { return type; }
			set { type = value; }
		}

		[MonoTODO]
		public XmlSchemaValidity Validity {
			get { return validity; }
			set { validity = value; }
		}
	}
}
#endif
