// 
// SoapServerMethod.cs
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

using System.Web.Services;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Serialization;

namespace System.Web.Services.Protocols
{
	public sealed class SoapServerMethod
	{
		SoapMethodStubInfo info;

		[MonoTODO] // what to do here?
		public SoapServerMethod ()
		{
			throw new NotImplementedException ();
		}

		public SoapServerMethod (Type serverType, LogicalMethodInfo methodInfo)
		{
			TypeStubInfo type = TypeStubManager.GetTypeStub (serverType, "Soap");
			info = type.GetMethod (methodInfo.Name) as SoapMethodStubInfo;
			if (info == null)
				throw new InvalidOperationException ("Argument methodInfo does not seem to be a member of the server type.");
		}

		public string Action {
			get { return info.Action; }
		}

		public SoapBindingUse BindingUse { 
			get { return info.Use; }
		}

		public SoapHeaderMapping [] InHeaderMappings {
			get { return info.InHeaders; }
		}

		public XmlSerializer InHeaderSerializer {
			get { return info.RequestHeadersSerializer; }
		}

		public LogicalMethodInfo MethodInfo {
			get { return info.MethodInfo; }
		}

		public bool OneWay {
			get { return info.OneWay; }
		}

		public SoapHeaderMapping [] OutHeaderMappings {
			get { return info.OutHeaders; }
		}

		public XmlSerializer OutHeaderSerializer {
			get { return info.ResponseHeadersSerializer; }
		}

		public XmlSerializer ParameterSerializer {
			get { return info.RequestSerializer; }
		}

		public SoapParameterStyle ParameterStyle {
			get { return info.ParameterStyle; }
		}

		public XmlSerializer ReturnSerializer {
			get { return info.ResponseSerializer; }
		}

		public bool Rpc {
			get { return info.SoapBindingStyle == SoapBindingStyle.Rpc; }
		}

		public WsiProfiles WsiClaims {
			get { return info.TypeStub.WsiClaims; }
		}
	}
}

#endif
