//
// SoapAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

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
		private object soapDefaultValue = System.DBNull.Value;
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
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("SA ");
			
			if (soapIgnore) 
				sb.Append ('i');
				
			if (soapAttribute != null)
				soapAttribute.AddKeyHash (sb);
				
			if (soapElement != null)
				soapElement.AddKeyHash (sb);
				
			if (soapEnum != null)
				soapEnum.AddKeyHash (sb);
				
			if (soapType != null)
				soapType.AddKeyHash (sb);
				
			if (soapDefaultValue == null) {
				sb.Append ("n");
			}
			else if (!(soapDefaultValue is System.DBNull)) {
				string v = XmlCustomFormatter.ToXmlString (TypeTranslator.GetTypeData (soapDefaultValue.GetType()), soapDefaultValue);
				sb.Append ("v" + v);
			}
			sb.Append ("|");
		}	
	}
}
