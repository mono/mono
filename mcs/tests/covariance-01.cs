//
// Test for covariance support in delegates
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
	 public C (string name)
	 {
		 this.name = "C::" + name;
	 }
 }

 public class Tester {

	 delegate A MethodHandler (string name);

	 static A MethodSampleA (string name)
	 {
		 return new A (name);
	 }

	 static B MethodSampleB (string name)
	 {
		 return new B (name);
	 }

	 static C MethodSampleC (string name)
	 {
		 return new C (name);
	 }

	 static void Main ()
	 {
		 MethodHandler a = MethodSampleA;
		 MethodHandler b = MethodSampleB;
		 MethodHandler c = MethodSampleC;

		 A instance1 = a ("Hello");
		 A instance2 = b ("World");
		 A instance3 = c ("!");

		 Console.WriteLine (instance1.Name);
		 Console.WriteLine (instance2.Name);
		 Console.WriteLine (instance3.Name);
	 }
	
 }

