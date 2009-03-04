using System;

namespace N
{
	enum FieldType
	{
		Foo
	}
}

namespace N
{
	class Test
	{
		public FieldType FieldType = FieldType.Foo;

		public Test ()
		{
		}

		public Test (int i)
		{
		}

		public static void Main ()
		{
		}
	}
}