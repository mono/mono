/*
Test tail.callvirt.

Author:
    Jay Krell (jaykrell@microsoft.com)

Copyright 2018 Microsoft
Licensed under the MIT license. See LICENSE file in the project root for full license information.

This IL will exit with 0 for success, 1 for failure.
It is based on nearby C# that is based on nearby F#.

This test covers tail.callvirt.
Incidentally it is a mutual recursion, not a self-recursion,
no generics, no pointers, small function signature, no value types, etc.

To properly test this:
 - compile with csc -optimize so ret is next to call
 - change call to tail. call
 - Or use the IL.

Actually only test the IL.
*/

using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

unsafe class A
{
    static int check (long stack1, long stack2)
    {
        return (stack1 == stack2) ? 0 : 1;
    }

    [MethodImpl (NoInlining)]
    public virtual int f1(int counter, long initial_stack, long current_stack)
    {
        int local;
        if (counter > 0)
            return f2(counter - 1, initial_stack, (long)&local);
        return check((long)&local, current_stack);
    }

    [MethodImpl (NoInlining)]
    public virtual int f2(int counter, long initial_stack, long current_stack)
    {
        int local;
        if (counter > 0)
            return f1(counter - 1, initial_stack, (long)&local);
        return check((long)&local, current_stack);
    }

    [MethodImpl (NoInlining)]
    public static void Main()
    {
        int stack;
        Environment.Exit(new A().f1(100, (long)&stack, 0));
    }
}
