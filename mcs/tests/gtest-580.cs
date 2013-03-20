// Parser tests for contextual where 

namespace WhereProblems
{
	class MyClass<where> { }
	interface MyInterface<where> { }
	struct MyStruct<where> { }

	class Classes
	{
		class where { }
		class DER17 : where { }

		public static void Main ()
		{
		}
	}
}