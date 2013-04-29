using System;
using System.Reflection;

public struct A
{
	public override string ToString ()
	{
		return "A";
	}
}

public class D
{
	public string Test ()
	{
		return "Test";
	}
}

enum Enum1
{
	A,
	B
}

enum Enum2
{
	C,
	D
}

class Tests
{
	public static Enum1 return_enum1 () {
		return Enum1.A;
	}

	public static Enum2 return_enum2 () {
		return Enum2.C;
	}

	public static long return_long () {
		return 1234;
	}

	public static ulong return_ulong () {
		return UInt64.MaxValue - 5;
	}

	static int Main (string[] args)
	{
		return TestDriver.RunTests (typeof (Tests), args);
	}

	public static int test_0_base () {
		Assembly ass = typeof (Tests).Assembly;
		Type a_type = ass.GetType ("A");
		MethodInfo a_method = a_type.GetMethod ("ToString");

		Type d_type = ass.GetType ("D");
		MethodInfo d_method = d_type.GetMethod ("Test");

		Console.WriteLine ("TEST: {0} {1}", a_method, d_method);

		A a = new A ();
		D d = new D ();

		object a_ret = a_method.Invoke (a, null);
		Console.WriteLine (a_ret);

		object d_ret = d_method.Invoke (d, null);
		Console.WriteLine (d_ret);

		return 0;
	}

	public static int test_0_enum_sharing () {
		/* Check sharing of wrappers returning enums */
		if (typeof (Tests).GetMethod ("return_enum1").Invoke (null, null).GetType () != typeof (Enum1))
			return 1;
		if (typeof (Tests).GetMethod ("return_enum2").Invoke (null, null).GetType () != typeof (Enum2))
			return 2;
		return 0;
	}

	public static int test_0_primitive_sharing () {
		/* Check sharing of wrappers returning primitive types */
		if (typeof (Tests).GetMethod ("return_long").Invoke (null, null).GetType () != typeof (long))
			return 3;
		if (typeof (Tests).GetMethod ("return_ulong").Invoke (null, null).GetType () != typeof (ulong))
			return 4;

		return 0;
	}

	public struct Foo
	{
		public string ToString2 () {
			return "FOO";
		}
	}

	public static object GetNSObject (IntPtr i) {
		return i;
	}

	public static int test_0_vtype_method_sharing () {
		/* Check sharing of wrappers of vtype methods with static methods having an IntPtr argument */
		if ((string)(typeof (Foo).GetMethod ("ToString2").Invoke (new Foo (), null)) != "FOO")
			return 3;
		object o = typeof (Tests).GetMethod ("GetNSObject").Invoke (null, new object [] { new IntPtr (42) });
		if (!(o is IntPtr) || ((IntPtr)o != new IntPtr (42)))
			return 4;

		return 0;
	}

	public static unsafe int test_0_ptr () {
		int[] arr = new int [10];
		fixed (void *p = &arr [5]) {
			object o = typeof (Tests).GetMethod ("data_types_ptr").Invoke (null, new object [1] { new IntPtr (p) });
			void *p2 = Pointer.Unbox (o);
			if (new IntPtr (p) != new IntPtr (p2))
				return 1;

			o = typeof (Tests).GetMethod ("data_types_ptr").Invoke (null, new object [1] { null });
			p2 = Pointer.Unbox (o);
			if (new IntPtr (p2) != IntPtr.Zero)
				return 1;
		}

		return 0;
	}

	public static int test_0_string_ctor () {
		string res = (string)typeof (String).GetConstructor (new Type [] { typeof (char[]) }).Invoke (new object [] { new char [] { 'A', 'B', 'C' } });
		if (res == "ABC")
			return 0;
		else
			return 1;
	}

	public class Foo<T> {
		public T t;
	}

	public static int test_0_ginst_ref () {
		Foo<string> f = new Foo<string> { t = "A" };
		Foo<string> f2 = (Foo<string>)typeof (Tests).GetMethod ("data_types_ginst_ref").MakeGenericMethod (new Type [] { typeof (string) }).Invoke (null, new object [] { f });
		if (f2.t != "A")
			return 1;
		else
			return 0;
	}

	public static int test_0_ginst_vtype () {
		FooStruct<string> f = new FooStruct<string> { t = "A" };
		FooStruct<string> f2 = (FooStruct<string>)typeof (Tests).GetMethod ("data_types_ginst_vtype").MakeGenericMethod (new Type [] { typeof (string) }).Invoke (null, new object [] { f });
		if (f2.t != "A")
			return 1;
		else
			return 0;
	}

	public static Foo<T> data_types_ginst_ref<T> (Foo<T> f) {
		return f;
	}

	public struct FooStruct<T> {
		public T t;
	}

	public static FooStruct<T> data_types_ginst_vtype<T> (FooStruct<T> f) {
		return f;
	}

    public static unsafe int* data_types_ptr (int *val) {
		//Console.WriteLine (new IntPtr (val));
        return val;
    }

	public static bool pack_u2 (ushort us) {
		return true;
	}

	public static bool pack_i2 (short value) {
		int c = 0;
		// Force 'value' to be register allocated
		for (int i = 0; i < value; ++i)
			c += value;
	  return value < 0x80;
	}

	// #11750
	public static int test_0_i2_u2 () {
		typeof (Tests).GetMethod ("pack_u2").Invoke (null, new object [] { (ushort)0 });
		var res = typeof (Tests).GetMethod ("pack_i2").Invoke (null, new object [] { (short)-1 });
		return (bool)res ? 0 : 1;
	}

	public static bool pack_bool (bool b) {
		return true;
	}

	public static bool pack_i1 (sbyte value) {
		int c = 0;
		// Force 'value' to be register allocated
		for (int i = 0; i < value; ++i)
			c += value;
		return value < -1;
	}

	public static int test_0_i1_bool () {
		typeof (Tests).GetMethod ("pack_bool").Invoke (null, new object [] { true });
		var res = typeof (Tests).GetMethod ("pack_i1").Invoke (null, new object [] { (sbyte)-0x40 });
		return (bool)res ? 0 : 1;
	}
}
