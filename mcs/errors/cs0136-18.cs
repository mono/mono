// CS0136: A local variable named `arg' cannot be declared in this scope because it would give a different meaning to `arg', which is already used in a `parent or current' scope to denote something else
// Line: 11
// Compiler options: -langversion:experimental

using System;

class A (Func<int, int> barg)
{
}

class B (int arg) 
	: A ((arg) => 1)
{
}