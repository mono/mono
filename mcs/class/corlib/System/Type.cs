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
using System.Runtime.CompilerServices;

namespace System {

	//
	// FIXME: Implement the various IReflect dependencies
	//
	
	public abstract class Type : MemberInfo /* IReflect */ {
		private RuntimeTypeHandle type_handle;

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_handle (RuntimeTypeHandle handle);
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type internal_from_name (string name);
		
		public static Type GetType(string typeName)
		{
			return internal_from_name (typeName);
		}

		public static Type GetTypeFromHandle (RuntimeTypeHandle handle)
		{ 
			return internal_from_handle (handle);
		}

		public RuntimeTypeHandle TypeHandle {
			get {
				return type_handle;
			}
		}
		
		public bool IsValueType {
			get {
				// FIXME
				return(false);
			}
		}

		public bool IsClass {
			get {
				// FIXME
				return true;
			}
		}

		public bool IsInterface {
			get {
				// FIXME
				return false;
			}
		}

		public bool IsArray {
			get {
				// FIXME
				return false;
			}
		}

		public bool IsSubclassOf (Type c)
		{
			// FIXME
			return false;
		}

		public virtual Type[] FindInterfaces (TypeFilter filter, object filterCriteria)
		{
			// FIXME
			return null;
		}

		public abstract Type[] GetInterfaces ();
		
		public virtual bool IsAssignableFrom (Type c)
		{
			// FIXME
			return false;
		}

		public virtual int GetArrayRank ()
		{
			// FIXME
			return 0;
		}

		public abstract Type GetElementType ();

		public bool IsSealed {
			get {
				// FIXME
				return false;
			}
		}

		public bool IsAbstract {
			get {
				// FIXME
				return false;
			}
		}
	}
}
