using System;
using System.Reflection;
using Mono.Debugger;
using Mono.Cecil;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	public class AssemblyMirror : Mirror
	{
		string location;
		MethodMirror entry_point;
		bool entry_point_set;
		ModuleMirror main_module;
		AssemblyName aname;
		AssemblyDefinition meta;
		AppDomainMirror domain;
		Dictionary<string, long> typeCacheIgnoreCase = new Dictionary<string, long> (StringComparer.InvariantCultureIgnoreCase);
		Dictionary<string, long> typeCache = new Dictionary<string, long> ();

		internal AssemblyMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string Location {
			get {
				if (location == null)
					location = vm.conn.Assembly_GetLocation (id);
				return location;
			}
	    }

		public MethodMirror EntryPoint {
			get {
				if (!entry_point_set) {
					long mid = vm.conn.Assembly_GetEntryPoint (id);

					if (mid != 0)
						entry_point = vm.GetMethod (mid);
					entry_point_set = true;
				}
				return entry_point;
			}
	    }

		public ModuleMirror ManifestModule {
			get {
				if (main_module == null) {
					main_module = vm.GetModule (vm.conn.Assembly_GetManifestModule (id));
				}
				return main_module;
			}
		}

		// Since Protocol version 2.45
		public AppDomainMirror Domain {
			get {
				if (domain == null) {
					vm.CheckProtocolVersion (2, 45);
					domain = vm.GetDomain (vm.conn.Assembly_GetIdDomain (id));
				}
				return domain;
			}
		}

		public virtual AssemblyName GetName () {
			if (aname == null) {
				string name = vm.conn.Assembly_GetName (id);
				aname = new AssemblyName (name);
			}
			return aname;
		}

		public ObjectMirror GetAssemblyObject () {
			return vm.GetObject (vm.conn.Assembly_GetObject (id));
		}

		public TypeMirror GetType (string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("name", "Name cannot be empty");

			if (throwOnError)
				throw new NotImplementedException ();
			long typeId;
			if (ignoreCase) {
				if (!typeCacheIgnoreCase.TryGetValue (name, out typeId)) {
					typeId = vm.conn.Assembly_GetType (id, name, ignoreCase);
					typeCacheIgnoreCase.Add (name, typeId);
					var type = vm.GetType (typeId);
					if (type != null) {
						typeCache.Add (type.FullName, typeId);
					}
					return type;
				}
			} else {
				if (!typeCache.TryGetValue (name, out typeId)) {
					typeId = vm.conn.Assembly_GetType (id, name, ignoreCase);
					typeCache.Add (name, typeId);
				}
			}
			return vm.GetType (typeId);
		}

		public TypeMirror GetType (String name, Boolean throwOnError)
		{
			return GetType (name, throwOnError, false);
		}

		public TypeMirror GetType (String name) {
			return GetType (name, false, false);
		}

		/* 
		 * An optional Cecil assembly which could be used to access metadata instead
		 * of reading it from the debuggee.
		 */
		public AssemblyDefinition Metadata {
			get {
				return meta;
			}
			set {
				if (value.MainModule.Name != ManifestModule.Name)
					throw new ArgumentException ("The supplied assembly is named '" + value.MainModule.Name + "', while the assembly in the debuggee is named '" + ManifestModule.Name + "'.");
				if (value.MainModule.Mvid != ManifestModule.ModuleVersionId)
					throw new ArgumentException ("The supplied assembly's main module has guid '" + value.MainModule.Mvid + ", while the assembly in the debuggee has guid '" + ManifestModule.ModuleVersionId + "'.", "value");
				meta = value;
			}
		}
    }
}
