// created on 22/04/2003 at 19:55
//
//	System.Runtime.Serialization.Formatters.Soap.SoapParser
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//


using System.Collections;
using System.Xml;
using System.IO;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal class SoapParser: ISoapParser {
		private XmlTextReader _xmlReader;
		public SoapParser(Stream inStream) {
			_xmlReader = new XmlTextReader(inStream);
			_xmlReader.WhitespaceHandling = WhitespaceHandling.None;
		}
		
		public event SoapElementReadEventHandler SoapElementReadEvent;
		
		public void Run() {
			SoapSerializationEntry entry = null;
			ArrayList attributeList;
			Queue elementQueue = new Queue();
			// Read the envelope tag
			_xmlReader.Read();
			_xmlReader.MoveToContent();
			// Read the Body tag
			_xmlReader.Read();
			_xmlReader.MoveToContent();
			try{
				while(_xmlReader.Read() && _xmlReader.MoveToContent() != XmlNodeType.None){
					switch(_xmlReader.NodeType) {
						case XmlNodeType.Element:
							int elementDepth = _xmlReader.Depth;
							bool isEmptyElement = _xmlReader.IsEmptyElement;
							if(entry != null) elementQueue.Enqueue(entry);
							entry = new SoapSerializationEntry();
							entry.elementName = XmlConvert.DecodeName(_xmlReader.LocalName);
							entry.elementNamespace = _xmlReader.NamespaceURI;
							entry.prefix = _xmlReader.Prefix;
							attributeList = new ArrayList();
							while(_xmlReader.MoveToNextAttribute()) {
								// if the element is an array, we have to get the type
								// of the array
								if(_xmlReader.Name == "SOAP-ENC:arrayType" || _xmlReader.Name == "xsi:type") {
									string name = _xmlReader.Value;
									string[] tokens = name.Split(new System.Char[] {':'});
									string xmlNamespace = _xmlReader.LookupNamespace(tokens[0]);
									attributeList.Add(new SoapAttributeStruct(xmlNamespace, _xmlReader.Name, tokens[1]));
								}
								else {
									attributeList.Add(new SoapAttributeStruct(_xmlReader.Prefix, _xmlReader.Name, _xmlReader.Value));
								}
							}
							entry.elementAttributes = attributeList;
							if(isEmptyElement && elementDepth == 2 && SoapElementReadEvent != null) {
								elementQueue.Enqueue(entry);
								entry = null;
								SoapElementReadEvent(this, new SoapElementReadEventArgs(elementQueue));								
							}
							break;
						case XmlNodeType.EndElement:
							if(_xmlReader.Depth == 2 && SoapElementReadEvent != null){
								elementQueue.Enqueue(entry);
								entry = null;
								SoapElementReadEvent(this, new SoapElementReadEventArgs(elementQueue));
							}
							if(_xmlReader.Depth == 0 || _xmlReader.Name == "SOAP-ENV:Envelope") return;
							break;
						case XmlNodeType.Text:
							entry.elementValue = _xmlReader.Value;
							break;
						case XmlNodeType.Attribute:
							break;
					}
				}
			}
			finally {
				//if(_xmlReader != null) _xmlReader.Close();
			}
		}
		
		public Stream InStream {
			set { _xmlReader = new XmlTextReader(value);}
		}
	}
}
