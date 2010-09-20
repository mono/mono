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
	public abstract class WebServiceEndpoint : ServiceEndpoint
	{
		internal WebServiceEndpoint (ContractDescription contract, EndpointAddress address)
			: base (contract, new WebHttpBinding (), address)
		{
			Behaviors.Add (new WebHttpBehavior ());
		}

		protected abstract Type WebEndpointType { get; }

		WebHttpBinding wbind {
			get {
				if (Binding is WebHttpBinding)
					return (WebHttpBinding) Binding;
				throw new InvalidOperationException ("Binding on this standard endpoint is not supposed to be overwritten.");
			}
		}

		public WebContentTypeMapper ContentTypeMapper {
			get { return wbind.ContentTypeMapper; }
			set { wbind.ContentTypeMapper = value; }
		}

		public bool CrossDomainScriptAccessEnabled {
			get { return wbind.CrossDomainScriptAccessEnabled; }
			set { wbind.CrossDomainScriptAccessEnabled = value; }
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return wbind.HostNameComparisonMode; }
			set { wbind.HostNameComparisonMode = value; }
		}

		public long MaxBufferPoolSize {
			get { return wbind.MaxBufferPoolSize; }
			set { wbind.MaxBufferPoolSize = value; }
		}

		public int MaxBufferSize {
			get { return wbind.MaxBufferSize; }
			set { wbind.MaxBufferSize = value; }
		}

		public long MaxReceivedMessageSize {
			get { return wbind.MaxReceivedMessageSize; }
			set { wbind.MaxReceivedMessageSize = value; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return wbind.ReaderQuotas; }
			set { wbind.ReaderQuotas = value; }
		}

		public WebHttpSecurity Security {
			get { return wbind.Security; }
		}

		public TransferMode TransferMode {
			get { return wbind.TransferMode; }
			set { wbind.TransferMode = value; }
		}

		public Encoding WriteEncoding {
			get { return wbind.WriteEncoding; }
			set { wbind.WriteEncoding = value; }
		}
	}
}
#endif
