//
// System.Runtime.Remoting.Metadata.SoapTypeAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct |
			 AttributeTargets.Enum | AttributeTargets.Interface)]
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
