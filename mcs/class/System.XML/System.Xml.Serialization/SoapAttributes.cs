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

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for SoapAttributes.
	/// </summary>
	public class SoapAttributes
	{
		private SoapAttributeAttribute soapAttribute;
		private object soapDefaultValue;

		public SoapAttributes ()
		{
		}
		
		[MonoTODO]
		public SoapAttributes (ICustomAttributeProvider provider)
		{
			throw new NotImplementedException ();
		}

		public SoapAttributeAttribute SoapAttribute 
		{
			get { 
				return soapAttribute; 
			} 
			set { 
				soapAttribute = value; 
			}
		}
		public object SoapDefaultValue {
			get { 
				return soapDefaultValue; 
			} 
			set {
				soapDefaultValue = value;
			}
		}

	}
}
