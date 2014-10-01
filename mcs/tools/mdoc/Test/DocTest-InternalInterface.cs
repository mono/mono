namespace MyNamespace {
	internal interface MyInternalInterface {
		bool Foo {get;set;}
		string FooSet {set;}
		void FooMeth ();
		void BarMeth ();
	}

	public class MyClass : MyInternalInterface {
		public string Bar {get;set;}
		public void BarMeth () {} // part of the interface, but publicly implemented

		string MyInternalInterface.FooSet {set {}}
		bool MyInternalInterface.Foo {get;set;}
		void MyInternalInterface.FooMeth () {}
	}
}
