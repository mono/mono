// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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
// ***********************************************************************

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NUnitLite")]
[assembly: AssemblyDescription("NUnitLite unit-testing framework")]
[assembly: AssemblyCompany("NUnit Software")]
[assembly: AssemblyProduct("NUnitLite")]
[assembly: AssemblyCopyright("Copyright 2013, Charlie Poole")]
[assembly: AssemblyTrademark("NUnitLite")]
[assembly: AssemblyCulture("")]

// Set AssemblyConfiguration attribute depending on
// how we are building the assembly.
#if DEBUG
#if NET_4_5
[assembly: AssemblyConfiguration(".NET 4.5 Debug")]
#elif NET_4_0
[assembly: AssemblyConfiguration(".NET 4.0 Debug")]
#elif NET_3_5
[assembly: AssemblyConfiguration(".NET 3.5 Debug")]
#elif NET_2_0
[assembly: AssemblyConfiguration(".NET 2.0 Debug")]
#elif NET_1_1
[assembly: AssemblyConfiguration(".NET 1.1 Debug")]
#elif NETCF_3_5
[assembly: AssemblyConfiguration(".NET CF 3.5 Debug")]
#elif NETCF_2_0
[assembly: AssemblyConfiguration(".NET CF 2.0 Debug")]
#elif SL_5_0
[assembly: AssemblyConfiguration("Silverlight 5.0 Debug")]
#elif SL_4_0
[assembly: AssemblyConfiguration("Silverlight 4.0 Debug")]
#elif SL_3_0
[assembly: AssemblyConfiguration("Silverlight 3.0 Debug")]
#endif
#else
#if NET_4_5
[assembly: AssemblyConfiguration(".NET 4.5")]
#elif NET_4_0
[assembly: AssemblyConfiguration(".NET 4.0")]
#elif NET_3_5
[assembly: AssemblyConfiguration(".NET 3.5")]
#elif NET_2_0
[assembly: AssemblyConfiguration(".NET 2.0")]
#elif NET_1_1
[assembly: AssemblyConfiguration(".NET 1.1")]
#elif NETCF_3_5
[assembly: AssemblyConfiguration(".NET CF 3.5")]
#elif NETCF_2_0
[assembly: AssemblyConfiguration(".NET CF 2.0")]
#elif SL_5_0
[assembly: AssemblyConfiguration("Silverlight 5.0")]
#elif SL_4_0
[assembly: AssemblyConfiguration("Silverlight 4.0")]
#elif SL_3_0
[assembly: AssemblyConfiguration("Silverlight 3.0")]
#endif
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0be367fd-d825-4039-a70b-54a3557170ec")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
#if !PocketPC && !WindowsCE && !NETCF
[assembly: AssemblyFileVersion("1.0.0.0")]
#endif
