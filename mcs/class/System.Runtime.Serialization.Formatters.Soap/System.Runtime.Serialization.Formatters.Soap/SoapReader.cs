// created on 24/04/2003 at 15:35
//
//	System.Runtime.Serialization.Formatters.Soap.SoapReader
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters.Soap {
	internal class SoapReader: ISoapReader {
		public event ElementReadEventHandler ElementReadEvent;
		private ISoapMessage _soapMessage;
		private ISoapParser _parser;
		
		public ISoapMessage TopObject {
			get { return _soapMessage; }
			set { 
				_soapMessage = value;
				
				// the first element of the SOAP stream
				// should be a SOAP RPC
				_parser.SoapElementReadEvent -= new SoapElementReadEventHandler(SoapElementRead);
				_parser.SoapElementReadEvent += new SoapElementReadEventHandler(SoapRPCElementRead);
			}
		}
		
		public SoapReader(ISoapParser parser) {
			// register the SoapElementReadEvent handler
			_parser = parser;
			_parser.SoapElementReadEvent += new SoapElementReadEventHandler(SoapElementRead);
		}
		
		public void SoapRPCElementRead(ISoapParser sender, SoapElementReadEventArgs e) {
			_parser.SoapElementReadEvent += new SoapElementReadEventHandler(SoapElementRead);
			_parser.SoapElementReadEvent -= new SoapElementReadEventHandler(SoapRPCElementRead);
			
			Queue elementQueue = e.ElementQueue;
			Queue elementInfoQueue = new Queue();
			SoapSerializationEntry root = (SoapSerializationEntry) elementQueue.Dequeue();
			
			// fill the SoapMessage members
			// MethodName
			//elementInfoQueue.Enqueue(new ElementInfo(typeof(string), "MethodName", rpcInfo.elementName, ElementType.Nothing, 0, 0);
			_soapMessage.MethodName = root.elementName;
			
			// XmlNamespace
			//elementInfoQueue.Enqueue(new ElementInfo(typeof(string), "XmlNamespace", rpcInfo.elementNamespace, ElementType.Nothing, 0, 0);
			_soapMessage.XmlNameSpace = root.elementNamespace;
			
			// the root element is a SoapMessage
			ElementInfo rpcInfo = new ElementInfo(typeof(SoapMessage), String.Empty, _soapMessage, ElementType.Id, 1, null);
			
			//todo: headers
			
			// add the function parameters to the queue
			SoapSerializationEntry field;
			ElementInfo fieldElementInfo;
			while(elementQueue.Count > 0){
				field = (SoapSerializationEntry) elementQueue.Dequeue();
				fieldElementInfo = GetElementInfo(field);
				elementInfoQueue.Enqueue(fieldElementInfo);
			}

			// raise the ElementReadEvent
			ElementReadEvent(this,new ElementReadEventArgs(rpcInfo, elementInfoQueue));
			
			
		}
		
		// called when SoapElementReadEvent is raized by the SoapParser object
		public void SoapElementRead(ISoapParser sender, SoapElementReadEventArgs e) {
			Queue elementQueue = e.ElementQueue;
			Queue elementInfoQueue = new Queue();
			SoapSerializationEntry root = (SoapSerializationEntry) elementQueue.Dequeue();
			
			ElementInfo rootInfo = GetElementInfo(root);
			SoapSerializationEntry field;
			ElementInfo fieldElementInfo;
			while(elementQueue.Count > 0){
				field = (SoapSerializationEntry) elementQueue.Dequeue();
				fieldElementInfo = GetElementInfo(field);
				elementInfoQueue.Enqueue(fieldElementInfo);
			}

			// raise the ElementReadEvent
			ElementReadEvent(this,new ElementReadEventArgs(rootInfo, elementInfoQueue));
		}
		
		// Converts the text information from the SoapParser into
		// information that with by used by the ObjectReader to reconstruct
		// the object.
		private ElementInfo GetElementInfo(SoapSerializationEntry entry) {
			SoapTypeMapping mapping = new SoapTypeMapping(entry.elementName, entry.elementNamespace);
			Type elementType = SoapTypeMapper.GetType(mapping);
			
			
			long id = 0;
			ElementType elementId = ElementType.Nothing;
			ICollection attrLst = entry.elementAttributes;
			int[] arrayRank = null;
			foreach(SoapAttributeStruct attr in attrLst){
				if(attr.attributeName == "id"){
					string attrId = ((string)attr.attributeValue).Remove(0,4);
					id = (new FormatterConverter()).ToInt64(attrId);
					elementId = ElementType.Id;
				} else if(attr.attributeName == "href") {
					string attrId = ((string)attr.attributeValue).Remove(0,5);
					id = (new FormatterConverter()).ToInt64(attrId);
					elementId = ElementType.Href;
				} else if(attr.attributeName == "xsi:null" && attr.attributeValue == "1") {
					elementId = ElementType.Null;
				} else if(attr.attributeName == "SOAP-ENC:arrayType") {
					string[] tokens = attr.attributeValue.Split(new System.Char[] {'[',',',']'});
					mapping = new SoapTypeMapping(tokens[0], attr.prefix);
					elementType = SoapTypeMapper.GetType(mapping);
					arrayRank = new int[tokens.Length - 2];
					for(int i=0; i<tokens.Length-2; i++) {
						arrayRank[i] = Convert.ToInt32(tokens[i+1]);
					}
					Type tempType = Type.GetType(elementType.ToString()+"[]");
					
					if(tempType == null) {
						AssemblyName assName = elementType.Assembly.GetName();
						Assembly ass = Assembly.Load(assName);
						tempType = ass.GetType(elementType.ToString()+"[]", true);
					}
					
					elementType = tempType;
				} else if(attr.attributeName == "xsi:type") {
					mapping = new SoapTypeMapping(attr.attributeValue, attr.prefix);
					elementType = SoapTypeMapper.GetType(mapping);
					
					
				}
			}
			

			ElementInfo elementInfo =  new ElementInfo(elementType, entry.elementName, entry.elementValue, elementId, id, arrayRank);
			return elementInfo;
			
		}
	}
}
