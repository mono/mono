//
// AssemblyInfo.cs
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
[assembly: AssemblyTitle ("SMDiagnostics.dll")]
[assembly: AssemblyDescription ("Contains share code for some System.ServiceModel libraries")]
[assembly: AssemblyDefaultAlias ("System.ServiceModel.dll")]

[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]
[assembly: CLSCompliant (true)]
[assembly: AssemblyDelaySign (true)]
#if MOBILE
[assembly: AssemblyKeyFile ("../winfx.pub")]
#else
[assembly: AssemblyKeyFile ("../ecma.pub")]
[assembly: AllowPartiallyTrustedCallers]
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
#endif
[assembly: InternalsVisibleTo ("System.IdentityModel, PublicKey=" + AssemblyRef.FrameworkPublicKeyFull)]
[assembly: InternalsVisibleTo ("System.IdentityModel.Selectors, PublicKey=" + AssemblyRef.FrameworkPublicKeyFull)]
[assembly: InternalsVisibleTo ("System.ServiceModel, PublicKey=" + AssemblyRef.FrameworkPublicKeyFull)]
[assembly: InternalsVisibleTo ("System.ServiceModel.Web, PublicKey=" + AssemblyRef.FrameworkPublicKeyFull)]

[assembly: ComVisible (false)]

