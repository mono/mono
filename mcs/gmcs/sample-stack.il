.assembly sample-stack {}

.namespace Generics
{
	.class interface public abstract Foo
	{
		.method public virtual hidebysig newslot abstract instance default void Hello(class Generics.Stack<int32> stack)
		{
		}
	}

	.class public beforefieldinit Driver extends [mscorlib]System.Object
	{
		.field static public class Generics.Stack<int32> int_stack
		.field static public class Generics.Stack<string> string_stack

		.method public hidebysig specialname rtspecialname instance default void .ctor()
		{
			ldarg.0
			call instance void valuetype [corlib]System.Object::.ctor()
			ret
		}

		.method public static void Init (class Generics.Foo foo)
		{
			newobj instance void class Generics.Stack<int32>::.ctor ()
			stsfld class Generics.Stack<int32> Generics.Driver::int_stack

			newobj instance void class Generics.Stack<string>::.ctor ()
			stsfld class Generics.Stack<string> Generics.Driver::string_stack

			ldarg.0
			ldsfld class Generics.Stack<int32> Generics.Driver::int_stack
			callvirt instance void class Generics.Foo::Hello(class Generics.Stack<int32>)

			ret
		}
	}

	.class public Stack<T> extends [mscorlib]System.Object
	{
		.field private int32 size
		.field private !0[] data

		.method public instance void .ctor ()
		{
			ldarg.0
			call instance void [mscorlib]System.Object::.ctor ()
			ldarg.0
			ldc.i4.s 100
			newarr !0
			stfld !0[] class Generics.Stack<!0>::data
			ldarg.0
			ldc.i4.m1
			stfld int32 class Generics.Stack<!0>::size
			ret
		}

		.method public instance void Push (!0 item)
		{
			.locals init (int32 temp)

			ldarg.0
			ldfld !0[] class Generics.Stack<!0>::data
			ldarg.0
			dup

			ldfld int32 class Generics.Stack<!0>::size
			ldc.i4.1
			add
			dup
			stloc.0
			stfld int32 class Generics.Stack<!0>::size
			ldloc.0
			ldarg.1
			stelem !0
			ret
		}
		   
		.method public instance !0 Pop ()
		{
			.locals init (!0, int32)

			ldarg.0
			ldfld !0[] class Generics.Stack<!0>::data
			ldarg.0
			dup

			ldfld int32 class Generics.Stack<!0>::size
			dup
			stloc.1
			ldc.i4.1
			sub
			stfld int32 class Generics.Stack<!0>::size
			ldloc.1
			ldelem !0
			stloc.0
			ldloc.0

			ret
		}
	}
}
