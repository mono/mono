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
		public SoapServerMethod ()
		{
		}

		[MonoTODO]
		public SoapServerMethod (Type serverType, LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string Action {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SoapBindingUse BindingUse { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SoapHeaderMapping [] InHeaderMappings {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSerializer InHeaderSerializer {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public LogicalMethodInfo MethodInfo {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool OneWay {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SoapHeaderMapping [] OutHeaderMappings {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSerializer OutHeaderSerializer {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSerializer ParameterSerializer {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SoapParameterStyle ParameterStyle {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public XmlSerializer ReturnSerializer {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public bool Rpc {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public WsiProfiles WsiClaims {
			get { throw new NotImplementedException (); }
		}
	}
}

#endif
