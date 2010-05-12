using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceOwnerQueryResult : InstanceStoreQueryResult
	{
		public InstanceOwnerQueryResult ()
			: this (new Dictionary<Guid, IDictionary<XName, InstanceValue>> ())
		{
		}

		public InstanceOwnerQueryResult (IDictionary<Guid, IDictionary<XName, InstanceValue>> instanceOwners)
		{
			if (instanceOwners == null)
				throw new ArgumentNullException ("instanceOwners");
			InstanceOwners = instanceOwners;
		}

		public InstanceOwnerQueryResult (Guid instanceOwnerId, IDictionary<XName, InstanceValue> metadata)
			: this ()
		{
			InstanceOwners [instanceOwnerId] = metadata;
		}

		public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceOwners { get; private set; }
	}
}
