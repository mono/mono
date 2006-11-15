// 
// System.Web.Services.Protocols.Soap12FaultCodes.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
//

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

#if NET_2_0

using System.Xml;

namespace System.Web.Services.Protocols 
{
	public sealed class Soap12FaultCodes
	{
		private Soap12FaultCodes ()
		{
		}
		
		public static readonly XmlQualifiedName DataEncodingUnknownFaultCode 
			= new XmlQualifiedName ("DataEncodingUnknown", "http://www.w3.org/2003/05/soap-envelope");
			
		public static readonly XmlQualifiedName EncodingMissingIdFaultCode
			= new XmlQualifiedName ("MissingID", "http://www.w3.org/2003/05/soap-encoding");
			
		public static readonly XmlQualifiedName EncodingUntypedValueFaultCode
			= new XmlQualifiedName ("UntypedValue", "http://www.w3.org/2003/05/soap-encoding");
			
		public static readonly XmlQualifiedName MustUnderstandFaultCode
			= new XmlQualifiedName ("MustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
			
		public static readonly XmlQualifiedName ReceiverFaultCode
			= new XmlQualifiedName ("Receiver", "http://www.w3.org/2003/05/soap-envelope");
			
		public static readonly XmlQualifiedName RpcBadArgumentsFaultCode
			= new XmlQualifiedName ("BadArguments", "http://www.w3.org/2003/05/soap-rpc");
			
		public static readonly XmlQualifiedName RpcProcedureNotPresentFaultCode
			= new XmlQualifiedName ("ProcedureNotPresent", "http://www.w3.org/2003/05/soap-rpc");
			
		public static readonly XmlQualifiedName SenderFaultCode
			= new XmlQualifiedName ("Sender", "http://www.w3.org/2003/05/soap-envelope");
			
		public static readonly XmlQualifiedName VersionMismatchFaultCode
			= new XmlQualifiedName ("VersionMismatch", "http://www.w3.org/2003/05/soap-envelope");
	}
}

#endif
