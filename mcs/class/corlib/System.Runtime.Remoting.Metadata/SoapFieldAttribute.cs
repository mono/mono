//
// System.Runtime.Remoting.Metadata.SoapFieldAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Metadata;

namespace System.Runtime.Remoting.Metadata {

	[AttributeUsage (AttributeTargets.Field)]
	public sealed class SoapFieldAttribute : SoapAttribute
	{
		public SoapFieldAttribute ()
		{
		}

		[MonoTODO]
		public int Order {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public string XmlElementName {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsInteropXmlElement ()
		{
			throw new NotImplementedException ();
		}
	}
}
