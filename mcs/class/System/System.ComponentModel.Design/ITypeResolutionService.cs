//
// System.ComponentModel.Design.ITypeResolutionService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Reflection;

namespace System.ComponentModel.Design
{
	public interface ITypeResolutionService
	{
		Assembly GetAssembly (AssemblyName name);
		Assembly GetAssembly (AssemblyName name, bool throwOnError);
		string GetPathOfAssembly (AssemblyName name);
		Type GetType (string name);
		Type GetType (string name, bool throwOnError);
		void ReferenceAssembly (AssemblyName name);
	}
}
