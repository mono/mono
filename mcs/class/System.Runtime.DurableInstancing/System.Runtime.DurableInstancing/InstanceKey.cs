using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public class InstanceKey
	{
		static InstanceKey ()
		{
			InvalidKey = new InstanceKey ();
		}
		
		public static InstanceKey InvalidKey { get; private set; }

		InstanceKey ()
		{
		}

		public InstanceKey (Guid value)
			: this (value, new Dictionary<XName, InstanceValue> ())
		{
		}

		public InstanceKey (Guid value, IDictionary<XName, InstanceValue> metadata)
		{
		}

		public bool IsValid {
			get { return this == InvalidKey; }
		}

		public IDictionary<XName, InstanceValue> Metadata { get; private set; }

		public Guid Value { get; private set; }

		public override bool Equals (object obj)
		{
			var o = obj as InstanceKey;
			return o != null && Value == o.Value;
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}
	}
}
