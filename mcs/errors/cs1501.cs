// cs1501.cs: No overload for method `Base' takes `0' arguments
// Line: 12
class Base {
	Base (string x)
	{
	}
}

// Notice how there is no invocation to "base (something)"

class Derived : Base {
	Derived ()
	{
	}
}
