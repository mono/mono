//
// ContractDescription.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Description
{
	public class ContractDescription
	{		
		[MonoTODO]
		public static ContractDescription GetContract (
			Type contractType)
		{
			return ContractDescriptionGenerator.GetContract (contractType);
		}

		[MonoTODO]
		public static ContractDescription GetContract (
			Type contractType, object serviceImplementation)
		{
			return ContractDescriptionGenerator.GetContract (contractType, serviceImplementation);
		}

		[MonoTODO]
		public static ContractDescription GetContract (
			Type contractType, Type serviceType)
		{
			return ContractDescriptionGenerator.GetContract (contractType, serviceType);
		}

		OperationDescriptionCollection operations;
		KeyedByTypeCollection<IContractBehavior> behaviors;
		Type callback_contract_type, contract_type;
		string name, ns, config_name;
		ProtectionLevel protection_level;
		bool has_protection_level;
		SessionMode session;

		public ContractDescription (string name)
			: this (name, null)
		{
		}

		public ContractDescription (string name, string ns)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentOutOfRangeException ("ContractDescription's Name must be a non-empty string.");
			if (ns == null)
				ns = "http://tempuri.org/";

			this.name = name;
			this.ns = ns;
			behaviors = new KeyedByTypeCollection<IContractBehavior>  ();
			operations = new OperationDescriptionCollection ();
		}

		public KeyedByTypeCollection<IContractBehavior> Behaviors {
			get { return behaviors; }
		}

		public Type CallbackContractType {
			get { return callback_contract_type; }
			set { callback_contract_type = value; }
		}

		public string ConfigurationName {
			get { return config_name; }
			set { config_name = value; }
		}

		public Type ContractType {
			get { return contract_type; }
			set { contract_type = value; }
		}

		public bool HasProtectionLevel {
			get { return has_protection_level; }
		}

		public ProtectionLevel ProtectionLevel {
			get { return protection_level; }
			set {
				protection_level = value;
				has_protection_level = true;
			}
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public OperationDescriptionCollection Operations {
			get { return operations; }
		}

		public SessionMode SessionMode {
			get { return session; }
			set { session = value; }
		}

		[MonoTODO]
		public Collection<ContractDescription> GetInheritedContracts ()
		{
			throw new NotImplementedException ();
		}

		internal ClientRuntime CreateClientRuntime (object callbackDispatchRuntime)
		{
			ClientRuntime proxy = new ClientRuntime (Name, Namespace, callbackDispatchRuntime) {ContractClientType = ContractType, CallbackClientType = CallbackContractType};
			FillClientOperations (proxy, false);
			return proxy;
		}

		internal void FillClientOperations (ClientRuntime proxy, bool isCallback)
		{
			foreach (OperationDescription od in Operations) {
				if (!(isCallback && od.InCallbackContract || !isCallback && od.InOrdinalContract))
					continue; // not in the contract in use.

				if (!proxy.Operations.Contains (od.Name))
					PopulateClientOperation (proxy, od, isCallback);
#if !MOONLIGHT
				foreach (IOperationBehavior ob in od.Behaviors)
					ob.ApplyClientBehavior (od, proxy.Operations [od.Name]);
#endif
			}
		}

		void PopulateClientOperation (ClientRuntime proxy, OperationDescription od, bool isCallback)
		{
			string reqA = null, resA = null;
			foreach (MessageDescription m in od.Messages) {
				bool isReq = m.Direction == MessageDirection.Input ^ isCallback;
				if (isReq)
					reqA = m.Action;
				else
					resA = m.Action;
			}
			ClientOperation o =
				od.IsOneWay ?
				new ClientOperation (proxy, od.Name, reqA) :
				new ClientOperation (proxy, od.Name, reqA, resA);
			foreach (MessageDescription md in od.Messages) {
				bool isReq = md.Direction == MessageDirection.Input ^ isCallback;
				if (isReq &&
				    md.Body.Parts.Count == 1 &&
				    md.Body.Parts [0].Type == typeof (Message))
					o.SerializeRequest = false;
				if (!isReq &&
				    md.Body.ReturnValue != null &&
				    md.Body.ReturnValue.Type == typeof (Message))
					o.DeserializeReply = false;
			}
#if !NET_2_1
			foreach (var fd in od.Faults)
				o.FaultContractInfos.Add (new FaultContractInfo (fd.Action, fd.DetailType));
#endif

			// FIXME: at initialization time it does not seem to 
			// fill default formatter. It should be filled after
			// applying all behaviors. (Tthat causes regression, so
			// I don't care little compatibility difference so far)
			//
			// FIXME: pass correct isRpc, isEncoded
			o.Formatter = new OperationFormatter (od, false, false);

			proxy.Operations.Add (o);
		}
	}
}
