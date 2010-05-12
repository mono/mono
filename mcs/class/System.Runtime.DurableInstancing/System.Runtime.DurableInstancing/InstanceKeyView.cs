using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceKeyView
	{
		internal InstanceKeyView (Guid instanceKey)
		{
			InstanceKey = instanceKey;
		}

		public Guid InstanceKey { get; private set; }
		public IDictionary<XName, InstanceValue> InstanceKeyMetadata { get; internal set; }
		public InstanceValueConsistency InstanceKeyMetadataConsistency { get; internal set; }
		public InstanceKeyState InstanceKeyState { get; internal set; }
	}
}
