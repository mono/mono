using System;
using System.Collections;
using System.Runtime.CompilerServices;

//
// This assembly is not AOT-ed, so all calls into it transition to the interpreter
//

public struct FooStruct {
	int i1, i2, i3, i4, i5, i6;
}

public interface InterpOnlyIFace
{
	int get_Field2 ();

	Type virt<T> ();
	Type virt2 (FooStruct s, FooStruct s2);
}

public class InterpOnly : InterpOnlyIFace
{
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int entry_1 (int i) {
		return i + 1;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static ArrayList corlib_call () {
		return new ArrayList (5);
	}

	public virtual int get_Field2 () {
		return 1;
	}

	public virtual Type virt<T> () {
		return typeof(T);
	}

	public virtual Type virt2 (FooStruct s, FooStruct s2) {
		return typeof(FooStruct);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int throw_into_interp () {
		try {
			Array.Sort (null);
			return 1;
		} catch (Exception ex) {
			return 0;
		}
		return 2;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int throw_from_interp () {
		throw new ArgumentNullException ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Func<int, int> create_del () {
		return InterpOnly.entry_1;
	}
}

public struct InterpOnlyStruct : InterpOnlyIFace
{
	public int Field;

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public int get_Field () {
		return Field;
	}

	public int get_Field2 () {
		return Field;
	}

	public Type virt<T> () {
		return typeof(T);
	}

	public Type virt2 (FooStruct s, FooStruct s2) {
		return typeof(FooStruct);
	}
}
