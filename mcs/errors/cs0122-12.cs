// cs0122.cs: 'Base.Base(string)' is inaccessible due to its protection level
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
