//
// ClientOperation.cs
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
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	public sealed class ClientOperation
	{
		internal class ClientOperationCollection :
#if NET_2_1
			KeyedCollection<string, ClientOperation>
#else
			SynchronizedKeyedCollection<string, ClientOperation>
#endif
		{
			protected override string GetKeyForItem (ClientOperation o)
			{
				return o.Name;
			}
		}

		ClientRuntime parent;
		string name, action, reply_action;
		MethodInfo sync_method, begin_method, end_method;
		bool deserialize_reply = true, serialize_request = true;
		bool is_initiating, is_terminating, is_oneway;
		IClientMessageFormatter formatter;
		SynchronizedCollection<IParameterInspector> inspectors
			= new SynchronizedCollection<IParameterInspector> ();
#if !NET_2_1
		SynchronizedCollection<FaultContractInfo> fault_contract_infos = new SynchronizedCollection<FaultContractInfo> ();
#endif

		public ClientOperation (ClientRuntime parent,
			string name, string action)
		{
			this.parent = parent;
			this.name = name;
			this.action = action;
		}

		public ClientOperation (ClientRuntime parent,
			string name, string action, string replyAction)
		{
			this.parent = parent;
			this.name = name;
			this.action = action;
			this.reply_action = replyAction;
		}

		public string Action {
			get { return action; }
		}

		public string ReplyAction {
			get { return reply_action; }
		}

		public MethodInfo BeginMethod {
			get { return begin_method; }
			set {
				ThrowIfOpened ();
				begin_method = value;
			}
		}

		public bool DeserializeReply {
			get { return deserialize_reply; }
			set {
				ThrowIfOpened ();
				deserialize_reply = value;
			}
		}

		public MethodInfo EndMethod {
			get { return end_method; }
			set {
				ThrowIfOpened ();
				end_method = value;
			}
		}

#if !NET_2_1
		public SynchronizedCollection<FaultContractInfo> FaultContractInfos {
			get { return fault_contract_infos; }
		}
#endif

		public IClientMessageFormatter Formatter {
			get { return formatter; }
			set {
				ThrowIfOpened ();
				formatter = value;
			}
		}

		public bool IsInitiating {
			get { return is_initiating; }
			set {
				ThrowIfOpened ();
				is_initiating = value;
			}
		}

		public bool IsOneWay {
			get { return is_oneway; }
			set {
				ThrowIfOpened ();
				is_oneway = value;
			}
		}

		public bool IsTerminating {
			get { return is_terminating; }
			set {
				ThrowIfOpened ();
				is_terminating = value;
			}
		}

		public string Name {
			get { return name; }
		}

#if MOONLIGHT
		public IList<IParameterInspector> ParameterInspectors {
 #else
		public SynchronizedCollection<IParameterInspector> ParameterInspectors {
#endif
			get { return inspectors; }
		}

		public ClientRuntime Parent {
			get { return parent; }
		}

		public bool SerializeRequest {
			get { return serialize_request; }
			set {
				ThrowIfOpened ();
				serialize_request = value;
			}
		}

		public MethodInfo SyncMethod {
			get { return sync_method; }
			set {
				ThrowIfOpened ();
				sync_method = value;
			}
		}

		void ThrowIfOpened ()
		{
			// FIXME: get correct state
			var state = CommunicationState.Created;
			switch (state) {
			case CommunicationState.Created:
			case CommunicationState.Opening:
				return;
			}
			throw new InvalidOperationException ("Cannot change this property after the service host is opened");
		}
		
#if MOONLIGHT
		// introduced for silverlight sdk compatibility
		internal bool IsFaultFormatterSetExplicit {
			get { throw new NotImplementedException (); }
		}
		// introduced for silverlight sdk compatibility
		internal SynchronizedCollection<FaultContractInfo> FaultContractInfos {
			get { throw new NotImplementedException (); }
		}
		// introduced for silverlight sdk compatibility
		internal IClientFaultFormatter FaultFormatter {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif
	}
}
