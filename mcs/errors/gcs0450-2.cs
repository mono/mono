// CS0450: `A<bool,int>': cannot specify both a constraint class and the `class' or `struct' constraint
// Line: 8

class A<T, U>
{
}

delegate void Test<T>() where T : struct, A<bool, int>;
