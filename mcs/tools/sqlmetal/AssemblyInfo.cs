//
// AssemblyInfo.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (C) 2009 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using DbLinq.Factory;

[assembly: AssemblyTitle ("SqlMetal")]
[assembly: AssemblyDescription ("System.Data.Linq code generator")]
[assembly: AssemblyDefaultAlias ("SqlMetal.exe")]

[assembly: CLSCompliant (true)]

// Unit tests needs access to the DbLinq internals
[assembly: InternalsVisibleTo("sqlmetal_test_net_2_0, PublicKey=" +
"0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400" +
"83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8" +
"fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663" +
"962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279" +
"2309259f")]

[assembly: DbLinq]

