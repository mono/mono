//
// System.Data.SqlTypes.SqlXml
//
// Author:
//	Umadevi S (sumadevi@novell.com)
//	Veerapuram Varadhan  (vvaradhan@novell.com>)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Globalization;
using System.Threading;
using System.Xml.Serialization;
using System.Text;

#if NET_2_0

namespace System.Data.SqlTypes
{
	[SerializableAttribute]
	[XmlSchemaProvider ("GetXsdType")]
	public sealed class SqlXml : INullable, IXmlSerializable
	{
		bool notNull;
		string xmlValue;
		
		public SqlXml ()
		{
			notNull = false;
			xmlValue = null;
		}

		public SqlXml (Stream value)
		{
			if (value == null) {
				notNull = false;
				xmlValue = null;
			} else {
				int len = (int) value.Length;
				
				if (len < 1) {
					xmlValue = String.Empty;
				} else {
					int bufSize = 8192;
					StringBuilder sb = new StringBuilder (len);
		    		
					value.Position = 0;
	 				// Now read value into a byte buffer.
					byte [] bytes = null;
				
					if (len < bufSize)
						bufSize = len;
					bytes = new byte [bufSize];

					while (len > 0) {
						// Read may return anything from 0 to bufSize.
						int n = value.Read(bytes, 0, bufSize);
						sb.Append (Encoding.Unicode.GetString (bytes, 0, n));
					
						// The end of the file is reached.
						if (n==0)
						    break;
						len -= n;
					}
					xmlValue = sb.ToString ();
				}
				notNull = true;
			}
		}

		public SqlXml (XmlReader value)
		{
			if (value == null) {
				notNull = false;
				xmlValue = null;
			} else {
				if (value.Read ()) {
					value.MoveToContent ();
					xmlValue = value.ReadOuterXml();
				} else 
					xmlValue = String.Empty;
				notNull = true;
			}
		}

		public bool IsNull {
			get { return !notNull; }
		}

		public static SqlXml Null {
			get {
				return new SqlXml ();
			}
		}

		public string Value {
			get {
				if (notNull)
					return xmlValue;
				throw new SqlNullValueException ();
			}
		}

		public static XmlQualifiedName GetXsdType (XmlSchemaSet schemaSet)
		{
			XmlQualifiedName qualifiedName = new XmlQualifiedName ("anyType", "http://www.w3.org/2001/XMLSchema");
			return qualifiedName;
		}

		public XmlReader CreateReader ()
		{
			if (notNull) {
			   	XmlReaderSettings xs = new XmlReaderSettings ();
				xs.ConformanceLevel = ConformanceLevel.Fragment;
				return XmlTextReader.Create (new StringReader (xmlValue), xs);
			} else
				throw new SqlNullValueException (); 
		}

		[MonoTODO]
		XmlSchema IXmlSerializable.GetSchema ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.ReadXml (XmlReader r)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		void IXmlSerializable.WriteXml (XmlWriter writer) 
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
