//
// AssemblyInfo.cs
//
// Authors:
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the assembly

[assembly: AssemblyTitle ("System.Net.Http.dll")]
[assembly: AssemblyDescription ("System.Net.Http.dll")]
[assembly: AssemblyDefaultAlias ("System.Net.Http.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]

#if MOBILE
[assembly: AssemblyVersion ("4.0.0.0")]
[assembly: SatelliteContractVersion ("4.0.0.0")]
[assembly: AssemblyInformationalVersion ("4.0.50524.0")]
[assembly: AssemblyFileVersion ("4.0.50524.0")]
#else
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
#endif

[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]


[assembly: ComVisible (false)]

[assembly: InternalsVisibleTo ("System.Net.Http.WebRequest, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]

#if MONOTOUCH

#if MONOTOUCH_TV
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.NSUrlSessionHandler))]
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.CFNetworkHandler))]
[assembly: InternalsVisibleTo ("Xamarin.TVOS, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#elif MONOTOUCH_WATCH
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.NSUrlSessionHandler))]
[assembly: InternalsVisibleTo ("Xamarin.WatchOS, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#else
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.NSUrlSessionHandler))]
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.CFNetworkHandler))]
[assembly: InternalsVisibleTo ("Xamarin.iOS, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#endif

#endif

#if XAMMAC || XAMMAC_4_5
[assembly: TypeForwardedToAttribute (typeof (Foundation.NSUrlSessionHandler))]
[assembly: TypeForwardedToAttribute (typeof (System.Net.Http.CFNetworkHandler))]
[assembly: InternalsVisibleTo ("Xamarin.Mac, PublicKey=0024000004800000940000000602000000240000525341310004000011000000438ac2a5acfbf16cbd2b2b47a62762f273df9cb2795ceccdf77d10bf508e69e7a362ea7a45455bbf3ac955e1f2e2814f144e5d817efc4c6502cc012df310783348304e3ae38573c6d658c234025821fda87a0be8a0d504df564e2c93b2b878925f42503e9d54dfef9f9586d9e6f38a305769587b1de01f6c0410328b2c9733db")]
#endif
