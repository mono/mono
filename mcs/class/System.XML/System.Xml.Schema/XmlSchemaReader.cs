
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

namespace System.Xml.Schema
{
	/// <summary>
	/// A wrapper around XmlReader
	/// </summary>
	internal class XmlSchemaReader : XmlReader, IXmlLineInfo
	{
		XmlReader reader;
		ValidationEventHandler handler;
		bool hasLineInfo;
		public XmlSchemaReader(XmlReader reader,ValidationEventHandler handler)
		{
			this.reader = reader;
			this.handler = handler;
			if(reader is IXmlLineInfo)
			{
				IXmlLineInfo info = (IXmlLineInfo)reader;
				hasLineInfo = info.HasLineInfo();
			}
		}

		/// <summary>
		/// Returns the Namespace:LocalName for the object
		/// </summary>
		public string FullName
		{
			get { return NamespaceURI + ":" + LocalName; }
		}

		public XmlReader Reader
		{
			get { return this.reader; }
		}

		public void RaiseInvalidElementError()
		{
			string errstr = "Element "+FullName + " is invalid in this context.\n";
			if(hasLineInfo)
				errstr += "The error occured on ("+((IXmlLineInfo)reader).LineNumber
					+","+((IXmlLineInfo)reader).LinePosition+")";
			XmlSchemaObject.error(handler, errstr, null);
			SkipToEnd();
		}
		/// <summary>
		/// Reads till the next Element or EndElement. Also checks that the Namespace of the element is
		/// Schema's Namespace.
		/// </summary>
		/// <returns></returns>
		public bool ReadNextElement()
		{
			MoveToElement();
			while(Read())
			{
				if(NodeType == XmlNodeType.Element || NodeType == XmlNodeType.EndElement)
				{
					if(reader.NamespaceURI != XmlSchema.Namespace)
					{
						RaiseInvalidElementError();
					}
					else
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Skips to the end of the current element
		/// </summary>
		public void SkipToEnd()
		{
			MoveToElement();
			if(IsEmptyElement || NodeType != XmlNodeType.Element)
				return;
			if(NodeType == XmlNodeType.Element)
			{
				int depth = Depth;
				while(Read())
				{
					if(Depth == depth)
						break;
				}
			}
			return;
		}

		#region LineInfo
		public bool HasLineInfo()
		{
			return hasLineInfo;
		}
		public int LineNumber
		{
			get { return hasLineInfo?((IXmlLineInfo)reader).LineNumber: 0; }
		}
		public int LinePosition
		{
			get { return hasLineInfo?((IXmlLineInfo)reader).LinePosition: 0; }
		}
		#endregion

		#region Delegates
		public override int AttributeCount 
		{
			get { return  reader.AttributeCount; }
		}
		public override string BaseURI 
		{
			get { return  reader.BaseURI; }
		}
		public override bool CanResolveEntity 
		{
			get { return  reader.CanResolveEntity; }
		}
		public override int Depth 
		{
			get { return  reader.Depth; }
		}
		public override bool EOF 
		{
			get { return  reader.EOF; }
		}
		public override bool HasAttributes 
		{
			get { return  reader.HasAttributes; }
		}
		public override bool HasValue 
		{
			get { return  reader.HasValue; }
		}
		public override bool IsDefault 
		{
			get { return  reader.IsDefault; }
		}
		public override bool IsEmptyElement 
		{
			get { return  reader.IsEmptyElement; }
		}
		public override string this[ int i ] 
		{
			get { return  reader[i]; }
		}
		public override string this[ string name ] 
		{
			get { return  reader[name]; }
		}
		public override string this[ string name, string namespaceURI ] 
		{
			get { return  reader[name,namespaceURI]; }
		}
		public override string LocalName 
		{
			get { return  reader.LocalName; }
		}
		public override string Name 
		{
			get { return  reader.Name; }
		}
		public override string NamespaceURI 
		{
			get { return  reader.NamespaceURI; }
		}
		public override XmlNameTable NameTable 
		{
			get { return  reader.NameTable; }
		}
		public override XmlNodeType NodeType 
		{
			get { return  reader.NodeType; }
		}
		public override string Prefix 
		{
			get { return  reader.Prefix; }
		}
		public override char QuoteChar 
		{
			get { return  reader.QuoteChar; }
		}
		public override ReadState ReadState 
		{
			get { return  reader.ReadState; }
		}
		public override string Value 
		{
			get { return  reader.Value; }
		}
		public override string XmlLang 
		{
			get { return  reader.XmlLang; }
		}
		public override XmlSpace XmlSpace 
		{
			get { return  reader.XmlSpace; }
		}

		public override void Close()
		{
			reader.Close(); 
		}

		public override bool Equals(object obj)
		{
			return reader.Equals(obj); 
		}

		public override string GetAttribute(int i)
		{
			return reader.GetAttribute(i); 
		}

		public override string GetAttribute(string name)
		{
			return reader.GetAttribute(name); 
		}

		public override string GetAttribute(string name, string namespaceURI)
		{
			return reader.GetAttribute(name, namespaceURI); 
		}

		public override int GetHashCode()
		{
			return reader.GetHashCode(); 
		}

		public override bool IsStartElement()
		{
			return reader.IsStartElement(); 
		}

		public override bool IsStartElement(string localname, string ns)
		{
			return reader.IsStartElement(localname, ns); 
		}

		public override bool IsStartElement(string name)
		{
			return reader.IsStartElement(name); 
		}

		public override string LookupNamespace(string prefix)
		{
			return reader.LookupNamespace(prefix); 
		}

		public override void MoveToAttribute(int i)
		{
			reader.MoveToAttribute(i); 
		}

		public override bool MoveToAttribute(string name)
		{
			return reader.MoveToAttribute(name); 
		}

		public override bool MoveToAttribute(string name, string ns)
		{
			return reader.MoveToAttribute(name,ns); 
		}

		public override System.Xml.XmlNodeType MoveToContent()
		{
			return reader.MoveToContent(); 
		}

		public override bool MoveToElement()
		{
			return reader.MoveToElement(); 
		}

		public override bool MoveToFirstAttribute()
		{
			return reader.MoveToFirstAttribute(); 
		}

		public override bool MoveToNextAttribute()
		{
			return reader.MoveToNextAttribute(); 
		}

		public override bool Read()
		{
			return reader.Read(); 
		}

		public override bool ReadAttributeValue()
		{
			return reader.ReadAttributeValue(); 
		}

		public override string ReadElementString()
		{
			return reader.ReadElementString(); 
		}

		public override string ReadElementString(string localname, string ns)
		{
			return reader.ReadElementString(localname, ns); 
		}

		public override string ReadElementString(string name)
		{
			return reader.ReadElementString(name); 
		}

		public override void ReadEndElement()
		{
			reader.ReadEndElement(); 
		}

		public override string ReadInnerXml()
		{
			return reader.ReadInnerXml(); 
		}

		public override string ReadOuterXml()
		{
			return reader.ReadOuterXml(); 
		}

		public override void ReadStartElement()
		{
			reader.ReadStartElement(); 
		}

		public override void ReadStartElement(string localname, string ns)
		{
			reader.ReadStartElement(localname, ns); 
		}

		public override void ReadStartElement(string name)
		{
			reader.ReadStartElement(name); 
		}

		public override string ReadString()
		{
			return reader.ReadString(); 
		}

		public override void ResolveEntity()
		{
			reader.ResolveEntity(); 
		}

		public override void Skip()
		{
			reader.Skip(); 
		}

		public override string ToString()
		{
			return reader.ToString(); 
		}

		#endregion
	}
}
