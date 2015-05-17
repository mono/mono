//
// DynamicModuleManager.cs: Manager for dynamic Http Modules.
//
// Author:
//   Matthias Bogad (bogad@cs.tum.edu)
//
// (C) 2015
//

using System;
using System.Collections.Generic;

namespace System.Web {
	sealed class DynamicModuleManager {
		const string moduleNameFormat = "__Module__{0}_{1}";
	
		readonly List<DynamicModuleInfo> entries = new List<DynamicModuleInfo> ();
		bool entriesAreReadOnly = false;
		readonly object mutex = new object ();
		
		public void Add (Type moduleType) 
		{
			if (moduleType == null)
				throw new ArgumentException ("moduleType");
			
			if (!typeof (IHttpModule).IsAssignableFrom (moduleType))
				throw new ArgumentException ("Given object does not implement IHttpModule.", "moduleType");
			
			lock (mutex) {
				if (entriesAreReadOnly)
					throw new InvalidOperationException ("A module was to be added to the dynamic module list, but the list was already initialized. The dynamic module list can only be initialized once.");

				entries.Add (new DynamicModuleInfo (moduleType,
							string.Format (moduleNameFormat, moduleType.AssemblyQualifiedName, Guid.NewGuid ())));
			}
		}
		
		public ICollection<DynamicModuleInfo> LockAndGetModules ()
		{
			lock (mutex) {
				entriesAreReadOnly = true;
				return entries;
			}
		}
	}

	struct DynamicModuleInfo {
		public readonly string Name;
		public readonly Type Type;

		public DynamicModuleInfo (Type type, string name)
		{
			Name = name;
			Type = type;
		}
	}
}
