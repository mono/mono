namespace Test {
	using Directory = Foo.Store.Directory;
	namespace Foo {
		namespace Index {
			public class CompoundFileReader : Directory {
				static void Main () { }
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

