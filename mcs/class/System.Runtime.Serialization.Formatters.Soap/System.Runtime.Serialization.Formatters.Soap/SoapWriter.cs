// created on 07/04/2003 at 17:56
//
//	System.Runtime.Serialization.Formatters.Soap.SoapWriter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal class SoapWriter: ISoapWriter {
		public struct EnqueuedObject {
			public long _id;
			public object _object;
			
			public EnqueuedObject(object currentObject, long id) {
				_id = id;
				_object = currentObject;
			}
		}
		
		private IFormatProvider _format;
		private XmlTextWriter _xmlWriter;
		private ObjectWriter _objWriter;
		private Queue _objectQueue = new Queue();
		private long _objectCounter = 0;
		private long _refCounter = 0;
		private Hashtable _prefixTable = new Hashtable();
		private Stack _currentArrayType = new Stack();
		
		// id -> object
		private Hashtable _objectRefs = new Hashtable();
		
		// object -> id
		private Hashtable _objectIds = new Hashtable();
		
		private long _currentObjectId;
		
		private event DoneWithElementEventHandler Done;
		public event DoneWithElementEventHandler DoneWithElementEvent;
		public event DoneWithElementEventHandler DoneWithArray;
		public event DoneWithElementEventHandler GetRootInfo;
		
		internal SoapWriter(Stream outStream) {
			// todo: manage the encoding
			_xmlWriter = new XmlTextWriter(outStream, null);
			_xmlWriter.Formatting = Formatting.Indented;
			_xmlWriter.Indentation = 2;
//			_xmlWriter.WriteComment("My serialization function");
			_format = new CultureInfo("en-US");
		}
		
		~SoapWriter() {
		}
		
		private SoapSerializationEntry FillEntry(Type defaultObjectType, object objValue) {
			SoapTypeMapping mapping = GetTagInfo((objValue != null && defaultObjectType == typeof(object))?objValue.GetType():defaultObjectType);
			
			SoapSerializationEntry soapEntry = new SoapSerializationEntry();
			long id;
			
			GetPrefix(mapping.TypeNamespace, out soapEntry.prefix);
			soapEntry.elementName = mapping.TypeName;
			soapEntry.elementNamespace = mapping.TypeNamespace;
			soapEntry.CanBeValue = mapping.CanBeValue;
			
			soapEntry.getIntoFields = false;
			if(mapping.CanBeValue || mapping.IsArray) soapEntry.WriteFullEndElement = true;
			if(defaultObjectType == typeof(object) || _currentArrayType.Count > 0 && _currentArrayType.Peek() == typeof(System.Object)) 
				soapEntry.SpecifyEncoding = true;
			
			if(objValue == null){
				soapEntry.elementType = ElementType.Null;
				mapping.IsNull = true;
			} 
			else if(mapping.IsValueType) return soapEntry;
			else if(_objectIds[objValue] != null) {
				soapEntry.elementType = ElementType.Href;
				soapEntry.i = (long) _objectIds[objValue];
				soapEntry.WriteFullEndElement = false;
				soapEntry.SpecifyEncoding = false;
			}
			else if(!mapping.CanBeValue){
				id = GetNextId();
				soapEntry.i = id;
				soapEntry.elementType = ElementType.Href;
				soapEntry.WriteFullEndElement = false;
				_objectQueue.Enqueue(new EnqueuedObject(objValue, id));
				_objectRefs[id] = objValue;
				_objectIds[objValue] = id;
				soapEntry.SpecifyEncoding = false;
			} 
			else if(mapping.NeedId){ 
				id = GetNextId();
				soapEntry.i = id;
				soapEntry.elementType = ElementType.Id;
				_objectIds[objValue] = id;
			}
			
			return soapEntry;
		}
		
		private ICollection FormatteAttributes(SoapSerializationEntry entry) {
			
			string prefix;
			bool needNamespace;
			ArrayList attributeList = new ArrayList();
			
			// If the type of the element needs to be specified, do it there
			if(entry.SpecifyEncoding) {
				needNamespace = GetPrefix(entry.elementNamespace, out prefix);
				attributeList.Add(new SoapAttributeStruct("xsi", "type", prefix+":"+ entry.elementName));
				if(needNamespace) attributeList.Add(new SoapAttributeStruct("xmlns", prefix, entry.elementNamespace));
			}
			
			switch(entry.elementType) {
				case ElementType.Null:
					attributeList.Add(new SoapAttributeStruct("xsi", "null", "1"));
					return attributeList;
					//break;
				case ElementType.Id:
					attributeList.Add(new SoapAttributeStruct(null, "id", "ref-"+entry.i.ToString()));
					break;
				case ElementType.Href:
					attributeList.Add(new SoapAttributeStruct(null, "href", "#ref-"+entry.i.ToString()));
					return attributeList;
					//break;
			}
			
			// Add attributes about the type of the array items
			if(entry.IsArray) {
				Array array = (Array) _objectRefs[entry.i];
				Type elementType = array.GetType().GetElementType();
				SoapTypeMapping elementMapping = GetTagInfo(elementType);
				string rank = "[";
				for(int i=0; i < array.Rank; i++){
					rank += array.GetLength(i)+",";
				}
				rank = rank.Substring(0,rank.Length - 1);
				rank += "]";
				needNamespace = GetPrefix(elementMapping.TypeNamespace, out prefix);
				attributeList.Add(new SoapAttributeStruct("SOAP-ENC", "arrayType",prefix+":"+elementMapping.TypeName+rank));
				if(needNamespace) attributeList.Add(new SoapAttributeStruct("xmlns", prefix, elementMapping.TypeNamespace));
				
			}
			return attributeList;
		}
		
		private string GetNextObjectPrefix() {
			return "a"+(++_objectCounter);
		}
		
		private long GetNextId() {
			return ++_refCounter;
		}
		
		private bool GetPrefix(string xmlNamespace, out string prefix) {
			prefix = _xmlWriter.LookupPrefix(xmlNamespace);
		    if(prefix == null){ 
		    	prefix = (string)_prefixTable[xmlNamespace];
		    	if(prefix == null){
					prefix = GetNextObjectPrefix();
		    		_prefixTable[xmlNamespace] = prefix;
		    	}
		    	return true;
			}
			else
				return false;
		}
		
		private SoapTypeMapping GetTagInfo(Type tagType) {
			SoapTypeMapping mapping = SoapTypeMapper.GetSoapType(tagType);
			mapping.Href = 0;
			mapping.Id = 0;
			mapping.IsNull = false;
			
			return mapping;
			
		}
		
		public void WriteArrayItem(Type itemType, object itemValue) {
			Array currentArray = (Array) _objectRefs[_currentObjectId];
			SoapSerializationEntry soapEntry;
			
			soapEntry = FillEntry(itemType, itemValue);
			
			soapEntry.elementAttributes = FormatteAttributes(soapEntry);
			
			soapEntry.elementName = "item";
			soapEntry.prefix = null;
			soapEntry.elementNamespace = null;
			if(soapEntry.elementType != ElementType.Href && soapEntry.CanBeValue) soapEntry.elementValue = itemValue.ToString();
			else if(itemType.IsValueType){
				// the array item is a struct
				// so we have to serialize it now
				soapEntry.getIntoFields = true;
				soapEntry.elementValue = itemValue;
				Done = GetRootInfo;
			} 
			
			WriteElement(soapEntry);
		}
		
		public void WriteFields(SerializationInfo info) {
			ICollection attributeList;
			SoapSerializationEntry soapEntry;
			
			foreach(SerializationEntry entry in info){
				soapEntry = FillEntry(entry.ObjectType, entry.Value);
				
				
				attributeList = FormatteAttributes(soapEntry);
				soapEntry.elementValue = (soapEntry.elementType != ElementType.Href && soapEntry.CanBeValue)?entry.Value:null;
				
				soapEntry.elementAttributes = attributeList;
				soapEntry.elementName = entry.Name;
				
				soapEntry.elementNamespace = null;
				soapEntry.prefix = null;
				
				WriteElement(soapEntry);
				
				
			}
			
		}
		
		private void WriteElement(SoapSerializationEntry entry) {
			_xmlWriter.WriteStartElement(entry.prefix, XmlConvert.EncodeNmToken(entry.elementName), entry.elementNamespace);
			
			if (entry.elementAttributes != null) foreach(SoapAttributeStruct attr in entry.elementAttributes){
				_xmlWriter.WriteAttributeString(attr.prefix, attr.attributeName, null, attr.attributeValue);
			}
			if(entry.CanBeValue && entry.elementValue != null && !entry.getIntoFields){
				_xmlWriter.WriteString(String.Format(_format, "{0}", entry.elementValue));
			}
			if( entry.getIntoFields && Done != null) Done(this, new DoneWithElementEventArgs(entry.elementValue));			
			if(entry.WriteFullEndElement) {
				_xmlWriter.WriteFullEndElement();
			}else 
				_xmlWriter.WriteEndElement();
			
		}
		
		
		public void WriteRoot(object rootValue, Type rootType, bool getIntoFields) { //EnqueuedObject rootObject) {
			Done = DoneWithElementEvent;
			SoapSerializationEntry entry = FillEntry(rootType, rootValue); //new SoapSerializationEntry();
			if(rootType.IsArray) {
				Done = DoneWithArray;
				entry.IsArray = true;
			}
			
			entry.i = _currentObjectId;
			entry.elementType = ElementType.Id;
			ICollection attributeList = FormatteAttributes(entry);
			entry.elementAttributes = attributeList;
			
			entry.elementValue = rootValue;
			entry.getIntoFields = getIntoFields;
			entry.WriteFullEndElement = true;
			
			if(_currentArrayType.Count > 0 )
				Done(this, new DoneWithElementEventArgs(entry.elementValue));
			else
				WriteElement(entry);
			
		}
		
		
		public void Run() {
			WriteEnvelope();
			_xmlWriter.Flush();
		}
		
		public ObjectWriter Writer {
			get{ return _objWriter;}
			set{ _objWriter = value;}
		}
		
		public object TopObject {
			set {
				_objectQueue.Enqueue(new EnqueuedObject(value, _currentObjectId = GetNextId()));
				_objectRefs[_currentObjectId] = value;
				_objectIds[value] = _currentObjectId;
				// There isn't object with id="ref-2" in MS Soap messages
				// Wonder why
				// So I skip id=2
				GetNextId();
			}
		}
		
		private void WriteEnvelope() {
			ArrayList lstAttr = new ArrayList();
			_xmlWriter.WriteStartElement("SOAP-ENV", "Envelope",  "http://schemas.xmlsoap.org/soap/envelope/");
			
			_xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			_xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema" );
			_xmlWriter.WriteAttributeString("xmlns", "SOAP-ENC", null, "http://schemas.xmlsoap.org/soap/encoding/");
			_xmlWriter.WriteAttributeString("xmlns", "SOAP-ENV", null, "http://schemas.xmlsoap.org/soap/envelope/");
			_xmlWriter.WriteAttributeString("xmlns", "clr", null, "http://schemas.microsoft.com/soap/encoding/clr/1.0" );
			_xmlWriter.WriteAttributeString("SOAP-ENV", "encodingStyle", null, "http://schemas.xmlsoap.org/soap/encoding/");
			
			WriteBody();
			
			_xmlWriter.WriteEndElement();
			_xmlWriter.Flush();
		}
		
		private void WriteBody() {
			_xmlWriter.WriteStartElement("SOAP-ENV", "Body",  "http://schemas.xmlsoap.org/soap/envelope/");
			
			EnqueuedObject enqueuedObject;
			while(_objectQueue.Count > 0){
				enqueuedObject = (EnqueuedObject) _objectQueue.Dequeue();
				_currentObjectId = enqueuedObject._id;
				if(enqueuedObject._object is ISoapMessage)
					WriteSoapRPC((ISoapMessage) enqueuedObject._object);
				else
					GetRootInfo(this, new DoneWithElementEventArgs(enqueuedObject._object)); //WriteRoot(enqueuedObject);
			}
			
			_xmlWriter.WriteEndElement();
			
		}
		
		private void WriteSoapRPC(ISoapMessage soapMsg) {
			
			_xmlWriter.WriteStartElement("i2", soapMsg.MethodName, soapMsg.XmlNameSpace);
			_xmlWriter.WriteAttributeString("id", null, "ref-"+_currentObjectId);
			SerializationInfo info = new SerializationInfo(soapMsg.GetType(), new FormatterConverter());
			
			
			if(soapMsg.ParamNames != null) {
				for(int i=0; i<soapMsg.ParamNames.Length; i++){
					string name = soapMsg.ParamNames[i];
					object objValue = soapMsg.ParamValues[i];
					Type type = soapMsg.ParamTypes[i];
					info.AddValue(name, objValue, type);
				}
				WriteFields(info);
			}
			
			
			_xmlWriter.WriteEndElement();
			
		}
		
		public Stack CurrentArrayType {
			get { return _currentArrayType;}
		}
	}
}
