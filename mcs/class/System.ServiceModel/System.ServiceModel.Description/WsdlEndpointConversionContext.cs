//
// WsdlEndpointConversionContext.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Web.Services.Description;
using System.Xml;

using WSBinding = System.Web.Services.Description.Binding;

namespace System.ServiceModel.Description
{
	[MonoTODO]
	public class WsdlEndpointConversionContext
	{
		WsdlContractConversionContext context;
		ServiceEndpoint endpoint;
		Port port;
		WSBinding wsdl_binding;

		internal WsdlEndpointConversionContext (WsdlContractConversionContext context, ServiceEndpoint endpoint, Port port, WSBinding wsdlBinding)
		{
			this.context = context;
			this.endpoint = endpoint;
			this.port = port;
			this.wsdl_binding = wsdlBinding;
		}

		public WsdlContractConversionContext ContractConversionContext {
			get { return context; }
		}

		public ServiceEndpoint Endpoint {
			get { return endpoint; }
		}

		public WSBinding WsdlBinding {
			get { return wsdl_binding; }
		}

		public Port WsdlPort {
			get { return port; }
		}

		public MessageBinding GetMessageBinding (
			MessageDescription message)
		{
			throw new NotImplementedException ();
		}

		public MessageDescription GetMessageDescription (
			MessageBinding messageBinding)
		{
			throw new NotImplementedException ();
		}

		public OperationBinding GetOperationBinding (
			OperationDescription operation)
		{
			throw new NotImplementedException ();
		}

		public OperationDescription GetOperationDescription (
			OperationBinding operationBinding)
		{
			throw new NotImplementedException ();
		}
	}
}
