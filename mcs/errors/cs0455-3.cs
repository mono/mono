// CS0455: Type parameter `T' inherits conflicting constraints `System.ValueType' and `Test'
// Line: 8

class Test
{ }

class Foo<T,U>
	where T : struct, U
	where U : Test
{ }
