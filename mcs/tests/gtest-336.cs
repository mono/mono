using System;

public class TestAttribute : Attribute
{
	object type;
	public object Type
	{
		get { return type; }
		set { type = value; }
	}
	public TestAttribute() { }
	public TestAttribute(Type type)
	{
		this.type = type;
	}
}

namespace N
{
	class C<T>
	{
		[Test(Type = typeof(C<>))] //this shouldn't fail
		public void Bar() { }

		[Test(typeof(C<>))]     // this shouldn't fail
		public void Bar2() { }
		
		[Test(typeof(C<int>))]     // this shouldn't fail
		public void Bar3() { }

		[Test(typeof(C<CC>))]     // this shouldn't fail
		public void Bar4() { }
	}

	class CC
	{
		public static void Main()
		{
		}
	}
}