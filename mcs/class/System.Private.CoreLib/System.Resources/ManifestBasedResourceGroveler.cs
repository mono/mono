using System.Globalization;
using System.Reflection;

namespace System.Resources
{
	partial class ManifestBasedResourceGroveler
	{
		static Assembly InternalGetSatelliteAssembly (Assembly mainAssembly, CultureInfo culture, Version version)
		{
			return ((RuntimeAssembly)mainAssembly).InternalGetSatelliteAssembly (culture, version, throwOnFileNotFound: false);
		}

		static bool GetNeutralResourcesLanguageAttribute (Assembly assemblyHandle, ref string cultureName, out short fallbackLocation)
		{
			throw new NotImplementedException ();
		}
	}
}