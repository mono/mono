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
		static BindingElementCollection empty_collection =
			new BindingElementCollection ();

		CustomBinding binding;
		BindingParameterCollection parameters;
		BindingElementCollection elements = empty_collection;
		BindingElementCollection remaining_elements;
		Uri listen_uri_base;
		string listen_uri_relative;
		ListenUriMode listen_uri_mode;

		public BindingContext (CustomBinding binding,
			BindingParameterCollection parameters)
		{
			if (binding == null)
				throw new ArgumentNullException ("binding");
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			this.binding = binding;
			this.parameters = parameters;
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

		// copy .ctor().
		private BindingContext (BindingContext other)
		{
			binding = other.binding;
			parameters = other.parameters;
			elements = other.elements == empty_collection ?
				empty_collection : other.elements.Clone ();
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
				if (remaining_elements == null)
					remaining_elements = new BindingElementCollection ();
				return remaining_elements;
			}
		}

		internal BindingElement DequeueBindingElement ()
		{
			return DequeueBindingElement (true);
		}

		BindingElement DequeueBindingElement (bool raiseError)
		{
			if (elements.Count == 0) {
				if (raiseError)
					throw new InvalidOperationException ("There is no more available binding element.");
				else
					return null;
			}
			BindingElement el = elements [0];
			elements.RemoveAt (0);
			return el;
		}

		private bool PrepareElements ()
		{
			if (elements != empty_collection)
				return false;
			elements = binding.CreateBindingElements ();
			return true;
		}

		public IChannelFactory<TChannel>
			BuildInnerChannelFactory<TChannel> ()
		{
			bool restore = PrepareElements ();
			try {
				return DequeueBindingElement ().BuildChannelFactory<TChannel> (this);
			} finally {
				if (restore)
					elements = empty_collection;
			}
		}

#if !NET_2_1
		public IChannelListener<TChannel>
			BuildInnerChannelListener<TChannel> ()
			where TChannel : class, IChannel
		{
			bool restore = PrepareElements ();
			try {
				return DequeueBindingElement ().BuildChannelListener<TChannel> (this);
			} finally {
				if (restore)
					elements = empty_collection;
			}
		}
#endif

		public bool CanBuildInnerChannelFactory<TChannel> ()
		{
			bool restore = PrepareElements ();
			try {
				return elements.Count > 0 &&
					DequeueBindingElement ().CanBuildChannelFactory<TChannel> (this);
			} finally {
				if (restore)
					elements = empty_collection;
			}
		}

#if !NET_2_1
		public bool CanBuildInnerChannelListener<TChannel> ()
			where TChannel : class, IChannel
		{
			bool restore = PrepareElements ();
			try {
				return elements.Count > 0 &&
					DequeueBindingElement ().CanBuildChannelListener<TChannel> (this);
			} finally {
				if (restore)
					elements = empty_collection;
			}
		}
#endif

		public T GetInnerProperty<T> () where T : class
		{
			bool restore = PrepareElements ();
			try {
				BindingElement e = DequeueBindingElement (false);
				return e == null ? default (T) : e.GetProperty<T> (this);
			} finally {
				if (restore)
					elements = empty_collection;
			}
		}

		public BindingContext Clone ()
		{
			return new BindingContext (this);
		}
	}
}
