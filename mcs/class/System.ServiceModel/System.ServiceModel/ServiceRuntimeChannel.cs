//
// ServiceRuntimeChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel
{
	internal class ServiceRuntimeChannel : CommunicationObject, IContextChannel, IClientChannel
	{
		IExtensionCollection<IContextChannel> extensions;
		readonly IChannel channel;		
		bool _allowInitializationUI;
		Uri _via;
		readonly TimeSpan _openTimeout;
		readonly TimeSpan _closeTimeout;

		public ServiceRuntimeChannel (IChannel channel, TimeSpan openTimeout, TimeSpan closeTimeout)
		{
			this.channel = channel;
			this._openTimeout = openTimeout;
			this._closeTimeout = closeTimeout;
		}

		#region IContextChannel

		[MonoTODO]
		public bool AllowOutputBatching {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public IInputSession InputSession {
			get {
				var ch = channel as ISessionChannel<IInputSession>;
				if (ch != null)
					return ch.Session;
				var dch = channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress LocalAddress {
			get {
				if (channel is IReplyChannel)
					return ((IReplyChannel) channel).LocalAddress;
				if (channel is IInputChannel)
					return ((IInputChannel) channel).LocalAddress;
				return null;
			}
		}

		[MonoTODO]
		public TimeSpan OperationTimeout {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public IOutputSession OutputSession {
			get {
				var ch = channel as ISessionChannel<IOutputSession>;
				if (ch != null)
					return ch.Session;
				var dch = channel as ISessionChannel<IDuplexSession>;
				return dch != null ? dch.Session : null;
			}
		}

		public EndpointAddress RemoteAddress {
			get {
				if (channel is IRequestChannel)
					return ((IRequestChannel) channel).RemoteAddress;
				if (channel is IOutputChannel)
					return ((IOutputChannel) channel).RemoteAddress;
				return null;
			}
		}

		public string SessionId {
			get { return InputSession != null ? InputSession.Id : null; }
		}

		#endregion

		// CommunicationObject
		protected internal override TimeSpan DefaultOpenTimeout {
			get { return _openTimeout; }
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return _closeTimeout; }
		}

		protected override void OnAbort ()
		{
			channel.Abort ();
		}

		protected override IAsyncResult OnBeginClose (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return channel.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			channel.EndClose (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			channel.Close (timeout);
		}

		protected override IAsyncResult OnBeginOpen (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return channel.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			channel.EndOpen (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			channel.Open (timeout);
		}

		// IChannel
		public T GetProperty<T> () where T : class
		{
			return channel.GetProperty<T> ();
		}

		// IExtensibleObject<IContextChannel>
		public IExtensionCollection<IContextChannel> Extensions {
			get {
				if (extensions == null)
					extensions = new ExtensionCollection<IContextChannel> (this);
				return extensions;
			}
		}


		#region IClientChannel Members

		public bool AllowInitializationUI {
			get {
				return _allowInitializationUI;
			}
			set {
				_allowInitializationUI = value;
			}
		}

		public bool DidInteractiveInitialization {
			get { throw new NotImplementedException (); }
		}

		public Uri Via {
			get { return _via; }
		}

		public IAsyncResult BeginDisplayInitializationUI (AsyncCallback callback, object state) {
			throw new NotImplementedException ();
		}

		public void EndDisplayInitializationUI (IAsyncResult result) {
			throw new NotImplementedException ();
		}

		public void DisplayInitializationUI () {
			throw new NotImplementedException ();
		}

		public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

		#endregion

		#region IDisposable Members

		public void Dispose () {
			throw new NotImplementedException ();
		}

		#endregion
	}
}
