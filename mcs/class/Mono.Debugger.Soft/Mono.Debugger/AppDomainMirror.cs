using System;

namespace Mono.Debugger
{
	public class AppDomainMirror : Mirror
	{
		string friendly_name;
		AssemblyMirror entry_assembly, corlib;

		internal AppDomainMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string FriendlyName {
			get {
				if (friendly_name == null)
					friendly_name = vm.conn.Domain_GetName (id);
				return friendly_name;
			}
	    }

		// Not cached
		public AssemblyMirror[] GetAssemblies () {
			long[] ids = vm.conn.Domain_GetAssemblies (id);
			AssemblyMirror[] assemblies = new AssemblyMirror [ids.Length];
			// FIXME: Uniqueness
			for (int i = 0; i < ids.Length; ++i)
				assemblies [i] = vm.GetAssembly (ids [i]);
			return assemblies;
	    }

		// This returns null when called before the first AssemblyLoad event
		public AssemblyMirror GetEntryAssembly () {
			if (entry_assembly == null) {
				long ass_id = vm.conn.Domain_GetEntryAssembly (id);

				entry_assembly = vm.GetAssembly (ass_id);
			}
			return entry_assembly;
	    }

		public AssemblyMirror Corlib {
			get {
				if (corlib == null) {
					long ass_id = vm.conn.Domain_GetCorlib (id);

					corlib = vm.GetAssembly (ass_id);
				}
				return corlib;
			}
	    }

		public StringMirror CreateString (string s) {
			if (s == null)
				throw new ArgumentNullException ("s");

			return vm.GetObject<StringMirror> (vm.conn.Domain_CreateString (id, s));
		}


    }
}
