// 
// SoapServerType.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

using System.Collections;
using System.Web.Services.Description;
using System.Web.Services.Configuration;

namespace System.Web.Services.Protocols
{
	public sealed class SoapServerType : ServerType
	{
		Hashtable serverMethods = new Hashtable ();

		public SoapServerType (Type type, WebServiceProtocols protocolsSupported)
			: base (type)
		{
			// FIXME: these calls could be altered. Here they
			// are invoked to verify attributes.
			if ((protocolsSupported & WebServiceProtocols.HttpSoap) != 0)
				LogicalType.GetTypeStub ("Soap");
			if ((protocolsSupported & WebServiceProtocols.HttpSoap12) != 0)
				LogicalType.GetTypeStub ("Soap12");

			foreach (LogicalMethodInfo m in LogicalType.LogicalMethods) {
				SoapServerMethod sm = new SoapServerMethod (type, m);
				serverMethods.Add (sm.Action, sm);
			}
		}

		[MonoTODO]
		public SoapServerMethod GetDuplicateMethod (object key)
		{
			throw new NotImplementedException ();
		}

		public SoapServerMethod GetMethod (object key)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return serverMethods [key] as SoapServerMethod;
		}

		public bool ServiceDefaultIsEncoded {
			get { return LogicalType.BindingUse == SoapBindingUse.Encoded; }
		}

		public string ServiceNamespace {
			get { return LogicalType.WebServiceNamespace; }
		}

		public bool ServiceRoutingOnSoapAction {
			get { return LogicalType.RoutingStyle == SoapServiceRoutingStyle.SoapAction; }
		}
	}
}

#endif
