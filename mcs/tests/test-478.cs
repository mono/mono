using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Test {
	class MainClass {
		public static void Main() {}
	}

	[Test(TestEnum2.TestValue)]
	enum TestEnum {
		Value
	}

	class TestAttribute : Attribute {
		public TestAttribute( TestEnum2 value ) {
		}
	}
	
	class Test2Attribute : Attribute {
	}
	
	enum TestEnum2 {
		[Test2]
		TestValue
	}
}
