//
// System.Runtime.Serialization.Formatters.SoapMessage.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	public class SoapMessage : ISoapMessage
	{
		[MonoTODO]
		public SoapMessage ()
		{
			throw new NotImplementedException ();
		}

		public Header[] Headers {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string MethodName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string [] ParamNames {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public Type [] ParamTypes {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public object [] ParamValues {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string XmlNameSpace {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
