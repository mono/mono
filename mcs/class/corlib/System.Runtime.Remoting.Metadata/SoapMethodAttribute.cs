//
// System.Runtime.Remoting.Metadata.SoapMethodAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class SoapMethodAttribute : SoapAttribute
	{
		string _responseElement;
		string _responseNamespace;
		string _returnElement;
		string _soapAction;
		bool _useAttribute;
		string _namespace;
		
		public SoapMethodAttribute ()
		{
		}

		public string ResponseXmlElementName {
			get {
				return _responseElement;
			}

			set {
				_responseElement = value;
			}
		}
		
		public string ResponseXmlNamespace {
			get {
				return _responseNamespace;
			}

			set {
				_responseNamespace = value;
			}
		}

		public string ReturnXmlElementName {
			get {
				return _returnElement;
			}

			set {
				_returnElement = value;
			}
		}

		public string SoapAction {
			get {
				return _soapAction;
			}

			set {
				_soapAction = value;
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

		public override string XmlNamespace {
			get {
				return _namespace;
			}

			set {
				_namespace = value;
			}
		}
		
		internal override void SetReflectionObject (object reflectionObject)
		{
			MethodBase mb = (MethodBase) reflectionObject;
			
			if (_responseElement == null)
				_responseElement = mb.Name + "Response";
				
			if (_responseNamespace == null)
				_responseNamespace = SoapServices.GetXmlNamespaceForMethodResponse (mb);
				
			if (_returnElement == null)
				_returnElement = "return";
				
			if (_soapAction == null) {
				_soapAction = SoapServices.GetXmlNamespaceForMethodCall (mb) + "#" + mb.Name;
			}
			
			if (_namespace == null)
				_namespace = SoapServices.GetXmlNamespaceForMethodCall (mb);
		}
	}
}
