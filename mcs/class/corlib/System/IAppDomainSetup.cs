//
// System.IAppDomainSetup
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
// (C) 2002, Duco Fijma
//

namespace System {

	public interface IAppDomainSetup {

		string ApplicationBase {get; }
		string ApplicationName {get; }
		string CachePath {get; }
		string ConfigurationFile {get; }
		string DynamicBase {get; }
		string LicenseFile {get; }
		string PrivateBinPath {get; }
		string PrivateBinPathProbe {get; }
		string ShadowCopyDirectories {get; }
		string ShadowCopyFiles {get; }

	}
}
