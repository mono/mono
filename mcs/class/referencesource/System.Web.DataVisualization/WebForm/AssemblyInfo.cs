using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Web.UI;
using System.Runtime.InteropServices;
using System.Resources;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: InternalsVisibleTo("System.Web.DataVisualization.Design, PublicKey=" + AssemblyRef.SharedLibPublicKeyFull)]
[assembly: TagPrefix("System.Web.UI.DataVisualization.Charting", "asp")] 

#if VS_BUILD
[assembly: AssemblyVersion(ThisAssembly.Version)]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguageAttribute("")]
[assembly: AllowPartiallyTrustedCallers]
#endif //VS_BUILD

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA900:AptcaAssembliesShouldBeReviewed",
    Justification = "We have APTCA signoff, for details please refer to SWI Track, Project ID 7972")]