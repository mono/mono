namespace System.Reflection.Emit
{
	abstract class TypeBuilderInstantiation : TypeInfo
	{
		internal static Type MakeGenericType (Type type, Type[] typeArguments)
		{
#if FULL_AOT_RUNTIME
			throw new NotSupportedException ("User types are not supported under full aot");
#else
			return new MonoGenericClass (type, typeArguments);
#endif
		}
	}
}