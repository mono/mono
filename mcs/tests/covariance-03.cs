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

	 delegate void MethodHandler (C c1, C c2, C c3);

	 static void MethodSample (B b, A a, C c)
	 {
		 Console.WriteLine ("b = {0}", b.Name);
		 Console.WriteLine ("a = {0}", a.Name);
		 Console.WriteLine ("c = {0}, {1}", c.Name, c.Value);
	 }

	 static void Main ()
	 {
		 MethodHandler mh = MethodSample;

		 C a = new C ("Hello", "hello");
		 C b = new C ("World", "world");
		 C c = new C ("!", "!!!");

		 mh (b, a, c);
	 }
	
 }

