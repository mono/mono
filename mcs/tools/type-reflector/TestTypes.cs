//
// TestTypes.cs: Sample types for testing type-reflector
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

		public AnotherAttribute (int _n) {
			n = _n;
		}

		public int N {
			get {return n;}
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

		[MyAttribute ("Public Property")]
		public int PublicGetSet {
			get {return 0;}
			set {PublicField = value;}
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
		public const int msBar = 42;

		[MyAttribute ("Some Name")]
		[return: MyAttribute ("Return Attribute")]
		public short PublicMethod ([MyAttribute("The parameter")] [AnotherAttribute(42)] short s) 
		{
			PubFoo (); return s;
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

