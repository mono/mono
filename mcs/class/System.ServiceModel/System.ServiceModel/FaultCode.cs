//
// System.ServiceModel.FaultCode.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceModel
{
	public class FaultCode
	{
		string name, ns;
		FaultCode subcode;
		
		public FaultCode (string name)
			: this (name, String.Empty)
		{
		}

		public FaultCode (string name, string ns)
			: this (name, ns, null)
		{
		}

		public FaultCode (string name, FaultCode subCode)
			: this (name, String.Empty, subCode)
		{
		}

		public FaultCode (string name, string ns, FaultCode subCode)
		{
			this.name = name;
			this.ns = ns;
			this.subcode = subCode;
		}

		public bool IsPredefinedFault {
			get { return ns == String.Empty; }
		}

		public bool IsReceiverFault {
			get { return name == "Receiver"; }
		}
	
		public bool IsSenderFault {
			get { return name == "Sender"; }
		}
		
		public string Name {
			get { return name; }
		}

		public string Namespace {
			get { return ns; }
		}

		public FaultCode SubCode {
			get { return subcode; }
		}

		public static FaultCode CreateReceiverFaultCode (FaultCode subCode)
		{
			return new FaultCode ("Receiver", subCode);
		}

		public static FaultCode CreateReceiverFaultCode (string name, string ns)
		{
			return CreateReceiverFaultCode (new FaultCode (name, ns));
		}
		
		public static FaultCode CreateSenderFaultCode (FaultCode subCode)
		{
			return new FaultCode ("Sender", subCode);
		}

		public static FaultCode CreateSenderFaultCode (string name, string ns)
		{
			return CreateSenderFaultCode (new FaultCode (name, ns));
		}
	}
}