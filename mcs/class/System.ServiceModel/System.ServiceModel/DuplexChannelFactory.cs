//
// DuplexChannelFactory.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marcos Cobena (marcoscobena@gmail.com)
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	public class DuplexChannelFactory<TChannel> : ChannelFactory<TChannel>
	{
		InstanceContext callback_instance;
		
		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance)
		{
		}

		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance,
			string endpointConfigurationName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance,
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance,
			ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance,
			Binding binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (object callbackInstance,
			Binding binding,
			EndpointAddress remoteAddress)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (InstanceContext callbackInstance,
			Binding binding)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (InstanceContext callbackInstance,
			Binding binding,
			EndpointAddress remoteAddress) : base (binding, remoteAddress)
		{
			callback_instance = callbackInstance;
		}

		[MonoTODO]
		public DuplexChannelFactory (InstanceContext callbackInstance,
			string endpointConfigurationName,
			EndpointAddress remoteAddress)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (InstanceContext callbackInstance,
			string endpointConfigurationName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DuplexChannelFactory (InstanceContext callbackInstance,
			ServiceEndpoint endpoint)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static TChannel CreateChannel (InstanceContext callbackInstance, 
		                                      Binding binding, 
		                                      EndpointAddress endpointAddress)
		{
			throw new NotImplementedException ();
		}
	}
}
