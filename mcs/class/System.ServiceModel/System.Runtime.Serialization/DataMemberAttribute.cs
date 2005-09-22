#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field,
		Inherited = false, AllowMultiple = false)]
	public sealed class DataMemberAttribute : Attribute
	{
		bool is_required;
		string name;
		int order;

		public DataMemberAttribute ()
		{
		}

		public bool IsRequired {
			get { return is_required; }
			set { is_required = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public int Order {
			get { return order; }
			set { order = value; }
		}
	}
}
#endif
