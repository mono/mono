using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;

[assembly: SecurityCritical]

[assembly: AssemblyTitle ("System.Data.Services.dll")]
[assembly: AssemblyDescription ("System.Data.Services.dll")]
[assembly: AssemblyDefaultAlias ("System.Data.Services.dll")]

[assembly: AssemblyCompany(Consts.MonoCompany)]
[assembly: AssemblyProduct(Consts.MonoProduct)]
[assembly: AssemblyCopyright(Consts.MonoCopyright)]
[assembly: AssemblyVersion(Consts.FxVersion)]
[assembly: SatelliteContractVersion(Consts.FxVersion)]
[assembly: AssemblyInformationalVersion(Consts.FxFileVersion)]
[assembly: AssemblyFileVersion(Consts.FxFileVersion)]

[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: AssemblyDelaySign (true)]
[assembly: AssemblyKeyFile ("../ecma.pub")]

[assembly: ComVisible (false)]
[assembly: CLSCompliant (true)]
