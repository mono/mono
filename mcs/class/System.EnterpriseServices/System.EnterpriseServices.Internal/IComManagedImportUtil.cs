// System.EnterpriseServices.Internal.IComManagedImportUtil.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//

using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
	//[Guid("")]
	public interface IComManagedImportUtil 
	{
		void GetComponentInfo (string assemblyPath, out string numComponents, out string componentInfo);

		void InstallAssembly (string filename, string parname, string appname);
	}
}
