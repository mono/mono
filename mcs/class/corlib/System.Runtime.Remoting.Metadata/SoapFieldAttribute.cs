//
// System.Runtime.Remoting.Metadata.SoapFieldAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;
using System.Reflection;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Field)]
	public sealed class SoapFieldAttribute : SoapAttribute
	{
		int _order;
		string _elementName;
		bool _isElement = false;
		
		public SoapFieldAttribute ()
		{
		}

		public int Order {
			get {
				return _order;
			}

			set {
				_order = value;
			}
		}
		
		public string XmlElementName {
			get {
				return _elementName;
			}

			set {
				_isElement = value != null;
				_elementName = value;
			}
		}

		public bool IsInteropXmlElement ()
		{
			return _isElement;
		}
		
		internal override void SetReflectionObject (object reflectionObject)
		{
			FieldInfo f = (FieldInfo) reflectionObject;
			if (_elementName == null) _elementName = f.Name;
		}
	}
}
