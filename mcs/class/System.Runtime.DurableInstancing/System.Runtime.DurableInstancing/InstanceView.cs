using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceView
	{
		internal InstanceView (Guid instanceId, InstanceOwner owner)
		{
		}
		
		public IDictionary<XName, InstanceValue> InstanceData { get; internal set; }
		public InstanceValueConsistency InstanceDataConsistency { get; internal set; }
		public Guid InstanceId { get; private set; }
		public IDictionary<Guid, InstanceKeyView> InstanceKeys { get; internal set; }
		public InstanceValueConsistency InstanceKeysConsistency { get; internal set; }
		public IDictionary<XName, InstanceValue> InstanceMetadata { get; internal set; }
		public InstanceValueConsistency InstanceMetadataConsistency { get; internal set; }
		public InstanceOwner InstanceOwner { get; private set; }
		public IDictionary<XName, InstanceValue> InstanceOwnerMetadata { get; internal set; }
		public InstanceValueConsistency InstanceOwnerMetadataConsistency { get; internal set; }
		public InstanceState InstanceState { get; internal set; }
		public ReadOnlyCollection<InstanceStoreQueryResult> InstanceStoreQueryResults { get; internal set; }

		public bool IsBoundToInstance {
			get { return InstanceId == Guid.Empty; }
		}

		public bool IsBoundToInstanceOwner {
			get { return InstanceOwner == null; }
		}

		public bool IsBoundToLock { get { throw new NotImplementedException (); } }
	}
}
