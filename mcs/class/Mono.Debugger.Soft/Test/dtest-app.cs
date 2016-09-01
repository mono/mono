/*
 * dtest-app.cs:
 *
 *   Application program used by the debugger tests.
 */
using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using MonoTests.Helpers;

public class TestsBase
{
#pragma warning disable 0414
#pragma warning disable 0169
	public int base_field_i;
	public string base_field_s;
	static int base_static_i = 57;
	static string base_static_s = "C";
#pragma warning restore 0414
#pragma warning restore 0169

	public virtual string virtual_method () {
		return "V1";
	}
}

public enum AnEnum {
	A = 0,
	B= 1
}

public sealed class Tests3 {
	public static void M1 () {
	}

	static void M2 () {
	}

	public void M3 () {
	}

	void M4 () {
	}

}

public static class Tests4 {
	static Tests4 () {
	}
}

public class AAttribute : Attribute {
	public int afield;
}

public class BAttribute : AAttribute {
	public int bfield;
}

[DebuggerDisplay ("Tests", Name="FOO", Target=typeof (int))]
[DebuggerTypeProxy (typeof (Tests))]
[BAttribute (afield = 1, bfield = 2)]
public class Tests2 {
	[DebuggerBrowsableAttribute (DebuggerBrowsableState.Collapsed)]
	public int field_j;
	public static int static_field_j;

	[DebuggerBrowsableAttribute (DebuggerBrowsableState.Collapsed)]
	public int AProperty {
		get {
			return 0;
		}
	}

	public void invoke () {
	}
}

public struct AStruct : ITest2 {
	public int i;
	public string s;
	public byte k;
	public IntPtr j;
	public int l;
/*
	public AStruct () {
		i = 0;
		s = null;
		k = 0;
		j = IntPtr.Zero;
		l = 0;
	}
*/
	public AStruct (int arg) {
		i = arg;
		s = null;
		k = 0;
		j = IntPtr.Zero;
		l = 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public int foo (int val) {
		return val;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int static_foo (int val) {
		return val;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public int invoke_return_int () {
		return i;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int invoke_static () {
		return 5;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public IntPtr invoke_return_intptr () {
		return j;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_mutate () {
		l = 5;
	}

	public int invoke_iface () {
		return i;
	}

	public override string ToString () {
		return i.ToString ();
	}
}

public class GClass<T> {
	public T field;
	public static T static_field;

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public GClass () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void bp<T2> () {
	}
}

public struct GStruct<T> {
	public T i;

	public int j;

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public int invoke_return_int () {
		return j;
	}
}

public struct NestedStruct {
	NestedInner nested1, nested2;
}

public struct NestedInner {
}

public interface IRecStruct {
	void foo (object o);
}

struct RecStruct : IRecStruct {
	public object o;

	public void foo (object o) {
		this.o = o;
	}
}

interface ITest
{
	void Foo ();
	void Bar ();
}

interface ITest<T>
{
	void Foo ();
	void Bar ();
}

class TestIfaces : ITest
{
	void ITest.Foo () {
	}

	void ITest.Bar () {
	}

	TestIfaces<int> Baz () {
		return null;
	}
}

class TestIfaces<T> : ITest<T>
{
	void ITest<T>.Foo () {
	}

	void ITest<T>.Bar () {
	}
}

public interface ITest2
{
	int invoke_iface ();
}

public class Tests : TestsBase, ITest2
{
#pragma warning disable 0414
	int field_i;
	string field_s;
	AnEnum field_enum;
	bool field_bool1, field_bool2;
	char field_char;
	byte field_byte;
	sbyte field_sbyte;
	short field_short;
	ushort field_ushort;
	long field_long;
	ulong field_ulong;
	float field_float;
	double field_double;
	Thread field_class;
	IntPtr field_intptr;
	int? field_nullable;
	static int static_i = 55;
	static string static_s = "A";
	public const int literal_i = 56;
	public const string literal_s = "B";
	public object child;
	public AStruct field_struct;
	public object field_boxed_struct;
	public GStruct<int> generic_field_struct;
	public KeyValuePair<int, object> boxed_struct_field;
	[ThreadStatic]
	public static int tls_i;
	public static bool is_attached = Debugger.IsAttached;
	public NestedStruct nested_struct;

#pragma warning restore 0414

	public class NestedClass {
	}

	public int IntProperty {
		get {
			return field_i;
		}
		set {
			field_i = value;
		}
	}

	public int ReadOnlyProperty {
		get {
			return field_i;
		}
	}

	public int this [int index] {
		get {
			return field_i;
		}
	}

	public static void wait_one ()
	{
		ManualResetEvent evt = new ManualResetEvent (false);
		evt.WaitOne ();
	}

	public static int Main (String[] args) {
		tls_i = 42;

		if (args.Length > 0 && args [0] == "suspend-test")
			/* This contains an infinite loop, so execute it conditionally */
			suspend ();
		if (args.Length >0 && args [0] == "unhandled-exception") {
			unhandled_exception ();
			return 0;
		}
		if (args.Length >0 && args [0] == "unhandled-exception-endinvoke") {
			unhandled_exception_endinvoke ();
			return 0;
		}
		if (args.Length >0 && args [0] == "unhandled-exception-user") {
			unhandled_exception_user ();
			return 0;
		}
		if (args.Length >0 && args [0] == "wait-one") {
			wait_one ();
			return 0;
		}
		if (args.Length >0 && args [0] == "threadpool-io") {
			threadpool_io ();
			return 0;
		}
		breakpoints ();
		single_stepping ();
		arguments ();
		objects ();
		objrefs ();
		vtypes ();
		locals ();
		line_numbers ();
		type_info ();
		assembly_load ();
		invoke ();
		exceptions ();
		exception_filter ();
		threads ();
		dynamic_methods ();
		user ();
		type_load ();
		regress ();
		gc_suspend ();
		set_ip ();
		step_filters ();
		local_reflect ();
		if (args.Length > 0 && args [0] == "domain-test")
			/* This takes a lot of time, so execute it conditionally */
			domains ();
		if (args.Length > 0 && args [0] == "ref-emit-test")
			ref_emit ();
		if (args.Length > 0 && args [0] == "frames-in-native")
			frames_in_native ();
		if (args.Length > 0 && args [0] == "invoke-single-threaded")
			new Tests ().invoke_single_threaded ();
		if (args.Length > 0 && args [0] == "invoke-abort")
			new Tests ().invoke_abort ();
		new Tests ().evaluate_method ();
		return 3;
	}

	public static void local_reflect () {
		//Breakpoint line below, and reflect someField via ObjectMirror;
		LocalReflectClass.RunMe ();
	}

	public static void breakpoints () {
		/* Call these early so it is JITted by the time a breakpoint is placed on it */
		bp3 ();
		bp7<int> ();
		bp7<string> ();

		bp1 ();
		bp2 ();
		bp3 ();
		bp4 ();
		bp4 ();
		bp4 ();
		bp5 ();
		bp6<string> (new GClass <int> ());
		bp7<int> ();
		bp7<string> ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp4 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp5 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp6<T> (GClass<int> gc) {
		gc.bp<int> ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void bp7<T> () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void single_stepping () {
		bool b = true;
		ss1 ();
		ss2 ();
		ss3 ();
		ss3_2 ();
		ss4 ();
		ss5 (new int [] { 1, 2, 3 }, new Func<int, bool> (is_even));
		try {
			ss6 (b);
		} catch {
		}
		ss7 ();
		ss_nested ();
		ss_regress_654694 ();
		ss_step_through ();
		ss_non_user_code ();
		ss_recursive (1);
		ss_recursive2 (1);
		ss_recursive2 (1);
		ss_recursive_chaotic ();
		ss_fp_clobber ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int ss3 () {
		int sum = 0;

		for (int i = 0; i < 10; ++i)
			sum += i;

		return sum;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss3_2 () {
		ss3_2_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss3_2_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int ss4 () {
		ss1 (); ss1 ();
		ss2 ();
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss5 (int[] arr, Func<int, bool> selector) {
		// Call into linq which calls back into this assembly
		arr.Count (selector);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss6 (bool b) {
		if (b) {
			ss6_2 ();
			throw new Exception ();
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss6_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss7 () {
		try {
			ss7_2 ();
			ss7_3 ();
		} catch {
		}
		ss7_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss7_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss7_3 () {
		throw new Exception ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_nested () {
		ss_nested_1 (ss_nested_2 ());
		ss_nested_1 (ss_nested_2 ());
		ss_nested_3 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_nested_1 (int i) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int ss_nested_2 () {
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_nested_3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_step_through () {
		step_through_1 ();
		StepThroughClass.step_through_2 ();
		step_through_3 ();
	}

	[DebuggerStepThrough]
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void step_through_1 () {
	}

	[DebuggerStepThrough]
	class StepThroughClass {
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static void step_through_2 () {
		}
	}

	[DebuggerStepThrough]
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void step_through_3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_non_user_code () {
		non_user_code_1 ();
		StepNonUserCodeClass.non_user_code_2 ();
		non_user_code_3 ();
	}

	[DebuggerNonUserCode]
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void non_user_code_1 () {
	}

	[DebuggerNonUserCode]
	class StepNonUserCodeClass {
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static void non_user_code_2 () {
		}
	}

	[DebuggerNonUserCode]
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void non_user_code_3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive (int n) {
		if (n == 10)
			return;
		ss_recursive (n + 1);
	}

	// Breakpoint will be placed here
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive2_trap ()
	{
	}

	public static void ss_recursive2_at (string s)
	{
		// Console.WriteLine (s);
	}

	// This method is used both for a step over and step out test.
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive2 (int x)
	{
		ss_recursive2_at ( "ss_recursive2 in " + x);
		if (x < 5) {
			int next = x + 1;
			ss_recursive2_at ("ss_recursive2 descend " + x);
			ss_recursive2_trap ();
			ss_recursive2 (next);
		}
		ss_recursive2_at ("ss_recursive2 out " + x);
	}

	// Breakpoint will be placed here
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic_trap ()
	{
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic_at (bool exiting, string at, int n)
	{
//		string indent = "";
//		for (int count = 5 - n; count > 0; count--)
//			indent += "\t";
//		Console.WriteLine (indent + (exiting ? "<--" : "-->") + " " + at + " " + n);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic_fizz (int n)
	{
		ss_recursive_chaotic_at (false, "fizz", n);
		if (n > 0) {
			int next = n - 1;
			ss_recursive_chaotic_buzz (next);
			ss_recursive_chaotic_fizzbuzz (next);
		} else {
			ss_recursive_chaotic_trap ();
		}
		ss_recursive_chaotic_at (true, "fizz", n);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic_buzz (int n)
	{
		ss_recursive_chaotic_at (false, "buzz", n);
		if (n > 0) {
			int next = n - 1;
			ss_recursive_chaotic_fizz (next);
			ss_recursive_chaotic_fizzbuzz (next);
		}
		ss_recursive_chaotic_at (true, "buzz", n);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic_fizzbuzz (int n)
	{
		ss_recursive_chaotic_at (false, "fizzbuzz", n);
		if (n > 0) {
			int next = n - 1;
			ss_recursive_chaotic_fizz (next);
			ss_recursive_chaotic_buzz (next);
			ss_recursive_chaotic_fizzbuzz (next);
		}
		ss_recursive_chaotic_at (true, "fizzbuzz", n);
	}

	// Call a complex tree of recursive calls that has tripped up "step out" in the past.
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_recursive_chaotic ()
	{
		ss_recursive_chaotic_fizz (5);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_fp_clobber () {
		double v = ss_fp_clobber_1 (5.0);
		ss_fp_clobber_2 (v);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static double ss_fp_clobber_1 (double d) {
		return d + 2.0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_fp_clobber_2 (double d) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static bool is_even (int i) {
		return i % 2 == 0;
	}

	/*
		lock (static_s) {
			Console.WriteLine ("HIT!");
		}
		return 0;
	}
	*/

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void arguments () {
		arg1 (SByte.MaxValue - 5, Byte.MaxValue - 5, true, Int16.MaxValue - 5, UInt16.MaxValue - 5, 'F', Int32.MaxValue - 5, UInt32.MaxValue - 5, Int64.MaxValue - 5, UInt64.MaxValue - 5, 1.2345f, 6.78910, new IntPtr (Int32.MaxValue - 5), new UIntPtr (UInt32.MaxValue - 5));
		int i = 42;
		arg2 ("FOO", null, "BLA", ref i, new GClass <int> { field = 42 }, new object (), '\0'.ToString () + "A");
		Tests t = new Tests () { field_i = 42, field_s = "S" };
		t.arg3 ("BLA");
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static int arg1 (sbyte sb, byte b, bool bl, short s, ushort us, char c, int i, uint ui, long l, ulong ul, float f, double d, IntPtr ip, UIntPtr uip) {
		return (int)(sb + b + (bl ? 0 : 1) + s + us + (int)c + i + ui + l + (long)ul + f + d + (int)ip + (int)uip);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static string arg2 (string s, string s3, object o, ref int i, GClass <int> gc, object o2, string s4) {
		return s + (s3 != null ? "" : "") + o + i + gc.field + o2;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public object arg3 (string s) {
		return s + s + s + s + this;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void objects () {
		Tests t = new Tests () { field_i = 42, field_bool1 = true, field_bool2 = false, field_char = 'A', field_byte = 129, field_sbyte = -33, field_short = Int16.MaxValue - 5, field_ushort = UInt16.MaxValue - 5, field_long = Int64.MaxValue - 5, field_ulong = UInt64.MaxValue - 5, field_float = 3.14f, field_double = 3.14f, field_s = "S", base_field_i = 43, base_field_s = "T", field_enum = AnEnum.B, field_class = null, field_intptr = new IntPtr (Int32.MaxValue - 5), field_nullable = null };
		t.o1 (new Tests2 () { field_j = 43 }, new GClass <int> { field = 42 }, new GClass <string> { field = "FOO" });
		o2 (new string [] { "BAR", "BAZ" }, new int[] { 42, 43 }, new int [,] { { 1, 2 }, { 3, 4 }}, (int[,])Array.CreateInstance (typeof (int), new int [] { 2, 2}, new int [] { 1, 3}), new int[] { 0 });
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public object o1 (Tests2 t, GClass <int> gc1, GClass <string> gc2) {
		if (t == null || gc1 == null || gc2 == null)
			return null;
		else
			return this;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static string o2 (string[] s2, int[] s3, int[,] s4, int[,] s5, IList<int> s6) {
		return s2 [0] + s3 [0] + s4 [0, 0] + s6 [0];
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void objrefs () {
		Tests t = new Tests () {};
		set_child (t);
		t.objrefs1 ();
		t.child = null;
		GC.Collect ();
		objrefs2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void set_child (Tests t) {
		t.child = new Tests ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void objrefs1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void objrefs2 () {
	}

	public static void vtypes () {
		Tests t = new Tests () { field_struct = new AStruct () { i = 42, s = "S", k = 43 }, generic_field_struct = new GStruct<int> () { i = 42 }, field_boxed_struct = new AStruct () { i = 42 }, boxed_struct_field = new KeyValuePair<int, object> (1, (long)42 ) };
		AStruct s = new AStruct { i = 44, s = "T", k = 45 };
		AStruct[] arr = new AStruct[] { 
			new AStruct () { i = 1, s = "S1" },
			new AStruct () { i = 2, s = "S2" } };
		TypedReference typedref = __makeref (s);
		t.vtypes1 (s, arr, typedref);
		vtypes2 (s);
		vtypes3 (s);
		vtypes4 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public object vtypes1 (AStruct s, AStruct[] arr, TypedReference typedref) {
		if (arr != null)
			return this;
		else
			return null;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void vtypes2 (AStruct s) {
		s.foo (5);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void vtypes3 (AStruct s) {
		AStruct.static_foo (5);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void vtypes4_2 (IRecStruct o) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void vtypes4 () {
		IRecStruct s = new RecStruct ();
		s.foo (s);
		vtypes4_2 (s);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals () {
		string s = null;
		var astruct = new AStruct () { i = 42 };
		locals1 (null);
		locals2<string> (null, 5, "ABC", ref s, ref astruct);
		locals3 ();
		locals6 ();
		locals7<int> (22);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void locals11 (double a, ref double b) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals1 (string[] args) {
		long foo = 42;

		double ri = 1;
		locals11 (b: ref ri, a: ri);

		for (int j = 0; j < 10; ++j) {
			foo ++;
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	[StateMachine (typeof (int))]
	public static void locals2<T> (string[] args, int arg, T t, ref string rs, ref AStruct astruct) {
		long i = 42;
		string s = "AB";

		for (int j = 0; j < 10; ++j) {
			if (s != null)
				i ++;
			if (t != null)
				i ++;
			astruct = new AStruct ();
		}
		rs = "A";
		List<int> alist = new List<int> () { 12 };
	}


	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals3 () {
		string s = "B";
		s.ToString ();

		{
			long i = 42;
			i ++;
			locals4 ();
		}
		{
			string i = "A";
			i.ToString ();
			locals5 ();
		}
		{
			long j = 42;
			j ++;
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals4 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals5 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6 () {
		int i = 0;
		int j = 0;
		for (i = 0; i < 10; ++i)
			j ++;
		sbyte sb = 0;
		for (i = 0; i < 10; ++i)
			sb ++;
		locals6_1 ();
		locals6_2 (j);
		locals6_3 ();
		locals6_4 (j);
		locals6_5 ();
		locals6_6 (sb);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_2 (int arg) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_3 () {
		// Clobber all registers
		int sum = 0, i, j, k, l, m;
		for (i = 0; i < 100; ++i)
			sum ++;
		for (j = 0; j < 100; ++j)
			sum ++;
		for (k = 0; k < 100; ++k)
			sum ++;
		for (l = 0; l < 100; ++l)
			sum ++;
		for (m = 0; m < 100; ++m)
			sum ++;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_4 (int arg) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_5 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals6_6 (int arg) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void locals7<T> (T arg) {
		T t = arg;
		T t2 = t;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void line_numbers () {
		LineNumbers.ln1 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void suspend () {
		long i = 5;

		while (true) {
			i ++;
		}
	}

	struct TypedRefTest {
		public int MaxValue;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void type_info () {
		Tests t = new Tests () { field_i = 42, field_s = "S", base_field_i = 43, base_field_s = "T", field_enum = AnEnum.B };
		t.ti1 (new Tests2 () { field_j = 43 }, new GClass <int> { field = 42 }, new GClass <string> { field = "FOO" });
		int val = 0;
		unsafe {
			AStruct s = new AStruct () { i = 42, s = "S", k = 43 };
			TypedRefTest reftest = new TypedRefTest () { MaxValue = 12 };
			TypedReference typedref = __makeref (reftest);
			ti2 (new string [] { "BAR", "BAZ" }, new int[] { 42, 43 }, new int [,] { { 1, 2 }, { 3, 4 }}, ref val, (int*)IntPtr.Zero, 5, s, new Tests (), new Tests2 (), new GClass <int> (), AnEnum.B, typedref);
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public object ti1 (Tests2 t, GClass <int> gc1, GClass <string> gc2) {
		if (t == null || gc1 == null || gc2 == null)
			return null;
		else
			return this;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static unsafe string ti2 (string[] s2, int[] s3, int[,] s4, ref int ri, int* ptr, int i, AStruct s, Tests t, Tests2 t2, GClass<int> g, AnEnum ae, TypedReference typedref) {
		return s2 [0] + s3 [0] + s4 [0, 0];
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void assembly_load () {
		assembly_load_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void assembly_load_2 () {
		// This will load System.dll while holding the loader lock
		new Foo ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void invoke () {
		new Tests ().invoke1 (new Tests2 (), new AStruct () { i = 42, j = (IntPtr)43 }, new GStruct<int> { j = 42 });
		new Tests ().invoke_ex ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke1 (Tests2 t, AStruct s, GStruct<int> g) {
		invoke2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_ex () {
		invoke_ex_inner ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_ex_inner () {
		try {
			throw new Exception ();
		} catch {
		}
	}

	int counter;

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_single_threaded () {
		// Spawn a thread incrementing a counter
		bool finished = false;

		new Thread (delegate () {
				while (!finished)
					counter ++;
		}).Start ();

		Thread.Sleep (100);

		invoke_single_threaded_2 ();

		finished = true;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_single_threaded_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_abort () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void invoke_abort_2 () {
		Thread.Sleep (1000000);
	}

	public void invoke_return_void () {
	}

	public string invoke_return_ref () {
		return "ABC";
	}

	public object invoke_return_null () {
		return null;
	}

	public int invoke_return_primitive () {
		return 42;
	}

	public int? invoke_return_nullable () {
		return 42;
	}

	public int? invoke_return_nullable_null () {
		return null;
	}

	public void invoke_type_load () {
		new Class3 ();
	}

	class Class3 {
	}

	public long invoke_pass_primitive (byte ub, sbyte sb, short ss, ushort us, int i, uint ui, long l, ulong ul, char c, bool b, float f, double d) {
		return ub + sb + ss + us + i + ui + l + (long)ul + (int)c + (b ? 1 : 0) + (int)f + (int)d;
	}

	public int invoke_pass_primitive2 (bool b) {
		return b ? 1 : 0;
	}

	public string invoke_pass_ref (string s) {
		return s;
	}

	public static string invoke_static_pass_ref (string s) {
		return s;
	}

	public static void invoke_static_return_void () {
	}

	public static void invoke_throws () {
		throw new Exception ();
	}

	public int invoke_iface () {
		return 42;
	}

	public void invoke_out (out int foo, out int[] arr) {
		foo = 5;
		arr = new int [10];
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void exceptions () {
		try {
			throw new OverflowException ();
		} catch (Exception) {
		}
		try {
			throw new OverflowException ();
		} catch (Exception) {
		}
		try {
			throw new ArgumentException ();
		} catch (Exception) {
		}
		try {
			throw new OverflowException ();
		} catch (Exception) {
		}
		// no subclasses
		try {
			throw new OverflowException ();
		} catch (Exception) {
		}
		try {
			throw new Exception ();
		} catch (Exception) {
		}

		object o = null;
		try {
			o.GetType ();
		} catch (Exception) {
		}

		try {
			exceptions2 ();
		} catch (Exception) {
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void unhandled_exception () {
		ThreadPool.QueueUserWorkItem (delegate {
				throw new InvalidOperationException ();
			});
		Thread.Sleep (10000);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void unhandled_exception_endinvoke_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void unhandled_exception_endinvoke () {
			Action action = new Action (() => 
			{
				throw new Exception ("thrown");
			});
			action.BeginInvoke ((ar) => {
				try {
					action.EndInvoke (ar);
				} catch (Exception ex) {
					//Console.WriteLine (ex);
				}
			}, null);
		Thread.Sleep (1000);
		unhandled_exception_endinvoke_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void unhandled_exception_user () {
		System.Threading.Tasks.Task.Factory.StartNew (() => {
				Throw ();
			});
		Thread.Sleep (10000);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void Throw () {
		throw new Exception ();
	}

	internal static Delegate create_filter_delegate (Delegate dlg, MethodInfo filter_method)
	{
		if (dlg == null)
			throw new ArgumentNullException ();
		if (dlg.Target != null)
			throw new ArgumentException ();
		if (dlg.Method == null)
			throw new ArgumentException ();

		var ret_type = dlg.Method.ReturnType;
		var param_types = dlg.Method.GetParameters ().Select (x => x.ParameterType).ToArray ();

		var dynamic = new DynamicMethod (Guid.NewGuid ().ToString (), ret_type, param_types, typeof (object), true);
		var ig = dynamic.GetILGenerator ();

		LocalBuilder retval = null;
		if (ret_type != typeof (void))
			retval = ig.DeclareLocal (ret_type);

		var label = ig.BeginExceptionBlock ();

		for (int i = 0; i < param_types.Length; i++)
			ig.Emit (OpCodes.Ldarg, i);
		ig.Emit (OpCodes.Call, dlg.Method);

		if (retval != null)
			ig.Emit (OpCodes.Stloc, retval);

		ig.Emit (OpCodes.Leave, label);

		ig.BeginExceptFilterBlock ();

		ig.Emit (OpCodes.Call, filter_method);

		ig.BeginCatchBlock (null);

		ig.Emit (OpCodes.Pop);

		ig.EndExceptionBlock ();

		if (retval != null)
			ig.Emit (OpCodes.Ldloc, retval);

		ig.Emit (OpCodes.Ret);

		return dynamic.CreateDelegate (dlg.GetType ());
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void exception_filter_method () {
		throw new InvalidOperationException ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static int exception_filter_filter (Exception exc) {
		return 1;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void exception_filter () {
		var method = typeof (Tests).GetMethod (
			"exception_filter_method", BindingFlags.NonPublic | BindingFlags.Static);
		var filter_method = typeof (Tests).GetMethod (
			"exception_filter_filter", BindingFlags.NonPublic | BindingFlags.Static);

		var dlg = Delegate.CreateDelegate (typeof (Action), method);

		var wrapper = (Action) create_filter_delegate (dlg, filter_method);

		wrapper ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static bool return_true () {
		return true;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void exceptions2 () {
		if (return_true ())
			throw new Exception ();
		Console.WriteLine ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void threads () {
		Thread t = new Thread (delegate () {});

		t.Start ();
		t.Join ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void domains () {
		AppDomain domain = AppDomain.CreateDomain ("domain");

		CrossDomain o = (CrossDomain)domain.CreateInstanceAndUnwrap (
				   typeof (CrossDomain).Assembly.FullName, "CrossDomain");

		domains_print_across (o);

		domains_2 (o, new CrossDomain ());

		o.invoke_2 ();

		o.invoke ();

		o.invoke_2 ();

		AppDomain.Unload (domain);

		domains_3 ();

		typeof (Tests).GetMethod ("called_from_invoke").Invoke (null, null);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void called_from_invoke () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void domains_2 (object o, object o2) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void domains_print_across (object o) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void domains_3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void invoke_in_domain () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void invoke_in_domain_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void dynamic_methods () {
		var m = new DynamicMethod ("dyn_method", typeof (void), new Type []  { typeof (int) }, typeof (object).Module);
		var ig = m.GetILGenerator ();

		ig.Emit (OpCodes.Ldstr, "FOO");
		ig.Emit (OpCodes.Call, typeof (Tests).GetMethod ("dyn_call"));
		ig.Emit (OpCodes.Ret);

		var del = (Action<int>)m.CreateDelegate (typeof (Action<int>));

		del (0);
	}

	public static void dyn_call (string s) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ref_emit () {
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = "foo";

		AssemblyBuilder assembly =
			Thread.GetDomain ().DefineDynamicAssembly (
													   assemblyName, AssemblyBuilderAccess.RunAndSave);

		ModuleBuilder module = assembly.DefineDynamicModule ("foo.dll");

		TypeBuilder tb = module.DefineType ("foo", TypeAttributes.Public, typeof (object));
		MethodBuilder mb = tb.DefineMethod ("ref_emit_method", MethodAttributes.Public|MethodAttributes.Static, CallingConventions.Standard, typeof (void), new Type [] { });
		ILGenerator ig = mb.GetILGenerator ();
		ig.Emit (OpCodes.Ldstr, "FOO");
		ig.Emit (OpCodes.Call, typeof (Tests).GetMethod ("ref_emit_call"));
		ig.Emit (OpCodes.Ret);

		Type t = tb.CreateType ();

		t.GetMethod ("ref_emit_method").Invoke (null, null);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ref_emit_call (string s) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void frames_in_native () {
		Thread.Sleep (500);
		var evt = new ManualResetEvent (false);
		
		object mon = new object ();
		ThreadPool.QueueUserWorkItem (delegate {
				frames_in_native_2 ();
				evt.Set ();
			});
		evt.WaitOne ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void frames_in_native_2 () {
		frames_in_native_3 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void frames_in_native_3 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void string_call (string s) {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ss_regress_654694 () {
		if (true) {
			string h = "hi";
			string_call (h);
		}
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void user () {
		Debugger.Break ();

		Debugger.Log (5, Debugger.IsLogging () ? "A" : "", "B");
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void type_load () {
		type_load_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void type_load_2 () {
		var c1 = new Dictionary<int, int> ();
		c1.ToString ();
		var c = new TypeLoadClass ();
		c.ToString ();
		var c2 = new TypeLoadClass2 ();
		c2.ToString ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void regress () {
		regress_2755 (DateTime.Now);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static unsafe void regress_2755 (DateTime d) {
		int* buffer = stackalloc int [128];

		regress_2755_2 ();

		int sum = 0;
		for (int i = 0; i < 128; ++i)
			sum += buffer [i];

		regress_2755_3 (sum);
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void regress_2755_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void regress_2755_3 (int sum) {
	}

	static object gc_suspend_field;

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static unsafe void set_gc_suspend_field () {
		set_gc_suspend_field_2 ();
		// Clear stack
		int* buffer = stackalloc int [4096];
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void set_gc_suspend_field_2 () {
		gc_suspend_field = new object ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void gc_suspend_1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void gc_suspend_invoke () {
		gc_suspend_field = null;
		GC.Collect ();
		GC.WaitForPendingFinalizers ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void gc_suspend () {
		set_gc_suspend_field ();
		gc_suspend_1 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void generic_method<T> () where T : class {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void evaluate_method_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public void evaluate_method () {
		field_i = 42;
		evaluate_method_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void set_ip_1 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	static void set_ip_2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void set_ip () {
		int i = 0, j;

		i ++;
		i ++;
		set_ip_1 ();
		i ++;
		j = 5;
		set_ip_2 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void step_filters () {
		ClassWithCctor.cctor_filter ();
	}

	class ClassWithCctor {
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static ClassWithCctor () {
			int i = 1;
			int j = 2;
		}

		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static void cctor_filter () {
		}
	}

	public override string virtual_method () {
		return "V2";
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void threadpool_bp () { }

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void threadpool_io () {
		// Start a threadpool task that blocks on I/O.
		// Regression test for #42625
		const int nbytes = 16;
		var bsOut = new byte[nbytes];
		for (int i = 0; i < nbytes; i++) {
			bsOut[i] = (byte)i;
		}
		var endPoint = NetworkHelpers.LocalEphemeralEndPoint ();
		var l = new TcpListener (endPoint);
		l.Start ();
		Task<byte[]> t = Task.Run (async () => {
			var c = new TcpClient ();
			await c.ConnectAsync (endPoint.Address, endPoint.Port);
			var streamIn = c.GetStream ();
			var bs = new byte[nbytes];
			int nread = 0;
			int nremain = nbytes;
			while (nread < nbytes) {
				int r = await streamIn.ReadAsync (bs, nread, nremain);
				nread += r;
				nremain -= r;
			}
			streamIn.Close ();
			return bs;
			});
		var s = l.AcceptTcpClient ();
		l.Stop ();
		// write bytes in two groups so that the task blocks on the ReadAsync
		var streamOut = s.GetStream ();
		var nbytesFirst = nbytes / 2;
		var nbytesRest = nbytes - nbytesFirst;
		streamOut.Write (bsOut, 0, nbytesFirst);
		threadpool_bp ();
		streamOut.Write (bsOut, nbytesFirst, nbytesRest);
		streamOut.Close ();
		var bsIn = t.Result;
	}
}

class TypeLoadClass {
}

class TypeLoadClass2 {
}

public class SentinelClass : MarshalByRefObject {
}

public class CrossDomain : MarshalByRefObject
{
	SentinelClass printMe = new SentinelClass ();

	public void invoke () {
		Tests.invoke_in_domain ();
	}

	public void invoke_2 () {
		Tests.invoke_in_domain_2 ();
	}

	public int invoke_3 () {
		return 42;
	}
}	

public class Foo
{
	public ProcessStartInfo info;
}

// Class used for line number info testing, don't change its layout
public class LineNumbers
{
	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ln1 () {
		// Column 3
		ln2 ();
		ln3 ();
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ln2 () {
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static void ln3 () {
#pragma warning disable 0219
		int i = 5;
#pragma warning restore 0219
		#line 55 "FOO"
	}
}

class LocalReflectClass
{
	public static void RunMe ()
	{
		var reflectMe = new someClass ();
		reflectMe.someMethod ();
	}

	class someClass : ContextBoundObject
	{
		public object someField;

		public void someMethod ()
		{
		}
	}
}


