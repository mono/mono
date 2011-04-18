// CS0533: `DerivedClass.Foo()' hides inherited abstract member `BaseClass.Foo()'
// Line: 9

abstract public class BaseClass {
        abstract protected void Foo ();
}

abstract class DerivedClass: BaseClass {
        public new void Foo () {}
		public static void Main () {}
}

