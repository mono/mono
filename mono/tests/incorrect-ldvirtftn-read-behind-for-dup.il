/*
Based on this C#, which is based on MSDN.
public class A
{
	public delegate void D();

	public class B
	{
		int32 i = 1;

		public void Instance ()
		{
			System.Console.WriteLine("Instance {0}", i);
		}

		public static void Static ()
		{
			System.Console.WriteLine("Static {0}", 2);
		}

		public virtual void Virtual ()
		{
			System.Console.WriteLine("Virtual {0}", i + 2);
		}
	}

	public static void Main()
	{
		var b = new B ();
		var d1 = new D (b.Instance);
		var d2 = new D (B.Static);
		var d3 = new D (b.Virtual);
		d1 ();
		d2 ();
		d3 ();
	}
}
*/
.assembly extern mscorlib { }
.assembly '1' { }
.class public A
{

.method public static void Main ()
{
	.entrypoint
	.maxstack 99
	.locals init (
		class A/B v0, class A/D v1, class A/D v2, class A/D v3,
		// pad out locals to hit the assert
		int32 v00, int32 v01, int32 v02, int32 v03, int32 v04, int32 v05, int32 v06, int32 v07,
		int32 v08, int32 v09, int32 v0A, int32 v0B, int32 v0C, int32 v0D, int32 v0E, int32 v0F,
		int32 v10, int32 v11, int32 v12, int32 v13, int32 v14, int32 v15, int32 v16, int32 v17,
		int32 v18, int32 v19, int32 v1A, int32 v1B, int32 v1C, int32 v1D, int32 v1E, int32 v1F,
		int32 v20, int32 v21, int32 v22, int32 v23, int32 v24, int32 v25, int32 v26, int32 v27
	)

	// v0 = new B ()
	newobj instance void class A/B::'.ctor'()
	stloc v0

	// v1 = new D (v0, B::Instance)
	ldloc v0
	ldftn instance void class A/B::Instance()
	newobj instance void class A/D::'.ctor'(object, native int)
	stloc v1

	// v2 = new D (null, B::Static)
	ldnull
	ldftn void class A/B::Static()
	newobj instance void class A/D::'.ctor'(object, native int)
	stloc v2

	// v3 = new D (v0, {v0, B::Virtual})
	ldloc v0
	dup		// the dup the code is/was looking back for
// dup is 0x25
// Let's put in 0x25 that isn't dup, and mess up the compile-time but not runtime stack
	ldc.i4 v0
	dup
	stloc.s 0x25
	stloc.s 0x25

	ldvirtftn instance void class A/B::Virtual()
	newobj instance void class A/D::'.ctor'(object, native int)
	stloc v3

	// v1 ()
	ldloc v1
	callvirt instance void class A/D::Invoke()

	// v2 ()
	ldloc v2
	callvirt instance void class A/D::Invoke()

	// v3 ()
	ldloc v3
	callvirt instance void class A/D::Invoke()

	ret
}

.method public instance void '.ctor' ()
{
	ldarg 0
	call instance void object::'.ctor'()
	ret
}

.class nested public auto ansi sealed D
extends [mscorlib]System.MulticastDelegate
{

.method public void '.ctor' (object 'object', native int 'method') runtime
{
}

.method public virtual newslot instance void Invoke () runtime
{
}

.method public virtual newslot instance class [mscorlib]System.IAsyncResult BeginInvoke (class [mscorlib]System.AsyncCallback callback, object 'object') runtime
{
}

.method public virtual newslot instance void EndInvoke (class [mscorlib]System.IAsyncResult result) runtime
{
}
}

.class nested public B
{
	.field int32 i

.method public instance void Instance ()
{
	.maxstack 99

	ldstr "Instance {0}"
	ldarg 0
	ldfld int32 A/B::i
	box [mscorlib]System.Int32
	call void class [mscorlib]System.Console::WriteLine(string, object)
	ret
}

.method public static void Static ()
{
	.maxstack 99

	ldstr "Static {0}"
	ldc.i4 2
	box [mscorlib]System.Int32
	call void class [mscorlib]System.Console::WriteLine(string, object)
	ret
}

.method public virtual newslot instance void Virtual ()
{
	.maxstack 99

	ldstr "Virtual {0}"
	ldarg 0
	ldfld int32 A/B::i
	ldc.i4 2
	add
	box [mscorlib]System.Int32
	call void class [mscorlib]System.Console::WriteLine(string, object)
	ret
}

.method public instance void '.ctor' ()
{
	.maxstack 99

	ldarg 0
	ldc.i4 1
	stfld int32 A/B::i
	ldarg 0
	call instance void object::'.ctor'()
	ret
}
}
}
