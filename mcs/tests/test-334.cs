namespace Test {
	using Bar = Foo.Store.Directory;
	namespace Foo {
		namespace Index {
			public class CompoundFileReader : Bar {
				public static void Main () { }
			}
		}
	}
}

namespace Test {
	namespace Foo {
		namespace Store {
			public class Directory { }
		}
	}
}

