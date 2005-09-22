#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Module,
		Inherited = false, AllowMultiple = true)]
	public sealed class ContractNamespaceAttribute : Attribute
	{
		string clr_ns, contract_ns;

		public ContractNamespaceAttribute ()
		{
		}

		public string ClrNamespace {
			get { return clr_ns; }
			set { clr_ns = value; }
		}

		public string ContractNamespace {
			get { return contract_ns; }
			set { contract_ns = value; }
		}
	}
}
#endif
