//
// System.Type.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
//

using System.Reflection;

namespace System {

	//
	// FIXME: Implement the various IReflect dependencies
	//
	
	public abstract class Type : MemberInfo /* IReflect */ {

		private RuntimeTypeHandle _handle;

		/// <summary>
		///   The assembly where the type is defined.
		/// </summary>
		public abstract Assembly Assembly {
			get;
		}

		/// <summary>
		///   Gets the fully qualified name for the type including the
		///   assembly name where the type is defined.
		/// </summary>
		public abstract string AssemblyQualifiedName {
			get;
		}

		/// <summary>
		///   Returns the Attributes associated with the type.
		/// </summary>
		public TypeAttributes Attributes {
			get {
				// FIXME: Implement me.
				return 0;
			}
		}
		
		/// <summary>
		///   Returns the basetype for this type
		/// </summary>
		public abstract Type BaseType {
			get;
		}
			
		/// <summary>
		///   Returns the class that declares the member.
		/// </summary>
		public override Type DeclaringType {
			get {
				// FIXME: Implement me.
				return null;
			}
		}
		
		/// <summary>
		///
		/// </summary>
		// public static Binder DefaultBinder {
		// get;
		// }
		
		/// <summary>
		///
		/// </summary>
		
		/// <summary>
		///
		/// </summary>
		/// <summary>
		///
		/// </summary>

		/// <summary>
		///    The full name of the type including its namespace
		/// </summary>
		public abstract string FullName {
			get;
		}

		public static Type GetType(string typeName) {
			return null;
		}


		public Type GetTypeFromHandle (RuntimeTypeHandle handle) { 
			
			// FIXME: implement me
			
			return null;
		}

		public bool IsValueType {
			get {
				// FIXME
				return(false);
			}
		}
		
	}
}
