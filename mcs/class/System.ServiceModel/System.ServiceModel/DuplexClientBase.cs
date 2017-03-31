//
// DuplexClientBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005,2009 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel
{
	public class DuplexClientBase<TChannel> : ClientBase<TChannel> where TChannel : class
	{
		protected DuplexClientBase (object callbackInstance)
			: this (new InstanceContext (callbackInstance), (Binding) null, null)
		{
		}

		protected DuplexClientBase (object callbackInstance,
			Binding binding, EndpointAddress remoteAddress)
			: this (new InstanceContext (callbackInstance), binding, remoteAddress)
		{
		}

		protected DuplexClientBase (object callbackInstance,
			string endpointConfigurationName)
			: this (new InstanceContext (callbackInstance), endpointConfigurationName)
		{
		}

		protected DuplexClientBase (object callbackInstance,
			string endpointConfigurationName, EndpointAddress remoteAddress)
			: this (new InstanceContext (callbackInstance), endpointConfigurationName, remoteAddress)
		{
		}

		protected DuplexClientBase (object callbackInstance,
			string endpointConfigurationName, string remoteAddress)
			: this (new InstanceContext (callbackInstance), endpointConfigurationName, remoteAddress)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance)
			: base (callbackInstance)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance,
			Binding binding, EndpointAddress remoteAddress)
			: base (callbackInstance, binding, remoteAddress)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance,
			string endpointConfigurationName)
			: base (callbackInstance, endpointConfigurationName)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance,
			string endpointConfigurationName, string remoteAddress)
			: base (callbackInstance, endpointConfigurationName, remoteAddress)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance,
			string endpointConfigurationName, EndpointAddress remoteAddress)
			: base (callbackInstance, endpointConfigurationName, remoteAddress)
		{
		}

		protected DuplexClientBase (object callbackInstance, ServiceEndpoint endpoint)
			: this (new InstanceContext (callbackInstance), endpoint)
		{
		}

		protected DuplexClientBase (InstanceContext callbackInstance, ServiceEndpoint endpoint)
			: base (callbackInstance, endpoint)
		{
		}

		public IDuplexContextChannel InnerDuplexChannel {
			get { return (IDuplexContextChannel) base.InnerChannel; }
		}

		internal override void Initialize (InstanceContext instance,
			string endpointConfigurationName, EndpointAddress remoteAddress)
		{
			ChannelFactory = new DuplexChannelFactory<TChannel> (instance, endpointConfigurationName, remoteAddress);
		}

		internal override void Initialize (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			ChannelFactory = new DuplexChannelFactory<TChannel> (instance, binding, remoteAddress);
		}

		protected override TChannel CreateChannel ()
		{
			return ChannelFactory.CreateChannel ();
		}
	}
}
