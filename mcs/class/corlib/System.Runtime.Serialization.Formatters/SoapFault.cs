//
// System.Runtime.Serialization.Formatters.SoapFault.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//
// 2002 (C) Copyright, Ximian, Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Serialization;

using System.Runtime.InteropServices;

namespace System.Runtime.Serialization.Formatters {

	[Serializable]
	[SoapType(Embedded=true)]
	[ComVisible (true)]
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
