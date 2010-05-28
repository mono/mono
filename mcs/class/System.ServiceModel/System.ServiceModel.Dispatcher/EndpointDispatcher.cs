//
// EndpointDispatcher.cs
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
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security.Tokens;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	public class EndpointDispatcher
	{
		EndpointAddress address;
		string contract_name, contract_ns;
		ChannelDispatcher channel_dispatcher;
		MessageFilter address_filter;
		MessageFilter contract_filter;
		int filter_priority;
		DispatchRuntime dispatch_runtime;

		// Umm, this API is ugly, since it or its members will
		// anyways require ServiceEndpoint, those arguments are
		// likely to be replaced by ServiceEndpoint (especially
		// considering about possible EndpointAddress inconsistency).
		public EndpointDispatcher (EndpointAddress address,
			string contractName, string contractNamespace)
		{
			if (contractName == null)
				throw new ArgumentNullException ("contractName");
			if (contractNamespace == null)
				throw new ArgumentNullException ("contractNamespace");
			if (address == null)
				throw new ArgumentNullException ("address");

			this.address = address;
			contract_name = contractName;
			contract_ns = contractNamespace;

			dispatch_runtime = new DispatchRuntime (this);

			this.address_filter = new EndpointAddressMessageFilter (address);
		}

		public DispatchRuntime DispatchRuntime {
			get { return dispatch_runtime; }
		}

		public string ContractName {
			get { return contract_name; }
		}

		public string ContractNamespace {
			get { return contract_ns; }
		}

		public ChannelDispatcher ChannelDispatcher {
			get { return channel_dispatcher; }
			internal set { channel_dispatcher = value; }
		}

		public MessageFilter AddressFilter {
			get { return address_filter; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				address_filter = value;
			}
		}

		public MessageFilter ContractFilter {
			get { return contract_filter ?? (contract_filter = new MatchAllMessageFilter ()); }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				contract_filter = value;
			}
		}

		public EndpointAddress EndpointAddress {
			get { return address; }
		}

		public int FilterPriority {
			get { return filter_priority; }
			set { filter_priority = value; }
		}

		internal void InitializeServiceEndpoint (bool isCallback, Type serviceType, ServiceEndpoint se)
		{
			this.ContractFilter = GetContractFilter (se.Contract);

			this.DispatchRuntime.Type = serviceType;
			
			//Build the dispatch operations
			DispatchRuntime db = this.DispatchRuntime;
			if (!isCallback && se.Contract.CallbackContractType != null) {
				var ccd = ContractDescriptionGenerator.GetCallbackContract (db.Type, se.Contract.CallbackContractType);
				// FIXME: remove below line. It does not have to be refilled here. (CallbackBehaviorAttributeTest blocks its removal.)
				db.CallbackClientRuntime = ccd.CreateClientRuntime ();
				db.CallbackClientRuntime.CallbackDispatchRuntime = DispatchRuntime;
				db.CallbackClientRuntime.CallbackClientType = se.Contract.CallbackContractType;
				// FIXME: enable it. it should be the contract type, not the callback type.
				// db.CallbackClientRuntime.ContractClientType = se.Contract.ContractType;
			}
			foreach (OperationDescription od in se.Contract.Operations)
				if (!db.Operations.Contains (od.Name))
					PopulateDispatchOperation (db, od);
		}

		void PopulateDispatchOperation (DispatchRuntime db, OperationDescription od) {
			string reqA = null, resA = null;
			foreach (MessageDescription m in od.Messages) {
				if (m.Direction == MessageDirection.Input)
					reqA = m.Action;
				else
					resA = m.Action;
			}
			DispatchOperation o =
				od.IsOneWay ?
				new DispatchOperation (db, od.Name, reqA) :
				new DispatchOperation (db, od.Name, reqA, resA);
			bool no_serialized_reply = od.IsOneWay;
			foreach (MessageDescription md in od.Messages) {
				if (md.Direction == MessageDirection.Input &&
					md.Body.Parts.Count == 1 &&
					md.Body.Parts [0].Type == typeof (Message))
					o.DeserializeRequest = false;
				if (md.Direction == MessageDirection.Output &&
					md.Body.ReturnValue != null) {
					if (md.Body.ReturnValue.Type == typeof (Message))
						o.SerializeReply = false;
					else if (md.Body.ReturnValue.Type == typeof (void))
						no_serialized_reply = true;
				}
			}

			foreach (var fd in od.Faults)
				o.FaultContractInfos.Add (new FaultContractInfo (fd.Action, fd.DetailType));

			// Setup Invoker
			o.Invoker = new DefaultOperationInvoker (od);

			// Setup Formater
			// FIXME: this seems to be null at initializing, and should be set after applying all behaviors.
			// I leave this as is to not regress and it's cosmetic compatibility to fix.
			// FIXME: pass correct isRpc, isEncoded
			o.Formatter = new OperationFormatter (od, false, false);

			if (o.Action == "*" && (o.IsOneWay || o.ReplyAction == "*")) {
				//Signature : Message  (Message)
				//	    : void  (Message)
				//FIXME: void (IChannel)
				if (!o.DeserializeRequest && (!o.SerializeReply || no_serialized_reply)) // what is this double-ish check for?
					db.UnhandledDispatchOperation = o;
			}

			db.Operations.Add (o);
		}

		MessageFilter GetContractFilter (ContractDescription cd)
		{
			List<string> actions = new List<string> ();
			foreach (var od in cd.Operations)
				foreach (var md in od.Messages)
					if (md.Direction == MessageDirection.Input)
						if (md.Action == "*")
							return new MatchAllMessageFilter ();
						else
							actions.Add (md.Action);

			return new ActionMessageFilter (actions.ToArray ());
		}
	}
}
