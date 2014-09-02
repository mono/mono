// CS0136: A local variable named `arg' cannot be declared in this scope because it would give a different meaning to `arg', which is already used in a `parent or current' scope to denote something else
// Line: 11

using System;

partial class PC
{
    Func<int, int> f = (arg) => 1;
}

partial class PC (int arg) 
{
}