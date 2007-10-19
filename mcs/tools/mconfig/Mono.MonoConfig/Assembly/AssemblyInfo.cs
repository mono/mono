//
// AssemblyInfo.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Marek Habersack
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
using System.Runtime.InteropServices;

[assembly: AssemblyTitle ("Mono.MonoConfig")]
[assembly: AssemblyDescription ("Utility for modifying .NET configuration files")]
[assembly: AssemblyConfiguration ("Development version")]
[assembly: AssemblyProduct ("mconfig")]
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyCopyright ("Copyright (c) 2007 Novell, Inc")]
[assembly: AssemblyCulture ("")]

[assembly: CLSCompliant (false)]
[assembly: ComVisible (false)]

[assembly: AssemblyVersion ("0.1.0.0")]

#if KEYFILE
[assembly: AssemblyKeyFile("../../mono.snk")]
#endif
