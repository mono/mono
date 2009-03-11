#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DbLinq.Factory;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DbLinq")]
[assembly: AssemblyDescription("DbLinq core")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM componenets.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e2381b27-cdb0-401d-9019-f72079b4928d")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyFileVersion("0.19")]

[assembly: DbLinq]

// Regarding tests, it is mandatory, since we test internals
[assembly: InternalsVisibleTo("DbLinqTest, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]
[assembly: InternalsVisibleTo("DbLinq_test, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]


// DbMetal and vendors use DbLinq's internal utilities
// this is not a requirement, but a little help
[assembly: InternalsVisibleTo("DbMetal, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

[assembly: InternalsVisibleTo("DbLinq.MySql, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

[assembly: InternalsVisibleTo("DbLinq.Oracle, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

[assembly: InternalsVisibleTo("DbLinq.PostgreSql, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]


[assembly: InternalsVisibleTo("DbLinq.Sqlite, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

[assembly: InternalsVisibleTo("DbLinq.Ingres, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

[assembly: InternalsVisibleTo("DbLinq.Firebird, PublicKey="
+ "0024000004800000940000000602000000240000525341310004000001000100c5753d8c47f400"
+ "83f549016a5711238ac8ec297605abccd3dc4b6d0f280b4764eb2cc58ec4e37831edad7e7a07b8"
+ "fe4a9cbb059374c0cc047aa28839fed7176761813caf6a2ffa0bff9afb50ead56dd3f56186a663"
+ "962a12b830c2a70eb70ec77823eb5750e5bdef9e01d097c30b5c5463c3d07d3472b58e4c02f279"
+ "2309259f")]

