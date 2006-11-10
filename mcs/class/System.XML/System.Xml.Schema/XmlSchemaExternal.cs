// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaExternal.
	/// </summary>
	public abstract class XmlSchemaExternal : XmlSchemaObject
	{
		private string id;
		private XmlSchema schema;
		private string location;
		private XmlAttribute[] unhandledAttributes;

		protected XmlSchemaExternal()
		{}
		
		[System.Xml.Serialization.XmlAttribute("schemaLocation", DataType="anyURI")]
		public string SchemaLocation 
		{
			get{ return  location; } 
			set{ location = value; }
		}

		[XmlIgnore]
		public XmlSchema Schema 
		{
			get{ return  schema; }
			set{ schema = value; }
		}

		[System.Xml.Serialization.XmlAttribute("id", DataType="ID")]
		public string Id 
		{
			get{ return  id; }
			set{ id = value; }
		}

		[XmlAnyAttribute]
		public XmlAttribute[] UnhandledAttributes 
		{
			get
			{
				if(unhandledAttributeList != null)
				{
					unhandledAttributes = (XmlAttribute[]) unhandledAttributeList.ToArray(typeof(XmlAttribute));
					unhandledAttributeList = null;
				}
				return unhandledAttributes;
			}
			set
			{ 
				unhandledAttributes = value; 
				unhandledAttributeList = null;
			}
		}
	}
}
