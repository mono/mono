//
// XmlTypeMapMemberAttribute.cs: 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Xml.Schema;

namespace System.Xml.Serialization
{
	// XmlTypeMapMemberAttribute
	// A member of a class that must be serialized as an attribute

	internal class XmlTypeMapMemberAttribute: XmlTypeMapMember
	{
		string _attributeName;
		string _dataType;
		string _namespace = "";
		XmlSchemaForm _form;
		XmlTypeMapping _mappedType;

		public XmlTypeMapMemberAttribute()
		{
		}

		public string AttributeName
		{
			get { return _attributeName; }
			set { _attributeName = value; }
		}

		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}

		public string DataTypeNamespace
		{
			get 
			{ 
				if (_mappedType == null) return XmlSchema.Namespace;
				else return _mappedType.Namespace;
			}
		}

		public XmlSchemaForm Form 
		{
			get { return _form; }
			set { _form = value; }
		}

		public XmlTypeMapping MappedType
		{
			get { return _mappedType; }
			set { _mappedType = value; }
		}
	}
}
