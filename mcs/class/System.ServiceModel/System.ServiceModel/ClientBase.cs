//
// generic ClientBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	[MonoTODO ("It somehow rejects classes, but dunno how we can do that besides our code level.")]
	public abstract class ClientBase<TChannel>
		: IDisposable, ICommunicationObject
	{
		static InstanceContext initialContxt = new InstanceContext (null);

		ChannelFactory<TChannel> factory;
		ClientRuntimeChannel inner_channel;
		CommunicationState state;

		protected ClientBase ()
			: this (initialContxt)
		{
		}

		protected ClientBase (string configname)
			: this (initialContxt, configname)
		{
		}

		protected ClientBase (Binding binding, EndpointAddress remoteAddress)
			: this (initialContxt, binding, remoteAddress)
		{
		}

		protected ClientBase (string configname, EndpointAddress remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (string configname, string remoteAddress)
			: this (initialContxt, configname, remoteAddress)
		{
		}

		protected ClientBase (InstanceContext instance)
			: this (instance, "*")
		{
		}

		protected ClientBase (InstanceContext instance, string configname)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");

			Initialize (instance, configname, null);
		}

		protected ClientBase (InstanceContext instance,
			string configname, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (configname == null)
				throw new ArgumentNullException ("configurationName");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, configname, remoteAddress);
		}

		protected ClientBase (InstanceContext instance,
			string configname, string remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (remoteAddress == null)
				throw new ArgumentNullException ("endpointAddress");
			if (configname == null)
				throw new ArgumentNullException ("configurationname");

			Initialize (instance, configname, new EndpointAddress (remoteAddress));
		}

		protected ClientBase (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			if (instance == null)
				throw new ArgumentNullException ("instanceContext");
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (remoteAddress == null)
				throw new ArgumentNullException ("remoteAddress");

			Initialize (instance, binding, remoteAddress);
		}

		void Initialize (InstanceContext instance,
			string configName, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (configName, remoteAddress);
		}

		void Initialize (InstanceContext instance,
			Binding binding, EndpointAddress remoteAddress)
		{
			factory = new ChannelFactory<TChannel> (binding, remoteAddress);
		}

		public ChannelFactory<TChannel> ChannelFactory {
			get { return factory; }
		}

#if !NET_2_1
		public ClientCredentials ClientCredentials {
			get { return ChannelFactory.Credentials; }
		}
#endif

		public ServiceEndpoint Endpoint {
			get { return factory.Endpoint; }
		}

		public IClientChannel InnerChannel {
			get {
				if (inner_channel == null)
					inner_channel = (ClientRuntimeChannel) (object) factory.CreateChannel ();
				return inner_channel;
			}
		}

		protected TChannel Channel {
			get { return (TChannel) (object) InnerChannel; }
		}

		public CommunicationState State {
			get { return InnerChannel.State; }
		}

		[MonoTODO]
		public void Abort ()
		{
			InnerChannel.Abort ();
		}

		[MonoTODO]
		public void Close ()
		{
			InnerChannel.Close ();
		}

		[MonoTODO]
		public void DisplayInitializationUI ()
		{
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			Close ();
		}

		protected virtual TChannel CreateChannel ()
		{
			return ChannelFactory.CreateChannel ();
		}

		public void Open ()
		{
			InnerChannel.Open ();
		}

		#region ICommunicationObject implementation

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginOpen (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndOpen (IAsyncResult result)
		{
			InnerChannel.EndOpen (result);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (callback, state);
		}

		[MonoTODO]
		IAsyncResult ICommunicationObject.BeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return InnerChannel.BeginClose (timeout, callback, state);
		}

		[MonoTODO]
		void ICommunicationObject.EndClose (IAsyncResult result)
		{
			InnerChannel.EndClose (result);
		}

		[MonoTODO]
		void ICommunicationObject.Close (TimeSpan timeout)
		{
			InnerChannel.Close (timeout);
		}

		[MonoTODO]
		void ICommunicationObject.Open (TimeSpan timeout)
		{
			InnerChannel.Open (timeout);
		}

		event EventHandler ICommunicationObject.Opening {
			add { InnerChannel.Opening += value; }
			remove { InnerChannel.Opening -= value; }
		}
		event EventHandler ICommunicationObject.Opened {
			add { InnerChannel.Opened += value; }
			remove { InnerChannel.Opened -= value; }
		}
		event EventHandler ICommunicationObject.Closing {
			add { InnerChannel.Closing += value; }
			remove { InnerChannel.Closing -= value; }
		}
		event EventHandler ICommunicationObject.Closed {
			add { InnerChannel.Closed += value; }
			remove { InnerChannel.Closed -= value; }
		}
		event EventHandler ICommunicationObject.Faulted {
			add { InnerChannel.Faulted += value; }
			remove { InnerChannel.Faulted -= value; }
		}

		#endregion
	}
}
