using System;
using Mono.Debugger;
using Mono.Cecil;

namespace Mono.Debugger.Soft
{
	public class ModuleMirror : Mirror
	{
		ModuleInfo info;
		Guid guid;
		AssemblyMirror assembly;

		internal ModuleMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		void ReadInfo () {
			if (info == null)
				info = vm.conn.Module_GetInfo (id);
		}

		public string Name {
			get {
				ReadInfo ();
				return info.Name;
			}
		}

		public string ScopeName {
			get {
				ReadInfo ();
				return info.ScopeName;
			}
		}

		public string FullyQualifiedName {
			get {
				ReadInfo ();
				return info.FQName;
			}
		}

		public Guid ModuleVersionId {
			get {
				if (guid == Guid.Empty) {
					ReadInfo ();
					guid = new Guid (info.Guid);
				}
				return guid;
			}
		}

		public AssemblyMirror Assembly {
			get {
				if (assembly == null) {
					ReadInfo ();
					if (info.Assembly == 0)
						return null;
					assembly = vm.GetAssembly (info.Assembly);
				}
				return assembly;
			}
		}

		// FIXME: Add function to query the guid, check in Metadata

		// Since protocol version 2.48
		public string SourceLink {
			get {
				vm.CheckProtocolVersion (2, 48);
				ReadInfo ();
				return info.SourceLink;
			}
		}
    }
}
