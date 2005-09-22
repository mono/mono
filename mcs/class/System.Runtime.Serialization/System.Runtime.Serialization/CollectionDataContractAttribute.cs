#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct, 
		Inherited = false, AllowMultiple = false)]
	public sealed class CollectionDataContractAttribute : Attribute
	{
		string name, ns;

		public CollectionDataContractAttribute ()
		{
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}
	}
}
#endif
