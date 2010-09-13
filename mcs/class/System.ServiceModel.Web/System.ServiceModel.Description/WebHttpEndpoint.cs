//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class WebHttpEndpoint : WebServiceEndpoint
	{
		public WebHttpEndpoint (ContractDescription contract)
			: this (contract, null)
		{
		}

		public WebHttpEndpoint (ContractDescription contract, EndpointAddress address)
			: base (contract, address)
		{
		}

		WebHttpBehavior wb {
			get {
				var b = Behaviors.Find<WebHttpBehavior> ();
				if (b != null)
					return b;
				throw new InvalidOperationException ("The preset WebHttpBehavior was unexpectedly removed.");
			}
		}

		public bool AutomaticFormatSelectionEnabled {
			get { return wb.AutomaticFormatSelectionEnabled; }
			set { wb.AutomaticFormatSelectionEnabled = value; }
		}

		public WebMessageFormat DefaultOutgoingResponseFormat {
			get { return wb.DefaultOutgoingResponseFormat; }
			set { wb.DefaultOutgoingResponseFormat = value; }
		}

		public bool FaultExceptionEnabled {
			get { return wb.FaultExceptionEnabled; }
			set { wb.FaultExceptionEnabled = value; }
		}

		public bool HelpEnabled {
			get { return wb.HelpEnabled; }
			set { wb.HelpEnabled = value; }
		}

		protected override Type WebEndpointType {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif
