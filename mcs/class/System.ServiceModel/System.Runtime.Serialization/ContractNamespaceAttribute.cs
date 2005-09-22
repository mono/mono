using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Module,
		Inherited = false, AllowMultiple = true)]
	public sealed class ContractNamespaceAttribute : Attribute
	{
		string clrNS, contractNS;

		public ContractNamespaceAttribute ()
		{
		}

		public string ClrNamespace {
			get { return clrNS; }
			set { clrNS = value; }
		}

		public string ContractNamespace {
			get { return contractNS; }
			set { contractNS = value; }
		}
	}
}
