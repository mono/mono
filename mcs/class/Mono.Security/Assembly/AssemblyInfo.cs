//
// AssemblyInfo.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  (sebastien@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell (http://www.novell.com)
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
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

[assembly: AssemblyVersion (Consts.FxVersion)]

[assembly: AssemblyCompany ("MONO development team")]
[assembly: AssemblyCopyright ("(c) 2003-2004 Various Authors")]
[assembly: AssemblyDescription ("Mono.Security.dll")]
[assembly: AssemblyProduct ("MONO CLI")]
[assembly: AssemblyTitle ("Mono.Security.dll")]
[assembly: CLSCompliant (true)]
[assembly: ComVisible (false)]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: AssemblyDelaySign (true)]

[assembly: InternalsVisibleTo ("System, PublicKey=" + AssemblyRef.FrameworkPublicKeyFull2)]

#if MONOTOUCH
[assembly: InternalsVisibleTo ("monotouch, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
[assembly: InternalsVisibleTo ("Xamarin.iOS, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
[assembly: InternalsVisibleTo ("Xamarin.TVOS, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#endif

#if XAMMAC || XAMMAC_4_5
[assembly: InternalsVisibleTo ("Xamarin.Mac, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#endif

