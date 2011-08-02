//
// ServiceEndpoint.cs
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
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Description
{
	[DebuggerDisplay ("Name={name}")]
	[DebuggerDisplay ("Address={address}")]
	public class ServiceEndpoint
	{
		ContractDescription contract;
		Binding binding;
		EndpointAddress address;
		KeyedByTypeCollection<IEndpointBehavior> behaviors;
		Uri listen_uri;
		ListenUriMode listen_mode;
		string name;

		public ServiceEndpoint (ContractDescription contract)
			: this (contract, null, null)
		{
		}

		public ServiceEndpoint(ContractDescription contract,
			Binding binding, EndpointAddress address)
		{
			if (contract == null)
				throw new ArgumentNullException ("contract");

			this.contract = contract;
			this.binding = binding;
			this.address = address;
			behaviors = new KeyedByTypeCollection<IEndpointBehavior> ();
		}

		public KeyedByTypeCollection<IEndpointBehavior> Behaviors {
			get { return behaviors; }
		}

		public ContractDescription Contract {
			get { return contract; }
#if NET_4_0
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				contract = value;
			}
#endif
		}

		public EndpointAddress Address {
			get { return address; }
			set { address = value; }
		}

		public Binding Binding {
			get { return binding; }
			set { binding = value; }
		}

#if NET_4_0
		public
#else
		internal
#endif
		bool IsSystemEndpoint { get; set; }

		public Uri ListenUri {
			get { return listen_uri ?? (Address != null ? Address.Uri : null); }
			set { listen_uri = value; }
		}

		public ListenUriMode ListenUriMode {
			get { return listen_mode; }
			set { listen_mode = value; }
		}

		public string Name {
			get {
				if (name == null) {
					// do not create cache when either of Binding or Contract is null.
					if (Binding == null)
						return Contract != null ? Contract.Name : null;
					else if (Contract != null)
						name = Binding.Name + "_" + Contract.Name;
				}
				return name;
			}
			set { name = value; }
		}

		internal void Validate ()
		{
			foreach (IContractBehavior b in Contract.Behaviors)
				b.Validate (Contract, this);
			foreach (IEndpointBehavior b in Behaviors)
				b.Validate (this);
			foreach (OperationDescription operation in Contract.Operations) {
				foreach (IOperationBehavior b in operation.Behaviors)
					b.Validate (operation);
			}
		}


		internal ClientRuntime CreateClientRuntime (object callbackDispatchRuntime)
		{
			ServiceEndpoint se = this;

			var proxy = se.Contract.CreateClientRuntime (callbackDispatchRuntime);

			foreach (IEndpointBehavior b in se.Behaviors)
				b.ApplyClientBehavior (se, proxy);
			foreach (IContractBehavior b in se.Contract.Behaviors)
				b.ApplyClientBehavior (se.Contract, se, proxy);

			return proxy;
		}
	}
}
