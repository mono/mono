// created on 16/04/2003 at 17:27
//
//	Contains classes, structs, delegates, enums used
//  by the other classes
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Collections;

namespace System.Runtime.Serialization.Formatters.Soap {
	internal delegate void SoapElementReadEventHandler(ISoapParser sender, SoapElementReadEventArgs e);
	
	internal class SoapElementReadEventArgs: EventArgs {
		private Queue _elementQueue;
		
		public SoapElementReadEventArgs(Queue elementQueue) {
			_elementQueue = elementQueue;
		}
		
		public Queue ElementQueue {
			get { return _elementQueue;}
		}
	}
	
	
	internal enum ElementType {
		Id, Href, Null, Nothing
	}
	internal struct ElementInfo {
		public Type _type;
		public object _value;
		public ElementType _elementType;
		public long _i;
		public string _name;
		public int[] _arrayDims;
		
		public ElementInfo (Type objType, string name, object objValue, ElementType elementType, long i, int[] arrayDims) {
			_name = name;
			_type = objType;
			_value = objValue;
			_elementType = elementType;
			_i = i;
			_arrayDims = arrayDims;
		}
	}
	internal delegate void ElementReadEventHandler(ISoapReader sender, ElementReadEventArgs e);
	
	internal class ElementReadEventArgs: EventArgs {
		private ElementInfo _rootElement;
		private Queue _fieldsInfo;
		
		public ElementReadEventArgs(ElementInfo rootElement, Queue fieldsInfo) {
			_rootElement = rootElement;
			_fieldsInfo = fieldsInfo;
		}
		
		public ElementInfo RootElement {
			get { return _rootElement;}
			set { _rootElement = value;}
		}
		
		public Queue FieldsInfo {
			get { return _fieldsInfo; }
		}
	}
	
	internal struct SoapAttributeStruct {
		public string prefix;
		public string attributeName;
		public string attributeValue;
		
		public SoapAttributeStruct(string prefix, string attributeName, string attributeValue) {
			this.prefix = prefix;
			this.attributeName = attributeName;
			this.attributeValue = attributeValue;
		}
	}
	
	internal class SoapSerializationEntry {
		public string prefix;
		public string elementName;
		public string elementNamespace;
		public ICollection elementAttributes;
		public object elementValue = null;
		public bool getIntoFields = false;
		public bool SpecifyEncoding = false;
		public long i = 0;
		public ElementType elementType = ElementType.Nothing;
		public bool IsArray = false;
		public bool CanBeValue = false;
		public bool WriteFullEndElement = false;
	}
	
	internal delegate void DoneWithElementEventHandler(ISoapWriter sender, DoneWithElementEventArgs e);
	
	internal class DoneWithElementEventArgs: EventArgs {
		private object _objCurrent; 

		public DoneWithElementEventArgs(object objCurrent) {
			_objCurrent = objCurrent;
		}
		
		public object Current {
			get { return _objCurrent;}
		}
		
		
	}
	

}
