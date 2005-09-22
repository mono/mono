#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field,
		Inherited = false, AllowMultiple = true)]
	public sealed class KnownTypeAttribute : Attribute
	{
		string method_name;
		Type type;

		public KnownTypeAttribute (string methodName)
		{
			method_name = methodName;
		}

		public KnownTypeAttribute (Type type)
		{
			this.type = type;
		}

		public string MethodName {
			get { return method_name; }
		}

		public Type Type {
			get { return type; }
		}
	}
}
#endif
