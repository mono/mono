//
// System.Runtime.Remoting.Metadata.SoapAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Metadata {

	public class SoapAttribute : Attribute
	{
		bool _nested;
		bool _useAttribute;
		string _namespace;
		object _reflectionObject;
		
		public SoapAttribute ()
		{
		}

		public virtual bool Embedded {
			get {
				return _nested;
			}

			set {
				_nested = value;
			}
		}

		public virtual bool UseAttribute {
			get {
				return _useAttribute;
			}

			set {
				_useAttribute = value;
			}
		}

		public virtual string XmlNamespace {
			get {
				return _namespace;
			}

			set {
				_namespace = value;
			}
		}
		
		internal virtual void SetReflectionObject (object reflectionObject)
		{
		}
	}
}
