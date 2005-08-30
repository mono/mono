using System;

enum Foo { foo };

public class Test
{
        static void Main ()
        {
                ValueType vt = (ValueType) 1;
		IComparable ic = (IComparable) 1;

		Enum e = (Enum) Foo.foo;
        }
}
