//
// Binding.cs
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
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	public abstract class Binding : IDefaultCommunicationTimeouts
	{
		string name, ns;
		TimeSpan open_timeout, close_timeout;
		TimeSpan receive_timeout, send_timeout;

		protected Binding ()
		{
			Initialize ();
			name = GetType ().Name;
			ns = "http://tempuri.org/";
		}

		protected Binding (string name, string ns)
		{
			this.name = name;
			this.ns = ns;
			Initialize ();
		}

		public TimeSpan CloseTimeout {
			get { return close_timeout; }
			set { close_timeout = value; }
		}

		public TimeSpan OpenTimeout {
			get { return open_timeout; }
			set { open_timeout = value; }
		}

		public TimeSpan ReceiveTimeout {
			get { return receive_timeout; }
			set { receive_timeout = value; }
		}

		public TimeSpan SendTimeout {
			get { return send_timeout; }
			set { send_timeout = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public abstract string Scheme { get; }

		public MessageVersion MessageVersion {
			get { return GetProperty<MessageVersion> (new BindingParameterCollection ()); }
		}

		BindingContext CreateContext (
			BindingParameterCollection parameters)
		{
			// FIXME: it seems that binding elements are
			// "validated" so that the last item is a transport.
			return new BindingContext (
				new CustomBinding (this), parameters);
		}

		BindingContext CreateContext (
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			ListenUriMode listenUriMode,
			BindingParameterCollection parameters)
		{
			// FIXME: it seems that binding elements are
			// "validated" so that the last item is a transport.
			return new BindingContext (
				new CustomBinding (this),
				parameters,
				listenUriBaseAddress,
				listenUriRelativeAddress,
				listenUriMode);
		}

		public IChannelFactory<TChannel>
			BuildChannelFactory<TChannel> (
			params object [] parameters)
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return BuildChannelFactory<TChannel> (pl);
		}

		public virtual IChannelFactory<TChannel>
			BuildChannelFactory<TChannel> (
			BindingParameterCollection parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			return CreateContext (parameters).BuildInnerChannelFactory<TChannel> ();
		}

#if !MOBILE
		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			ListenUriMode listenUriMode,
			params object [] parameters)
			where TChannel : class, IChannel
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return BuildChannelListener<TChannel> (
				listenUriBaseAddress,
				listenUriRelativeAddress,
				listenUriMode,
				pl);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			ListenUriMode listenUriMode,
			BindingParameterCollection parameters)
			where TChannel : class, IChannel
		{
			if (listenUriBaseAddress == null)
				throw new ArgumentNullException ("listenUriBaseAddress");
			if (listenUriRelativeAddress == null)
				throw new ArgumentNullException ("listenUriRelativeAddress");
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			BindingContext ctx = CreateContext (listenUriBaseAddress,
				listenUriRelativeAddress,
				listenUriMode,
				parameters);
			return ctx.BuildInnerChannelListener<TChannel> ();
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			params object [] parameters)
			where TChannel : class, IChannel
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return BuildChannelListener<TChannel> (listenUriBaseAddress, pl);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			BindingParameterCollection parameters)
			where TChannel : class, IChannel
		{
			return BuildChannelListener<TChannel> (listenUriBaseAddress,
				String.Empty, parameters);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			params object [] parameters)
			where TChannel : class, IChannel
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return BuildChannelListener<TChannel> (
				listenUriBaseAddress, listenUriRelativeAddress, pl);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			BindingParameterCollection parameters)
			where TChannel : class, IChannel
		{
			return BuildChannelListener<TChannel> (
				listenUriBaseAddress,
				listenUriRelativeAddress,
				ListenUriMode.Explicit,
				parameters);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			params object [] parameters)
			where TChannel : class, IChannel
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return BuildChannelListener<TChannel> (pl);
		}

		public virtual IChannelListener<TChannel>
			BuildChannelListener<TChannel> (
			BindingParameterCollection parameters)
			where TChannel : class, IChannel
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			return CreateContext (parameters).BuildInnerChannelListener<TChannel> ();
		}
#endif

		public bool CanBuildChannelFactory<TChannel> (
			params object [] parameters)
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return CanBuildChannelFactory<TChannel> (pl);
		}

		public virtual bool CanBuildChannelFactory<TChannel> (
			BindingParameterCollection parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			return CreateContext (parameters).CanBuildInnerChannelFactory<TChannel> ();
		}

#if !MOBILE
		public bool CanBuildChannelListener<TChannel> (
			params object [] parameters)
			where TChannel : class, IChannel
		{
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			foreach (object o in parameters)
				pl.Add (o);
			return CanBuildChannelListener<TChannel> (pl);
		}

		public virtual bool CanBuildChannelListener<TChannel> (
			BindingParameterCollection parameters)
			where TChannel : class, IChannel
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			return CreateContext (parameters).CanBuildInnerChannelListener<TChannel> ();
		}
#endif

		public abstract BindingElementCollection CreateBindingElements ();

		public T GetProperty<T> (BindingParameterCollection parameters)
			where T : class
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");
			return CreateContext (parameters).GetInnerProperty<T> ();
		}

		private void Initialize ()
		{
			IDefaultCommunicationTimeouts t =
				DefaultCommunicationTimeouts.Instance;
			open_timeout = t.OpenTimeout;
			close_timeout = t.CloseTimeout;
			receive_timeout = t.ReceiveTimeout;
			send_timeout = t.SendTimeout;
		}
	}
}
