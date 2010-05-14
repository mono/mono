//
// BindingContext.cs
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
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
	public class BindingContext
	{
		CustomBinding binding;
		BindingParameterCollection parameters;
		Uri listen_uri_base;
		string listen_uri_relative;
		ListenUriMode listen_uri_mode;

		BindingElementCollection elements; // for internal use

		public BindingContext (CustomBinding binding,
			BindingParameterCollection parms)
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (parms == null)
				throw new ArgumentNullException ("parms");

			this.binding = binding;
			parameters = new BindingParameterCollection ();
			foreach (var item in parms)
				parameters.Add (item);
			this.elements = new BindingElementCollection ();
			foreach (var item in binding.Elements)
				this.elements.Add (item);
		}

		public BindingContext (CustomBinding binding,
			BindingParameterCollection parameters,
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			ListenUriMode listenUriMode)
			: this (binding, parameters)
		{
			listen_uri_base = listenUriBaseAddress;
			listen_uri_relative = listenUriRelativeAddress;
			listen_uri_mode = listenUriMode;
		}

		// deep clone .ctor().
		BindingContext (CustomBinding binding,
			BindingParameterCollection parms,
			BindingElementCollection elems,
			Uri listenUriBaseAddress,
			string listenUriRelativeAddress,
			ListenUriMode listenUriMode)
		{
			this.binding = new CustomBinding (binding);
			parameters = new BindingParameterCollection ();
			foreach (var item in parms)
				parameters.Add (item);
			this.elements = new BindingElementCollection ();
			foreach (var item in elems)
				this.elements.Add (item);
			listen_uri_base = listenUriBaseAddress;
			listen_uri_relative = listenUriRelativeAddress;
			listen_uri_mode = listenUriMode;
		}


		public CustomBinding Binding {
			get { return binding; }
		}

		public BindingParameterCollection BindingParameters {
			get { return parameters; }
		}

		public Uri ListenUriBaseAddress {
			get { return listen_uri_base; }
			set { listen_uri_base = value; }
		}

		public string ListenUriRelativeAddress {
			get { return listen_uri_relative; }
			set { listen_uri_relative = value; }
		}

		public ListenUriMode ListenUriMode {
			get { return listen_uri_mode; }
			set { listen_uri_mode = value; }
		}

		public BindingElementCollection RemainingBindingElements {
			get {
				if (elements == null)
					elements = binding.CreateBindingElements ();
				return elements;
			}
		}

		internal BindingElement DequeueBindingElement ()
		{
			return DequeueBindingElement (true);
		}

		BindingElement DequeueBindingElement (bool raiseError)
		{
			if (RemainingBindingElements.Count == 0) {
				if (raiseError)
					throw new InvalidOperationException ("There is no more available binding element.");
				else
					return null;
			}
			BindingElement el = RemainingBindingElements [0];
			RemainingBindingElements.RemoveAt (0);
			return el;
		}

		public IChannelFactory<TChannel>
			BuildInnerChannelFactory<TChannel> ()
		{
			BindingContext ctx = this.Clone ();
			return ctx.DequeueBindingElement ().BuildChannelFactory<TChannel> (ctx);
		}

#if !NET_2_1
		public IChannelListener<TChannel>
			BuildInnerChannelListener<TChannel> ()
			where TChannel : class, IChannel
		{
			BindingContext ctx = this.Clone ();
			var be = ctx.DequeueBindingElement (false);
			if (be == null)
				throw new InvalidOperationException ("There is likely no TransportBindingElement that can build a channel listener in this binding context");
			return be.BuildChannelListener<TChannel> (ctx);
		}
#endif

		public bool CanBuildInnerChannelFactory<TChannel> ()
		{
			BindingContext ctx = this.Clone ();
			return ctx.DequeueBindingElement ().CanBuildChannelFactory<TChannel> (ctx);
		}

#if !NET_2_1
		public bool CanBuildInnerChannelListener<TChannel> ()
			where TChannel : class, IChannel
		{
			BindingContext ctx = this.Clone ();
			var be = ctx.DequeueBindingElement (false);
			if (be == null)
				throw new InvalidOperationException ("There is likely no TransportBindingElement that can build a channel listener in this binding context");
			return be.CanBuildChannelListener<TChannel> (ctx);
		}
#endif

		public T GetInnerProperty<T> () where T : class
		{
			BindingContext ctx = this.Clone ();
			BindingElement e = ctx.DequeueBindingElement (false);
			return e == null ? default (T) : e.GetProperty<T> (ctx);
		}

		public BindingContext Clone ()
		{
			return new BindingContext (this.binding, this.parameters, this.elements, this.listen_uri_base, this.listen_uri_relative, this.listen_uri_mode);
		}
	}
}
