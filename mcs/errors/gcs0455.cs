// CS0455: Type parameter `T' inherits conflicting constraints `Test' and `World'
// Line: 11

class Test
{ }

class World
{ }

class Foo<T,U>
	where T : Test, U
	where U : World
{ }
