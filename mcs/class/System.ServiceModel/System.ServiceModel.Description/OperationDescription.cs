//
// OperationDescription.cs
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
using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace System.ServiceModel.Description
{
	[DebuggerDisplay ("Name={name}, IsInitiating={isInitiating}, IsTerminating={isTerminating}")]
	public class OperationDescription
	{
		MethodInfo begin_method, end_method, sync_method;
		FaultDescriptionCollection faults
			= new FaultDescriptionCollection ();
		ContractDescription contract;
		KeyedByTypeCollection<IOperationBehavior> behaviors
			= new KeyedByTypeCollection<IOperationBehavior> ();
		bool is_initiating, is_oneway, is_terminating;
		Collection<Type> known_types = new Collection<Type> ();
		MessageDescriptionCollection messages
			= new MessageDescriptionCollection ();
		string name;
		ProtectionLevel protection_level;
		bool has_protection_level;

		public OperationDescription (string name,
			ContractDescription declaringContract)
		{
			this.name = name;
			contract = declaringContract;
			is_initiating = true;
			XmlName = new XmlName (name);
		}

		internal bool InOrdinalContract { get; set; }
		internal bool InCallbackContract { get; set; }

		public MethodInfo BeginMethod {
			get { return begin_method; }
			set { begin_method = value; }
		}

		public KeyedByTypeCollection<IOperationBehavior> Behaviors {
			get { return behaviors; }
		}

#if NET_4_5
		[MonoTODO]
		public KeyedCollection<Type,IOperationBehavior> OperationBehaviors {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public MethodInfo TaskMethod {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		public ContractDescription DeclaringContract {
			get { return contract; }
			set { contract = value; }
		}

		public MethodInfo EndMethod {
			get { return end_method; }
			set { end_method = value; }
		}

		public FaultDescriptionCollection Faults {
			get { return faults; }
		}

		public bool HasProtectionLevel {
			get { return has_protection_level; }
		}

		public bool IsInitiating {
			get { return is_initiating; }
			set { is_initiating = value; }
		}

		public bool IsOneWay {
			get { return is_oneway; }
			// LAMESPEC: I believe there should be a setter since
			// otherwise OperationContractAttribute.set_IsOneWay() does not make sense.
			internal set { is_oneway = value; }
		}

		public bool IsTerminating {
			get { return is_terminating; }
			set { is_terminating = value; }
		}

		public Collection<Type> KnownTypes {
			get { return known_types; }
		}

		public MessageDescriptionCollection Messages {
			get { return messages; }
		}

		public string Name {
			get { return name; }
		}

		public ProtectionLevel ProtectionLevel {
			get { return protection_level; }
			set {
				protection_level = value;
				has_protection_level = true;
			}
		}

		public MethodInfo SyncMethod {
			get { return sync_method; }
			set { sync_method = value; }
		}

		#region internal members for moonlight compatibility

		internal XmlName XmlName {
			get; private set;
		}

		internal object FormatterBehavior { get; set; }

		#endregion
	}
}
