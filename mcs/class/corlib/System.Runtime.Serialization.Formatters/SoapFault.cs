//
// System.Runtime.Serialization.Formatters.SoapFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	public sealed class SoapFault : ISerializable
	{
		string code;
		string actor;
		string faultString;
		ServerFault serverFault;

		[MonoTODO]
		public SoapFault ()
		{
			throw new NotImplementedException ();
		}

		public SoapFault (string faultCode, string faultString,
				  string faultActor, ServerFault serverFault)
		{
			this.code = faultCode;
			this.actor = faultActor;
			this.faultString = faultString;
			this.serverFault = serverFault;
		}
		
		[MonoTODO]
		public object Detail {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string FaultActor {
			get { return actor; }
			set { actor = value; }
		}

		public string FaultCode {
			get { return code; }
			set { code = value; }
		}

		public string FaultString {
			get { return faultString; }
			set { faultString = value; }
		}
		
		public void GetObjectData (SerializationInfo info,
					   StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
