//
// Test for contravariance support in delegates
//

using System;

 public class A {
	 protected string name;
	 
	 public A (string name)
	 {
		 this.name = "A::" + name;
	 }

	 public A ()
	 {
	 }

	 public string Name {
		 get {
			 return name;
		 }
	 }
 }

 public class B : A {
	 public B (string name)
	 {
		 this.name = "B::" + name;
	 }

	 public B ()
	 {
	 }
 }

 public class C : B {
	 string value;

	 public C (string name, string value)
	 {
		 this.name = "C::" + name;
		 this.value = value;
	 }

	 public string Value {
		 get {
			 return value;
		 }
	 }
 }

 public class Tester {

	 delegate string MethodHandler (C c);

	 static string MethodSampleA (A value)
	 {
		 return value.Name;
	 }

	 static string MethodSampleB (B value)
	 {
		 return value.Name;
	 }

	 static string MethodSampleC (C value)
	 {
		 return value.Name + " " + value.Value;
	 }

	 static void Main ()
	 {
		 MethodHandler da = MethodSampleA;
		 MethodHandler db = MethodSampleB;
		 MethodHandler dc = MethodSampleC;

		 C a = new C ("Hello", "hello");
		 C b = new C ("World", "world");
		 C c = new C ("!", "!!!");

		 Console.WriteLine (da (a));
		 Console.WriteLine (db (b));
		 Console.WriteLine (dc (c));
	 }
	
 }

