//
// SoapAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System.Reflection;
using System;
using System.ComponentModel;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for SoapAttributes.
	/// </summary>
	public class SoapAttributes
	{
		private SoapAttributeAttribute soapAttribute;
		private object soapDefaultValue;
		private SoapElementAttribute soapElement;
		private SoapEnumAttribute soapEnum;
		private bool soapIgnore;
		private SoapTypeAttribute soapType;

		public SoapAttributes ()
		{
		}
		
		public SoapAttributes (ICustomAttributeProvider provider)
		{
			object[] attributes = provider.GetCustomAttributes(false);
			foreach(object obj in attributes)
			{
				if(obj is SoapAttributeAttribute)
					soapAttribute = (SoapAttributeAttribute) obj;
				else if(obj is DefaultValueAttribute)
					soapDefaultValue = obj;
				else if(obj is SoapElementAttribute)
					soapElement = (SoapElementAttribute) obj;
				else if(obj is SoapEnumAttribute)
					soapEnum = (SoapEnumAttribute) obj;
				else if(obj is SoapIgnoreAttribute)
					soapIgnore = true;
				else if(obj is SoapTypeAttribute)
					soapType = (SoapTypeAttribute) obj;
			}
		}

		public SoapAttributeAttribute SoapAttribute 
		{
			get { return  soapAttribute; } 
			set { soapAttribute = value; }
		}

		public object SoapDefaultValue 
		{
			get { return  soapDefaultValue; } 
			set { soapDefaultValue = value; }
		}

		public SoapElementAttribute SoapElement 
		{
			get { return  soapElement; } 
			set { soapElement = value; }
		}

		public SoapEnumAttribute SoapEnum 
		{
			get { return  soapEnum; } 
			set { soapEnum = value; }
		}

		public bool SoapIgnore
		{
			get { return  soapIgnore; } 
			set { soapIgnore = value; }
		}

		public SoapTypeAttribute SoapType 
		{
			get { return  soapType; } 
			set { soapType = value; }
		}
		
		internal bool InternalEquals (SoapAttributes other)
		{
			if (other == null) return false;
			if (soapIgnore != other.soapIgnore) return false;
			
			if (soapAttribute == null) {
				if (other.soapAttribute != null) return false; }
			else
				if (!soapAttribute.InternalEquals (other.soapAttribute)) return false;
				
			if (soapElement == null) {
				if (other.soapElement != null) return false; }
			else
				if (!soapElement.InternalEquals (other.soapElement)) return false;
				
			if (soapEnum == null) {
				if (other.soapEnum != null) return false; }
			else
				if (!soapEnum.InternalEquals (other.soapEnum)) return false;
				
			if (soapType == null) {
				if (other.soapType != null) return false; }
			else
				if (!soapType.InternalEquals (other.soapType)) return false;
				
			if (soapDefaultValue == null) {
				if (other.soapDefaultValue != null) return false; }
			else
				if (!soapDefaultValue.Equals (other.soapDefaultValue)) return false;
				
			return true;
		}	
	}
}
