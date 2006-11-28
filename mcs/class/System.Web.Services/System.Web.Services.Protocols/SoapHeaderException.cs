// 
// System.Web.Services.Protocols.SoapHeaderException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Runtime.Serialization;
using System.Xml;

namespace System.Web.Services.Protocols 
{
#if NET_2_0
	[Serializable]
#endif
	public class SoapHeaderException : SoapException {

		#region Constructors

#if NET_2_0
		public SoapHeaderException ()
			: this ("SOAP header error", XmlQualifiedName.Empty)
		{
		}
#endif

		public SoapHeaderException (string message, XmlQualifiedName code)
			: base (message, code)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, Exception innerException)
			: base (message, code, innerException)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, string actor)
			: base (message, code, actor)
		{
		}

		public SoapHeaderException (string message, XmlQualifiedName code, string actor, Exception innerException)
			: base (message, code, actor, innerException)
		{
		}
		
#if NET_2_0

		public SoapHeaderException (
			string message, 
			XmlQualifiedName code, 
			string actor, 
			string role, 
			string lang, 
			SoapFaultSubCode subcode, 
			Exception innerException)
			
		: base (message, code, actor, role, lang, null, subcode, innerException)
		{
			
		}

		public SoapHeaderException (
			string message, 
			XmlQualifiedName code, 
			string actor, 
			string role, 
			SoapFaultSubCode subcode, 
			Exception innerException)
			
		: base (message, code, actor, role, null, subcode, innerException)
		{
			
		}

		protected SoapHeaderException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
#endif

		#endregion // Constructors
	}
}
