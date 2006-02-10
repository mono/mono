using System;

class Tests {

	struct TestStruct {
		public int i;

		public TestStruct (int i) {
			this.i = i;
		}
	}

	static int Main ()
	{
		return TestDriver.RunTests (typeof (Tests));
	}

	public static int test_1_nullable_unbox ()
	{
		return Unbox<int?> (1).Value;
	}

	public static int test_1_nullable_unbox_null ()
	{
		return Unbox<int?> (null).HasValue ? 0 : 1;
	}

	public static int test_1_nullable_box ()
	{
		return (int) Box<int?> (1);
	}

	public static int test_1_nullable_box_null ()
	{
		return Box<int?> (null) == null ? 1 : 0;
	}

	public static int test_1_isinst_nullable ()
	{
		object o = 1;
		return (o is int?) ? 1 : 0;
	}

	public static int test_1_nullable_unbox_vtype ()
	{
		return Unbox<TestStruct?> (new TestStruct (1)).Value.i;
	}

	public static int test_1_nullable_unbox_null_vtype ()
	{
		return Unbox<TestStruct?> (null).HasValue ? 0 : 1;
	}

	public static int test_1_nullable_box_vtype ()
	{
		return ((TestStruct)(Box<TestStruct?> (new TestStruct (1)))).i;
	}

	public static int test_1_nullable_box_null_vtype ()
	{
		return Box<TestStruct?> (null) == null ? 1 : 0;
	}

	public static int test_1_isinst_nullable_vtype ()
	{
		object o = new TestStruct (1);
		return (o is TestStruct?) ? 1 : 0;
	}

	public static int test_0_nullable_normal_unbox ()
	{
		int? i = 5;

		object o = i;
		// This uses unbox instead of unbox_any
		int? j = (int?)o;

		if (j != 5)
			return 1;

		return 0;
	}

	public static void stelem_any<T> (T[] arr, T elem) {
		arr [0] = elem;
	}

	public static T ldelem_any<T> (T[] arr) {
		return arr [0];
	}

	public static int test_1_ldelem_stelem_any_int () {
		int[] arr = new int [3];
		stelem_any (arr, 1);

		return ldelem_any (arr);
	}

	interface ITest
	{
		void Foo<T> ();
	}

	public static int test_0_iface_call_null_bug_77442 () {
		ITest test = null;

		try {
			test.Foo<int> ();
		}
		catch (NullReferenceException) {
			return 0;
		}
		
		return 1;
	}

	public static int test_18_ldobj_stobj_generics () {
		GenericClass<int> t = new GenericClass <int> ();
		int i = 5;
		int j = 6;
		return t.ldobj_stobj (ref i, ref j) + i + j;
	}

	public static int test_5_ldelem_stelem_generics () {
		GenericClass<TestStruct> t = new GenericClass<TestStruct> ();

		TestStruct s = new TestStruct (5);
		return t.ldelem_stelem (s).i;
	}

	public static int test_0_constrained_vtype_box () {
		GenericClass<TestStruct> t = new GenericClass<TestStruct> ();

		return t.toString (new TestStruct ()) == "Tests+TestStruct" ? 0 : 1;
	}

	public static int test_0_constrained_vtype () {
		GenericClass<int> t = new GenericClass<int> ();

		return t.toString (1234) == "1234" ? 0 : 1;
	}

	public static int test_0_constrained_reftype () {
		GenericClass<String> t = new GenericClass<String> ();

		return t.toString ("1234") == "1234" ? 0 : 1;
	}

	static object Box<T> (T t)
	{
		return t;
	}
	
	static T Unbox <T> (object o) {
		return (T) o;
	}

	class GenericClass <T> {
		public T ldobj_stobj (ref T t1, ref T t2) {
			t1 = t2;
			T t = t1;

			return t;
		}

		public T ldelem_stelem (T t) {
			T[] arr = new T [10];
			arr [0] = t;

			return arr [0];
		}

		public String toString (T t) {
			return t.ToString ();
		}
	}
}
