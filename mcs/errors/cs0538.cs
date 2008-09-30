// CS0538: The type `BaseClass' in explicit interface declaration is not an interface
// Line: 11

class BaseClass
{
	public void Foo() {}
}

class InstanceClass: BaseClass
{
	void BaseClass.Foo()
	{
	}
}



