//
// System.Runtime.Remoting.Metadata.SoapTypeAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
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
				_xmlNamespace = value;
			}
		}

		public string XmlTypeName {
			get {
				return _xmlTypeName;
			}

			set {
				_xmlTypeName = value;
			}
		}

		public string XmlTypeNamespace {
			get {
				return _xmlTypeNamespace;
			}

			set {
				_xmlTypeNamespace = value;
			}
		}
	}
}
