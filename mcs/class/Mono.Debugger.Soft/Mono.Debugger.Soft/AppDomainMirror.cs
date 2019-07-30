using System;
using System.Threading;

namespace Mono.Debugger.Soft
{
	public class AppDomainMirror : Mirror
	{
		string friendly_name;
		AssemblyMirror entry_assembly, corlib;
		AssemblyMirror[] assemblies;
		bool assembliesCacheIsInvalid = true;
		object assembliesCacheLocker = new object ();

		internal AppDomainMirror (VirtualMachine vm, long id) : base (vm, id) {
		}

		public string FriendlyName {
			get {
				/* The name can't be empty during domain creation */
				if (friendly_name == null || friendly_name == String.Empty)
					friendly_name = vm.conn.Domain_GetName (id);
				return friendly_name;
			}
	    }

		internal void InvalidateAssembliesCache () {
			assembliesCacheIsInvalid = true;
		}

		public AssemblyMirror[] GetAssemblies () {
			if (assembliesCacheIsInvalid) {
				lock (assembliesCacheLocker) {
					if (assembliesCacheIsInvalid) {
						long[] ids = vm.conn.Domain_GetAssemblies (id);
						assemblies = new AssemblyMirror [ids.Length];
						// FIXME: Uniqueness
						for (int i = 0; i < ids.Length; ++i)
							assemblies [i] = vm.GetAssembly (ids [i]);
						Thread.MemoryBarrier ();
						assembliesCacheIsInvalid = false;
					}
				}
			}
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

		public ArrayMirror CreateByteArray (byte [] bytes) {
			vm.CheckProtocolVersion (2, 52);
			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			return vm.GetObject<ArrayMirror> (vm.conn.Domain_CreateByteArray (id, bytes));
		}

		public ObjectMirror CreateBoxedValue (Value value) {
			if (value == null)
				throw new ArgumentNullException ("value");
			if (!(value is PrimitiveValue) && !(value is StructMirror))
				throw new ArgumentException ("Value must be a PrimitiveValue or a StructMirror", "value");
			if ((value is PrimitiveValue) && (value as PrimitiveValue).Value == null)
				return null;

			TypeMirror t = null;
			if (value is PrimitiveValue)
				t = GetCorrespondingType ((value as PrimitiveValue).Value.GetType ());
			else
				t = (value as StructMirror).Type;

			return vm.GetObject<ObjectMirror> (vm.conn.Domain_CreateBoxedValue (id, t.Id, vm.EncodeValue (value)));
		}

		TypeMirror[] primitiveTypes = new TypeMirror [32];
		
		public TypeMirror GetCorrespondingType (Type t) {
			if (t == null)
				throw new ArgumentNullException ("t");
			TypeCode tc = Type.GetTypeCode (t);

			if (tc == TypeCode.Empty || tc == TypeCode.Object)
				throw new ArgumentException ("t must be a primitive type", "t");

			int tc_index = (int)tc;
			if (primitiveTypes [tc_index] == null) {
				primitiveTypes [tc_index] = Corlib.GetType ("System." + t.Name, false, false);
				if (primitiveTypes [tc_index] == null)
					throw new NotImplementedException ();
			}
			return primitiveTypes [tc_index];
		}
    }
}
