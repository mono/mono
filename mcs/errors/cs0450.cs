// CS0450: `A': cannot specify both a constraint class and the `class' or `struct' constraint
// Line: 8

class A
{
}

class B<T> where T : class, A
{
}
