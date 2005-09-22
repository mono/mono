using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, 
		Inherited = false, AllowMultiple = false)]
	public sealed class DataContractAttribute : Attribute
	{
		string name, ns;

		public DataContractAttribute ()
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
