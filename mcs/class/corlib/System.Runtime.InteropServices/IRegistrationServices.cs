//
// System.Runtime.InteropServices.IRegistrationServices.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

using System.Reflection;

namespace System.Runtime.InteropServices {

	[Guid("CCBD682C-73A5-4568-B8B0-C7007E11ABA2")]
	public interface IRegistrationServices {
		Guid GetManagedCategoryGuid ();
		string GetProgIdForType (Type type);
		Type[] GetRegistrableTypesInAssembly (Assembly assembly);
		bool RegisterAssembly (Assembly assembly, AssemblyRegistrationFlags flags);
		void RegisterTypeForComClients (Type type, ref Guid g);
		bool TypeRepresentsComType (Type type);
		bool TypeRequiresRegistration (Type type);
		bool UnregisterAssembly (Assembly assembly);
	}
}
