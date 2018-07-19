//
// CustomBinding.cs
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
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Channels
{
	public class CustomBinding : Binding
	{
		const string default_ns = "http://tempuri.org";

		BindingElementCollection elements;
		ISecurityCapabilities security;
		string scheme = "";

		public CustomBinding (string configurationName)
			: this (configurationName, default_ns)
		{
		}

		public CustomBinding ()
			: base ()
		{
			elements = new BindingElementCollection ();
		}

		// Binding passed to .ctor() seems to have nothing to do
		// with the properties on this class.
		public CustomBinding (Binding binding)
			: this (binding.CreateBindingElements (),
				binding.Name, binding.Namespace)
		{
			OpenTimeout = binding.OpenTimeout;
			CloseTimeout = binding.CloseTimeout;
			SendTimeout = binding.SendTimeout;
			ReceiveTimeout = binding.ReceiveTimeout;
			scheme = binding.Scheme;
			security = binding as ISecurityCapabilities;
		}

		public CustomBinding (params BindingElement [] bindingElementsInTopDownChannelStackOrder)
			: this ("CustomBinding", default_ns, bindingElementsInTopDownChannelStackOrder)
		{
		}

		public CustomBinding (IEnumerable<BindingElement> bindingElementsInTopDownChannelStackOrder)
			: this (bindingElementsInTopDownChannelStackOrder, "CustomBinding", default_ns)
		{
		}

		public CustomBinding (string name, string ns,
			params BindingElement [] bindingElementsInTopDownChannelStackOrder)
			: this (bindingElementsInTopDownChannelStackOrder, name, ns)
		{
		}

		private CustomBinding (IEnumerable<BindingElement> binding,
			string name, string ns)
			: base (name, ns)
		{
			elements = new BindingElementCollection (binding);
			foreach (BindingElement be in elements) {
				TransportBindingElement tbe = be as TransportBindingElement;
				if (tbe == null)
					continue;
				scheme = tbe.Scheme;
				break;
			}
		}

		public BindingElementCollection Elements {
			get { return elements; }
		}

		public override string Scheme {
			get { return scheme; }
		}

		public override BindingElementCollection CreateBindingElements ()
		{
			return elements.Clone ();
		}
	}
}
