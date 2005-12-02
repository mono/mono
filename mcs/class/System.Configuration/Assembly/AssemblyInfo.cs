//
// AssemblyInfo.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Configuration.Install assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

[assembly: AssemblyTitle ("System.Configuration.dll")]
[assembly: AssemblyDescription ("System.Configuration.dll")]
[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyCopyright ("(c) 2005 Various Authors")]

[assembly: CLSCompliant (true)]
[assembly: AssemblyDefaultAlias ("System.Configuration.dll")]
[assembly: AssemblyInformationalVersion ("0.0.0.1")]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile("../msfinal.pub")]

#if NET_2_0
[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
#endif
[assembly: AllowPartiallyTrustedCallers]
