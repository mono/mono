//
// System.Runtime.Serialization.Formatters.SoapFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Jean-Marc Andre (jean-marc.andre@polymtl.ca)
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
		object detail;

		public SoapFault ()
		{

		}

		private SoapFault (SerializationInfo info, StreamingContext context)
		{
			code = info.GetString ("faultcode");
			faultString = info.GetString ("faultstring");
			detail = info.GetValue ("detail", typeof (object));
		}

		public SoapFault (string faultCode, string faultString,
				  string faultActor, ServerFault serverFault)
		{
			this.code = faultCode;
			this.actor = faultActor;
			this.faultString = faultString;
			this.detail = serverFault;
		}
		

		public object Detail {
			get { return detail; }
			set { detail = value; }
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
			info.AddValue ("faultcode", code, typeof (string));
			info.AddValue ("faultstring", faultString, typeof (string));
			info.AddValue ("detail", detail, typeof (object));
		}
	}
}
