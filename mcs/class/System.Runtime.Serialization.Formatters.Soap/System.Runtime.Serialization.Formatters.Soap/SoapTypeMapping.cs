// created on 09/04/2003 at 19:01
//
//	System.Runtime.Serialization.Formatters.Soap.SoapTypeMapping
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	
	internal class SoapTypeMapping 
	{
		public static readonly string SoapEncodingNamespace = "http://schemas.xmlsoap.org/soap/encoding/";
		private string _typeNamespace;
		private string _typeName;
		private Type _type;
		private bool _isPrimitive;
		private bool _canBeValue;
		private bool _isValueType;
		private bool _needId;
		private int _id;
		private int _href;
		private bool _isNull;
		private bool _isArray;
		private bool _specifyEncoding;
		
		public bool IsArray {
			get {
				return _isArray;
			}
		}
		
		
		public bool IsNull {
			get {
				return _isNull;
			}
			set {
				_isNull = value;
			}
		}
		
		
		public int Id {
			get {
				return _id;
			}
			set {
				_id = value;
			}
		}
		public int Href {
			get {
				return _href;
			}
			set {
				_href = value;
			}
		}
		
		public bool NeedId {
			get {
				return _needId;
			}
		}
		
		
		
		internal SoapTypeMapping (Type type,
		                          string typeName,
		                          bool canBeValue,
		                          bool isPrimitive,
		                          bool isValueType,
		                          bool needId){
			_typeName = typeName;
			_type = type;
			_isPrimitive = isPrimitive;
			_isValueType = isValueType;
			_canBeValue = canBeValue;
			_needId = needId;
			_typeNamespace = SoapEncodingNamespace;
		    _id = 0;
		    _href = 0;
		    _isNull = false;
		    _isArray = type.IsArray;
		    _specifyEncoding = false;
		}
		
		internal SoapTypeMapping (Type type, 
		                          string typeName, 
		                          string typeNamespace, 
		                          bool canBeValue, 
		                          bool isPrimitive, 
		                          bool isValueType,
		                          bool needId): this(type, typeName, canBeValue, isPrimitive, isValueType, needId){
			_typeNamespace = typeNamespace;
		}
		
		internal SoapTypeMapping (string typeName, string typeNamespace) {
			_typeName = typeName;
			_typeNamespace = typeNamespace;
		}
		
		public override int GetHashCode() {
			int hashCode = _typeName.GetHashCode() + _typeNamespace.GetHashCode();
			return hashCode;
		}
		
		public override bool Equals(object obj) {
			if( ((SoapTypeMapping)obj)._typeName == _typeName && ((SoapTypeMapping)obj)._typeNamespace == _typeNamespace)
				return true;
			else
				return false;
			
		}
		
		public string TypeNamespace {
			get { return _typeNamespace;}
		}
		
		public string TypeName {
			get { return _typeName;}
		}
		
		public Type Type {
			get { return _type; }
		}
		
		public bool IsPrimitive {
			get { return _isPrimitive;}
		}
		
		public bool IsValueType {
			get { return _isValueType;}
		}
		
		public bool CanBeValue {
			get { return _canBeValue;}
		}
		
		public bool SpecifyEncoding {
			get {
				return _specifyEncoding;
			}
		}
		
	}
}
