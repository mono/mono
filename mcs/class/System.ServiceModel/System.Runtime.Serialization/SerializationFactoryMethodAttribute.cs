#if NET_2_0
using System;

namespace System.Runtime.Serialization
{
	[AttributeUsage (AttributeTargets.Method,
		Inherited = false, AllowMultiple = false)]
	public sealed class SerializationFactoryMethodAttribute : Attribute
	{
		public SerializationFactoryMethodAttribute ()
		{
		}
	}
}
#endif
