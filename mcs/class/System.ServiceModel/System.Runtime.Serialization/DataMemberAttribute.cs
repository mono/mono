using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field,
		Inherited = false, AllowMultiple = false)]
	public sealed class DataMemberAttribute : Attribute
	{
		bool isRequired;
		string name;
		int order;

		public DataMemberAttribute ()
		{
		}

		public bool IsRequired {
			get { return isRequired; }
			set { isRequired = value; }
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
