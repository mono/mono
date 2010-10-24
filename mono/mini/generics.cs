using System;
using System.Collections.Generic;
using System.Linq;

class Tests {

	struct TestStruct {
		public int i;
		public int j;

		public TestStruct (int i, int j) {
			this.i = i;
			this.j = j;
		}
	}

	class Enumerator <T> : MyIEnumerator <T> {
		T MyIEnumerator<T>.Current {
			get {
				return default(T);
			}
		}

		bool MyIEnumerator<T>.MoveNext () {
			return true;
		}
	}

	class Comparer <T> : IComparer <T> {
		bool IComparer<T>.Compare (T x, T y) {
			return true;
		}
	}

	static int Main (string[] args)
	{
		return TestDriver.RunTests (typeof (Tests), args);
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
		return Unbox<TestStruct?> (new TestStruct (1, 2)).Value.i;
	}

	public static int test_1_nullable_unbox_null_vtype ()
	{
		return Unbox<TestStruct?> (null).HasValue ? 0 : 1;
	}

	public static int test_1_nullable_box_vtype ()
	{
		return ((TestStruct)(Box<TestStruct?> (new TestStruct (1, 2)))).i;
	}

	public static int test_1_nullable_box_null_vtype ()
	{
		return Box<TestStruct?> (null) == null ? 1 : 0;
	}

	public static int test_1_isinst_nullable_vtype ()
	{
		object o = new TestStruct (1, 2);
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

	public static T return_ref<T> (ref T t) {
		return t;
	}

	public static T ldelema_any<T> (T[] arr) {
		return return_ref<T> (ref arr [0]);
	}

	public static int test_0_ldelema () {
		string[] arr = new string [1];

		arr [0] = "Hello";

		if (ldelema_any <string> (arr) == "Hello")
			return 0;
		else
			return 1;
	}

	public static T[,] newarr_multi<T> () {
		return new T [1, 1];
	}

	public static int test_0_newarr_multi_dim () {
		return newarr_multi<string> ().GetType () == typeof (string[,]) ? 0 : 1;
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

		TestStruct s = new TestStruct (5, 5);
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

	public static int test_0_box_brtrue_optimizations () {
		if (IsNull<int>(5))
			return 1;

		if (!IsNull<object>(null))
			return 1;

		return 0;
	}

	[Category ("!FULLAOT")]
	public static int test_0_generic_get_value_optimization_int () {
		int[] x = new int[] {100, 200};

		if (GenericClass<int>.Z (x, 0) != 100)
			return 2;

		if (GenericClass<int>.Z (x, 1) != 200)
			return 3;

		return 0;
	}

	public static int test_0_generic_get_value_optimization_vtype () {
		TestStruct[] arr = new TestStruct[] { new TestStruct (100, 200), new TestStruct (300, 400) };
		IEnumerator<TestStruct> enumerator = GenericClass<TestStruct>.Y (arr);
		TestStruct s;
		int sum = 0;
		while (enumerator.MoveNext ()) {
			s = enumerator.Current;
			sum += s.i + s.j;
		}

		if (sum != 1000)
			return 1;

		s = GenericClass<TestStruct>.Z (arr, 0);
		if (s.i != 100 || s.j != 200)
			return 2;

		s = GenericClass<TestStruct>.Z (arr, 1);
		if (s.i != 300 || s.j != 400)
			return 3;

		return 0;
	}

	public static int test_0_nullable_ldflda () {
		return GenericClass<string>.BIsAClazz == false ? 0 : 1;
	}

	public struct GenericStruct<T> {
		public T t;

		public GenericStruct (T t) {
			this.t = t;
		}
	}

	public class GenericClass<T> {
		public T t;

		public GenericClass (T t) {
			this.t = t;
		}

		public GenericClass () {
		}

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

		public static IEnumerator<T> Y (IEnumerable <T> x)
		{
			return x.GetEnumerator ();
		}

		public static T Z (IList<T> x, int index)
		{
			return x [index];
		}

        protected static T NullB = default(T);       
        private static Nullable<bool>  _BIsA = null;
        public static bool BIsAClazz {
            get {
                _BIsA = false;
                return _BIsA.Value;
            }
        }
	}

	public class MRO : MarshalByRefObject {
		public GenericStruct<int> struct_field;
		public GenericClass<int> class_field;
	}

	public static int test_0_ldfld_stfld_mro () {
		MRO m = new MRO ();
		GenericStruct<int> s = new GenericStruct<int> (5);
		// This generates stfld
		m.struct_field = s;

		// This generates ldflda
		if (m.struct_field.t != 5)
			return 1;

		// This generates ldfld
		GenericStruct<int> s2 = m.struct_field;
		if (s2.t != 5)
			return 2;

		if (m.struct_field.t != 5)
			return 3;

		m.class_field = new GenericClass<int> (5);
		if (m.class_field.t != 5)
			return 4;

		return 0;
	}

	// FIXME:
	[Category ("!FULLAOT")]
    public static int test_0_generic_virtual_call_on_vtype_unbox () {
		object o = new Object ();
        IFoo h = new Handler(o);

        if (h.Bar<object> () != o)
			return 1;
		else
			return 0;
    }

	public static int test_0_box_brtrue_opt () {
		Foo<int> f = new Foo<int> (5);

		f [123] = 5;

		return 0;
	}

	public static int test_0_box_brtrue_opt_regress_81102 () {
		if (new Foo<int>(5).ToString () == "null")
			return 0;
		else
			return 1;
	}

	struct S {
		public int i;
	}

	public static int test_0_ldloca_initobj_opt () {
		if (new Foo<S> (new S ()).get_default ().i != 0)
			return 1;
		if (new Foo<object> (null).get_default () != null)
			return 2;
		return 0;
	}

	public static int test_0_variance_reflection () {
		// covariance on IEnumerator
		if (!typeof (MyIEnumerator<object>).IsAssignableFrom (typeof (MyIEnumerator<string>)))
			return 1;
		// covariance on IEnumerator and covariance on arrays
		if (!typeof (MyIEnumerator<object>[]).IsAssignableFrom (typeof (MyIEnumerator<string>[])))
			return 2;
		// covariance and implemented interfaces
		if (!typeof (MyIEnumerator<object>).IsAssignableFrom (typeof (Enumerator<string>)))
			return 3;

		// contravariance on IComparer
		if (!typeof (IComparer<string>).IsAssignableFrom (typeof (IComparer<object>)))
			return 4;
		// contravariance on IComparer, contravariance on arrays
		if (!typeof (IComparer<string>[]).IsAssignableFrom (typeof (IComparer<object>[])))
			return 5;
		// contravariance and interface inheritance
		if (!typeof (IComparer<string>[]).IsAssignableFrom (typeof (IKeyComparer<object>[])))
			return 6;
		return 0;
	}

	public static int test_0_ldvirtftn_generic_method () {
		new Tests ().ldvirtftn<string> ();		

		return the_type == typeof (string) ? 0 : 1;
	}

	public static int test_0_throw_dead_this () {
        new Foo<string> ("").throw_dead_this ();
		return 0;
	}

	struct S<T> {}

	public static int test_0_inline_infinite_polymorphic_recursion () {
           f<int>(0);

		   return 0;
	}

	private static void f<T>(int i) {
		if(i==42) f<S<T>>(i);
	}

	// This cannot be made to work with full-aot, since there it is impossible to
	// statically determine that Foo<string>.Bar <int> is needed, the code only
	// references IFoo.Bar<int>
	[Category ("!FULLAOT")]
	public static int test_0_generic_virtual_on_interfaces () {
		Foo<string>.count1 = 0;
		Foo<string>.count2 = 0;
		Foo<string>.count3 = 0;

		IFoo f = new Foo<string> ("");
		for (int i = 0; i < 1000; ++i) {
			f.Bar <int> ();
			f.Bar <string> ();
			f.NonGeneric ();
		}

		if (Foo<string>.count1 != 1000)
			return 1;
		if (Foo<string>.count2 != 1000)
			return 2;
		if (Foo<string>.count3 != 1000)
			return 3;

		VirtualInterfaceCallFromGenericMethod<long> (f);

		return 0;
	}

	//repro for #505375
	[Category ("!FULLAOT")]
	public static int test_2_cprop_bug () {
		int idx = 0;
		int a = 1;
		var cmp = System.Collections.Generic.Comparer<int>.Default ;
		if (cmp.Compare (a, 0) > 0)
			a = 0;
		do { idx++; } while (cmp.Compare (idx - 1, a) == 0);
		return idx;
	}

	enum MyEnumUlong : ulong {
		Value_2 = 2
	}

	public static int test_0_regress_550964_constrained_enum_long () {
        MyEnumUlong a = MyEnumUlong.Value_2;
        MyEnumUlong b = MyEnumUlong.Value_2;

        return Pan (a, b) ? 0 : 1;
	}

    static bool Pan<T> (T a, T b)
    {
        return a.Equals (b);
    }

	public class XElement {
		public string Value {
			get; set;
		}
	}

	public static int test_0_fullaot_linq () {
		var allWords = new XElement [] { new XElement { Value = "one" } };
		var filteredWords = allWords.Where(kw => kw.Value.StartsWith("T"));
		return filteredWords.Count ();
	}

	public static int test_0_fullaot_comparer_t () {
		var l = new SortedList <TimeSpan, int> ();
		return l.Count;
	}

	public static int test_0_fullaot_comparer_t_2 () {
		var l = new Dictionary <TimeSpan, int> ();
		return l.Count;
	}

	static void enumerate<T> (IEnumerable<T> arr) {
		foreach (var o in arr)
			;
		int c = ((ICollection<T>)arr).Count;
	}

	/* Test that treating arrays as generic collections works with full-aot */
	public static int test_0_fullaot_array_wrappers () {
		Tests[] arr = new Tests [10];
		enumerate<Tests> (arr);
		return 0;
	}

	static int cctor_count = 0;

    public abstract class Beta<TChanged> 
    {		
        static Beta()
        {
			cctor_count ++;
        }
    }   
    
    public class Gamma<T> : Beta<T> 
    {   
        static Gamma()
        {
        }
    }

	// #519336    
	public static int test_2_generic_class_init_gshared_ctor () {
		new Gamma<object>();
		new Gamma<string>();

		return cctor_count;
	}

	static int cctor_count2 = 0;

	class ServiceController<T> {
		static ServiceController () {
			cctor_count2 ++;
		}

		public ServiceController () {
		}
	}

	static ServiceController<T> Create<T>() {
		return new ServiceController<T>();
	}

	// #631409
	public static int test_2_generic_class_init_gshared_ctor_from_gshared () {
		Create<object> ();
		Create<string> ();

		return cctor_count2;
	}

	public static Type get_type<T> () {
		return typeof (T);
	}

	public static int test_0_gshared_delegate_rgctx () {
		Func<Type> t = new Func<Type> (get_type<string>);

		if (t () == typeof (string))
			return 0;
		else
			return 1;
	}

	// Creating a delegate from a generic method from gshared code
	public static int test_0_gshared_delegate_from_gshared () {
		if (gshared_delegate_from_gshared <object> () != 0)
			return 1;
		if (gshared_delegate_from_gshared <string> () != 0)
			return 2;
		return 0;
	}

	public static int gshared_delegate_from_gshared <T> () {
		Func<Type> t = new Func<Type> (get_type<T>);

		return t () == typeof (T) ? 0 : 1;
	}

	public static int test_0_marshalbyref_call_from_gshared_virt_elim () {
		/* Calling a virtual method from gshared code which is changed to a nonvirt call */
		Class1<object> o = new Class1<object> ();
		o.Do (new Class2<object> ());
		return 0;
	}

	class Pair<TKey, TValue> {
		public static KeyValuePair<TKey, TValue> make_pair (TKey key, TValue value)
			{
				return new KeyValuePair<TKey, TValue> (key, value);
			}

		public delegate TRet Transform<TRet> (TKey key, TValue value);
	}

	public static int test_0_bug_620864 () {
		var d = new Pair<string, Type>.Transform<KeyValuePair<string, Type>> (Pair<string, Type>.make_pair);

		var p = d ("FOO", typeof (int));
		if (p.Key != "FOO" || p.Value != typeof (int))
			return 1;

		return 0;
	}

	public static int test_0_partial_sharing () {
		if (PartialShared1 (new List<string> (), 1) != typeof (string))
			return 1;
		if (PartialShared1 (new List<Tests> (), 1) != typeof (Tests))
			return 2;
		if (PartialShared2 (new List<string> (), 1) != typeof (int))
			return 3;
		if (PartialShared2 (new List<Tests> (), 1) != typeof (int))
			return 4;
		return 0;
	}

	public static int test_6_partial_sharing_linq () {
		var messages = new List<Message> ();

		messages.Add (new Message () { MessageID = 5 });
		messages.Add (new Message () { MessageID = 6 });

		return messages.Max(i => i.MessageID);
	}

	public static int test_0_partial_shared_method_in_nonshared_class () {
		var c = new Class1<double> ();
		return (c.Foo<string> (5).GetType () == typeof (Class1<string>)) ? 0 : 1;
	}

	class Message {
		public int MessageID {
			get; set;
		}
	}

	public static Type PartialShared1<T, K> (List<T> list, K k) {
		return typeof (T);
	}

	public static Type PartialShared2<T, K> (List<T> list, K k) {
		return typeof (K);
	}

    public class Class1<T> {
		public virtual void Do (Class2<T> t) {
			t.Foo ();
		}

		public virtual object Foo<U> (T t) {
			return new Class1<U> ();
		}
	}

	public interface IFace1<T> {
		void Foo ();
	}

	public class Class2<T> : MarshalByRefObject, IFace1<T> {
		public void Foo () {
		}
	}



	public static void VirtualInterfaceCallFromGenericMethod <T> (IFoo f) {
		f.Bar <T> ();
	}

	public static Type the_type;

	public void ldvirtftn<T> () {
		Foo <T> binding = new Foo <T> (default (T));

		binding.GenericEvent += event_handler;
		binding.fire ();
	}

	public virtual void event_handler<T> (Foo<T> sender) {
		the_type = typeof (T);
	}

	public interface IFoo {
		void NonGeneric ();
		object Bar<T>();
	}

	public class Foo<T1> : IFoo
	{
		public Foo(T1 t1)
		{
			m_t1 = t1;
		}
		
		public override string ToString()
		{
			return Bar(m_t1 == null ? "null" : "null");
		}

		public String Bar (String s) {
			return s;
		}

		public int this [T1 key] {
			set {
				if (key == null)
					throw new ArgumentNullException ("key");
			}
		}

		public void throw_dead_this () {
			try {
				new SomeClass().ThrowAnException();
			}
			catch {
			}
		}

		public T1 get_default () {
			return default (T1);
		}
		
		readonly T1 m_t1;

		public delegate void GenericEventHandler (Foo<T1> sender);

		public event GenericEventHandler GenericEvent;

		public void fire () {
			GenericEvent (this);
		}

		public static int count1, count2, count3;

		public void NonGeneric () {
			count3 ++;
		}

		public object Bar <T> () {
			if (typeof (T) == typeof (int))
				count1 ++;
			else if (typeof (T) == typeof (string))
				count2 ++;
			return null;
		}
	}

	public class SomeClass {
		public void ThrowAnException() {
			throw new Exception ("Something went wrong");
		}
	}		

	struct Handler : IFoo {
		object o;

		public Handler(object o) {
			this.o = o;
		}

		public void NonGeneric () {
		}

		public object Bar<T>() {
			return o;
		}
	}

	static bool IsNull<T> (T t)
	{
		if (t == null)
			return true;
		else
			return false;
	}

	static object Box<T> (T t)
	{
		return t;
	}
	
	static T Unbox <T> (object o) {
		return (T) o;
	}
}
