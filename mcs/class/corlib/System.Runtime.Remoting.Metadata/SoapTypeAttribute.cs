//
// System.Runtime.Remoting.Metadata.SoapTypeAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
			 AttributeTargets.Enum | AttributeTargets.Interface)]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapTypeAttribute : SoapAttribute
	{
		SoapOption _soapOption;
		bool _useAttribute;
		string _xmlElementName;
		XmlFieldOrderOption _xmlFieldOrder;
		string _xmlNamespace;
		string _xmlTypeName;
		string _xmlTypeNamespace;
		bool _isType;
		bool _isElement;
		
		public SoapTypeAttribute ()
		{
		}

		public SoapOption SoapOptions {
			get {
				return _soapOption;
			}

			set {
				_soapOption = value;
			}
		}

		public override bool UseAttribute {
			get {
				return _useAttribute;
			}

			set {
				_useAttribute = value;
			}
		}

		public string XmlElementName {
			get {
				return _xmlElementName;
			}

			set {
				_isElement = value != null;
				_xmlElementName = value;
			}
		}

		public XmlFieldOrderOption XmlFieldOrder {
			get {
				return _xmlFieldOrder;
			}

			set {
				_xmlFieldOrder = value;
			}
		}

		public override string XmlNamespace {
			get {
				return _xmlNamespace;
			}

			set {
				_isElement = value != null;
				_xmlNamespace = value;
			}
		}

		public string XmlTypeName {
			get {
				return _xmlTypeName;
			}

			set {
				_isType = value != null;
				_xmlTypeName = value;
			}
		}

		public string XmlTypeNamespace {
			get {
				return _xmlTypeNamespace;
			}

			set {
				_isType = value != null;
				_xmlTypeNamespace = value;
			}
		}
		
		internal bool IsInteropXmlElement
		{
			get { return _isElement; }
		}
		
		internal bool IsInteropXmlType
		{
			get { return _isType; }
		}
		
		internal override void SetReflectionObject (object reflectionObject)
		{
			Type type = (Type) reflectionObject;
			
			if (_xmlElementName == null)
				_xmlElementName = type.Name;
				
			if (_xmlTypeName == null)
				_xmlTypeName = type.Name;
			
			if (_xmlTypeNamespace == null)
			{
				string na;
				if (type.Assembly == typeof (object).Assembly) na = string.Empty;
				else na = type.Assembly.GetName().Name;
				_xmlTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace (type.Namespace, na);
			}
			
			if (_xmlNamespace == null)
				_xmlNamespace = _xmlTypeNamespace;
		}
	}
}
