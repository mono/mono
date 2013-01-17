//
// Test for access modifiers
//

using System;

 public class Tester {

	 public static void Main ()
	 {
		 A a = new A (8);
		 B b = new B (9);

		 b.SetCount (10);
		 Console.WriteLine ("b.Count should be 9: {0}", b.Count);
		 Console.WriteLine ("b [{0}] should return {0}: {1}", 10, b [10]);

		 Console.WriteLine ("a.Message : {0}", a.Message);
		 b.Message = "";
		 Console.WriteLine ("b.Messasge : {0}", b.Message);
	 }

 }

 public class A {

	 protected int count;

	 public A (int count)
	 {
		 this.count = count;
	 }

	 public virtual int Count {
		 get {
			 return count;
		 }
		 protected set {
			 count = value;
		 }
	 }

	 public virtual int this [int index] {
		 get {
			 return index;
		 }
	 }

	 public virtual string Message {
		 get {
			 return "Hello Mono";
		 }
	 }

 }

 public class B : A {

	 public B (int count) : base (count)
	 {
	 }

	 public override int Count {
		 protected set {
		 }
	 }

	 public void SetCount (int value)
	 {
		 Count = value;
	 }

	 public override int this [int index] {
		 get {
			 return base [index];
		 }
	 }

	 public new string Message {
		 get {
			 return "Hello Mono (2)";
		 }
		 internal set {
		 }
	 }

 }

