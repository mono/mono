// CS0112: `Foo.A.A' is inaccessible due to its protection level
// Line: 11
namespace Foo
{
	class A
	{
		private A (int a)
		{ }
	}

	class T : A
	{
	}
}
