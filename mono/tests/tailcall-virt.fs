// Test tail.callvirt.
//
// Author:
//     Jay Krell (jaykrell@microsoft.com)
//
// Copyright 2018 Microsoft
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//
// This F# will stack overflow or exit with 1 or 2.
//
// This test covers tail.callvirt.
// Incidentally it is a mutual recursion, not a self-recursion,
// no generics, no pointers, small function signature, no value types, etc.
//
// Actually only test the nearby related IL, but this is what it is based on
// and this motivates that tail.callvirt is worth handling -- F# produces it.
//
// Be sure to compile with --optimize- else the tailcall might be optimized away.

type A() =
    member this.f2 (a) = if a > 0 then this.f1(a - 1)
                         else 1
    member this.f1 (a) = if a > 0 then this.f2(a - 1)
                         else 2

let a = A()
printfn "%d" (a.f1(1000 * 1000 * 10))
