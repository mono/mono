using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Mono
{
	static class ReflectionEmitGate
	{
#if !FULL_AOT_RUNTIME
		[MonoLinkerConditional (MonoLinkerFeatures.ReflectionEmit, MonoLinkerConditionalAction.Return)]
		public static bool IsTypeBuilder (object instance)
		{
			return instance is TypeBuilder;
		}

		[MonoLinkerConditional (MonoLinkerFeatures.ReflectionEmit, MonoLinkerConditionalAction.Throw)]
		public static Type MakeGenericType (Type type, Type[] instantiation)
		{
			return TypeBuilderInstantiation.MakeGenericType (type, instantiation);
		}

		[MonoLinkerConditional (MonoLinkerFeatures.ReflectionEmit, MonoLinkerConditionalAction.Throw)]
		public static MethodInfo CreateMethodOnTypeBuilderInst (RuntimeMethodInfo rmi, Type[] methodInstantiation)
		{
			return new MethodOnTypeBuilderInst (rmi, methodInstantiation);
		}
#endif
	}
}
