//
// System.Runtime.Remoting.Metadata.SoapAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Metadata {

	public class SoapAttribute : Attribute
	{
		public SoapAttribute ()
		{
		}

		protected string ProtXmlNamespace;
		protected object ReflectionInfo;

		[MonoTODO]
		public virtual bool Embedded {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual bool UseAttribute {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual string XmlNamespace {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}
	}
}
