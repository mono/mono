// 
// System.Xml.Serialization.XmlReflectionMember 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace System.Xml.Serialization {
	public class XmlReflectionMember {

		#region Fields

		bool isReturnValue;
		string memberName;
		Type memberType;
		bool overrideIsNullable;
#if !MOONLIGHT
		SoapAttributes soapAttributes;
#endif
		XmlAttributes xmlAttributes;
		Type declaringType;

		#endregion

		#region Constructors

		public XmlReflectionMember ()
		{
		}

		internal XmlReflectionMember (string name, Type type, XmlAttributes attributes)
		{
			memberName = name;
			memberType = type;
			xmlAttributes = attributes;
		}

#if !MOONLIGHT
		internal XmlReflectionMember (string name, Type type, SoapAttributes attributes)
		{
			memberName = name;
			memberType = type;
			soapAttributes = attributes;
		}
#endif

		#endregion // Constructors

		#region Properties

		public bool IsReturnValue {
			get { return isReturnValue; }
			set { isReturnValue = value; }
		}

		public string MemberName {
			get { return memberName; }
			set { memberName = value; }
		}

		public Type MemberType {
			get { return memberType; }
			set { memberType = value; }
		}

		public bool OverrideIsNullable {
			get { return overrideIsNullable; }
			set { overrideIsNullable = value; }
		}

#if !MOONLIGHT
		public SoapAttributes SoapAttributes {
			get { 
				if (soapAttributes == null) soapAttributes = new SoapAttributes();
				return soapAttributes; 
			}
			set { soapAttributes = value; }
		}
#endif

		public XmlAttributes XmlAttributes {
			get { 
				if (xmlAttributes == null) xmlAttributes = new XmlAttributes();
				return xmlAttributes; 
			}
			set { xmlAttributes = value; }
		}
		
		internal Type DeclaringType {
			get { return declaringType; }
			set { declaringType = value; }
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XRM ");
			KeyHelper.AddField (sb, 1, isReturnValue);
			KeyHelper.AddField (sb, 1, memberName);
			KeyHelper.AddField (sb, 1, memberType);
			KeyHelper.AddField (sb, 1, overrideIsNullable);
			
#if !MOONLIGHT
			if (soapAttributes != null)
				soapAttributes.AddKeyHash (sb);
#endif
			
			if (xmlAttributes != null)
				xmlAttributes.AddKeyHash (sb);
			
			sb.Append ('|');
		}

		#endregion // Properties
	}
}
