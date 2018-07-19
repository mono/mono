using System.Runtime.CompilerServices;

#if !SILVERLIGHT 
[assembly:StringFreezingAttribute()]

[assembly:DefaultDependencyAttribute(LoadHint.Always)]

[assembly:System.Runtime.InteropServices.TypeLibVersion(2, 4)]

#if !FEATURE_PAL
// Opts into the VS loading icons from the Icon Satellite assembly
[assembly: System.Drawing.BitmapSuffixInSatelliteAssemblyAttribute()]
#endif

#endif

