//
// System.Runtime.Remoting.Metadata.SoapFieldAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Field)]
	public sealed class SoapFieldAttribute : SoapAttribute
	{
		int _order;
		string _elementName;
		
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
				_elementName = value;
			}
		}

		[MonoTODO]
		public bool IsInteropXmlElement ()
		{
			throw new NotImplementedException ();
		}
	}
}
