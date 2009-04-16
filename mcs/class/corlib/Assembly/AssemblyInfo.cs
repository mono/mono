//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the mscorlib assembly

#if NET_2_0
	[assembly: AssemblyTitle ("mscorlib.dll")]
	[assembly: AssemblyDescription ("mscorlib.dll")]
	[assembly: AssemblyDefaultAlias ("mscorlib.dll")]

	[assembly: AssemblyCompany (Consts.MonoCompany)]
	[assembly: AssemblyProduct (Consts.MonoProduct)]
	[assembly: AssemblyCopyright (Consts.MonoCopyright)]

	[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
#endif

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: CLSCompliant (true)]
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
[assembly: Guid ("BED7F4EA-1A96-11D2-8F08-00A0C9A6186D")]
[assembly: AllowPartiallyTrustedCallers]

#if !BOOTSTRAP_WITH_OLDLIB
	[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification = true)]
	[assembly: AssemblyDelaySign (true)]
#if NET_2_1
	[assembly: AssemblyKeyFile ("../silverlight.pub")]
#else
	[assembly: AssemblyKeyFile ("../ecma.pub")]
#endif
#endif

#if NET_2_0
	[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
	[assembly: ComVisible (false)]
	[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
	[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
	[assembly: TypeLibVersion (2, 0)]
	[assembly: RuntimeCompatibility (WrapNonExceptionThrows = true)]
	[assembly: DefaultDependency (LoadHint.Always)]
	[assembly: StringFreezing]
#elif NET_1_1 || NET_1_0
	[assembly: AssemblyDescription("Common Language Runtime Library")]
	[assembly: TypeLibVersion (1, 10)]
#endif

#if NET_2_1
	[assembly: InternalsVisibleTo ("System, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB")]
	[assembly: InternalsVisibleTo ("System.Windows, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB")]
	[assembly: InternalsVisibleTo ("System.Net, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB")]
	[assembly: InternalsVisibleTo ("System.Runtime.Serialization, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB")]
	[assembly: InternalsVisibleTo ("System.Xml, PublicKey=00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB")]
	[assembly: InternalsVisibleTo ("Mono.CompilerServices.SymbolWriter, PublicKey=002400000480000094000000060200000024000052534131000400000100010079159977D2D03A8E6BEA7A2E74E8D1AFCC93E8851974952BB480A12C9134474D04062447C37E0E68C080536FCF3C3FBE2FF9C979CE998475E506E8CE82DD5B0F350DC10E93BF2EEECF874B24770C5081DBEA7447FDDAFA277B22DE47D6FFEA449674A4F9FCCF84D15069089380284DBDD35F46CDFF12A1BD78E4EF0065D016DF")]
#endif
