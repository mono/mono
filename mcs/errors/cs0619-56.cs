// CS0619: `A' is obsolete: `stop'
// Line: 11

using System;

[Obsolete ("stop", true)]
public class A
{
}

public class C<T> where T : A
{
}
