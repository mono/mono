/*
Test tail.callvirt.

Author:
    Jay Krell (jaykrell@microsoft.com)

Copyright 2018 Microsoft
Licensed under the MIT license. See LICENSE file in the project root for full license information.

This test covers tail.callvirt.
Incidentally it is a mutual recursion, not a self-recursion,
no generics, no pointers, small function signature, no value types, etc.

This is based on F#, that stack overflows or exit with 1 or 2.
If testing the F#, be sure to compile with --optimize- else
the tailcall might be optimized away:
	type A() =
	    member this.f2 (a) = if a > 0 then this.f1(a - 1)
				 else 1
	    member this.f1 (a) = if a > 0 then this.f2(a - 1)
				 else 2

	let a = 	A()
	printfn "%d" (a.f1(1000 * 1000 * 10))

and more closely on C#, csc -optimize to keep
ret next to call.
*/
using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

class A
{
	[MethodImpl (NoInlining)]
	public virtual void f1(int counter)
	{
		if (counter > 0)
			f2(counter - 1);
	}

	[MethodImpl (NoInlining)]
	public virtual void f2(int counter)
	{
		if (counter > 0)
			f1(counter - 1);
	}

	[MethodImpl (NoInlining)]
	public static void Main()
	{
		new A().f1(999999);
	}
}
