using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field,
		Inherited = false, AllowMultiple = true)]
	public sealed class KnownTypeAttribute : Attribute
	{
		string methodName;
		Type type;

		public KnownTypeAttribute (string methodName)
		{
		}

		public KnownTypeAttribute (Type type)
		{
		}

		public string MethodName {
			get { return methodName; }
		}

		public Type Type {
			get { return type; }
		}
	}
}
