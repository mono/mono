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

	//[Guid("")]
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
