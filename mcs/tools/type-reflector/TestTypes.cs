//
// TestTypes.cs: Sample types for testing type-reflector
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;

namespace Testing
{
	public interface IFoo {}
	public interface IBar {}
	public interface IBaz {}

	public delegate void FooEventHandler ();

	public class MyAttribute : Attribute {
		private string _f;
		public MyAttribute (string f) {
			_f = f;
		}
		public string F {
			get {return _f;}
		}
	}

	public class AnotherAttribute : Attribute {
		private int n;
		private string s;

		public AnotherAttribute (int _n) {
			n = _n;
		}

		public AnotherAttribute (string _s) {
			s = _s;
		}

		public int N {
			get {return n;}
		}

		public string S {
			get {return s;}
		}
	}

	public class TypeAttributes : Attribute {
		public TypeAttributes () {}

		public char Char = 'a';
		public decimal Decimal = 42m;
		public double Double = 17.0;
		public int Int = 24;
		public long Long = 57L;
		public string String = "hello, world!";
		public float Float = 34.0f;
		public uint UInt = 10;
		public ulong ULong = 20;
	}

	// [CLSCompliant(false)]
	[MyAttribute ("Hello, world!")]
	[Serializable]
	public class TestClass : IFoo, IBar, IBaz {

		[TypeAttributes]
		public sealed class NestedClass {
			public static int Foo = 10;
			public const int Bar = 20;
			public int Baz {
				get {return 30;}
			}
		}

		private int PrivateField;
		protected float ProtectedField;

		[MyAttribute("foo!")]
		public double PublicField;

		internal long InternalField;

		public TestClass (short s) {PublicField = 3.14;}
		protected TestClass (long l) {ProtectedField = 2.71F;}
		private TestClass (int i) {PrivateField = 13;}
		internal TestClass (float f) {InternalField = 64;}

		// indexer
		public int this [int index] {
			get {return 42;}
			set {/* ignore */}
		}

		[MyAttribute ("Public Property")]
		public int PublicGetSet {
			get {return 0;}
			set {PublicField = value;}
		}

		public static int StaticGetter {
			get {return 42;}
		}

		public static int StaticSetter {
			set {/* ignore */}
		}

		protected short ProtectedGetter {
			get {return -1;}
		}

		private char PrivateSetter {
			set {PrivateField = value;}
		}

		internal float InternalProperty {
			get {return ProtectedField;}
			set {ProtectedField = value;}
		}

		public event FooEventHandler PubFoo;
		protected event FooEventHandler ProFoo;
		private event FooEventHandler PrivFoo;
		internal event FooEventHandler IntFoo;

		public static int msFoo = 42;
		public const int msBar = 64;

		[MyAttribute ("Some Name")]
		[return: AnotherAttribute ("Return Attribute")]
		public short PublicMethod ([MyAttribute("The parameter")] [AnotherAttribute(42)] short s) 
		{
			PubFoo (); return s;
		}

		public static TestEnum PublicMethod2 ()
		{
			return TestEnum.Foo;
		}

		private int PrivateMethod (int i) {PrivFoo (); return i;}
		protected long ProtectedMethod (long l) {ProFoo (); return l;}
		internal float InternalMethod (float f) {IntFoo (); return f;}
	}

	[Flags]
	public enum TestEnum {
		Foo, 
		Bar, 
		Baz, 
		Qux, 
		Quux
	}
}

