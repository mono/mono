//
// type.cs: Type container.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

namespace CIR {
	using System.Collections;
	using System;

	public class UnresolvedType {
		TypeContainer parent;
		string name;
		
		public UnresolvedType (TypeContainer parent, string name)
		{
			this.parent = parent;
			this.name = name;
		}

		public string Name {
			get {
				return name;
			}
		}

		public TypeContainer Parent {
			get {
				return parent;
			}
		}
	}
	
	public class TypeRef {
		object data;

		public TypeRef (object data)
		{
			this.data = data;
		}

		public Object Data {
			get {
				return data;
			}
		}

		public UnresolvedType UnresolvedData {
			get {
				if (data is UnresolvedType)
					return (UnresolvedType) data;
				else
					return null;
			}
		}

		public Type Type {
			get {
				if (data is UnresolvedType)
					Resolve ();
				
				return (Type) data;
			}
		}
		
		public bool IsResolved {
			get {
				return !(data is UnresolvedType);
			}
		}

		public bool Resolve () {
			return false;
		}
	}

	public class TypeRefManager {
		ArrayList pending_types;
		
		public TypeRefManager ()
		{
			pending_types = new ArrayList ();
		}

		public TypeRef GetTypeRef (TypeContainer container, string name)
		{
			object unresolved;
			TypeRef typeref;

			unresolved = new UnresolvedType (container, name);
			typeref = new TypeRef (unresolved);
			pending_types.Add (typeref);

			return typeref;
		}
	}
}



