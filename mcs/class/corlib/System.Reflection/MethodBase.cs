//
// System.Reflection/MethodBase.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class MethodBase: MemberInfo {

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static MethodBase GetCurrentMethod ();

		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle)
		{
			return null;
		}

		public abstract MethodImplAttributes GetMethodImplementationFlags();

		public abstract ParameterInfo[] GetParameters();
		
#if NET_1_2
		virtual
#endif
		public Object Invoke(Object obj, Object[] parameters) {
			return Invoke (obj, 0, null, parameters, null);
		}

		public abstract Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture);

		protected MethodBase()
		{
		}

		public abstract RuntimeMethodHandle MethodHandle { get; }
		public abstract MethodAttributes Attributes { get; }
		public virtual CallingConventions CallingConvention { get {return CallingConventions.Standard;} }
		public Boolean IsPublic { 
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public;
			}
		}
		public Boolean IsPrivate {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
			}
		}
		public Boolean IsFamily {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Family;
			}
		}
		public Boolean IsAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Assembly;
			}
		}
		public Boolean IsFamilyAndAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamANDAssem;
			}
		}
		public Boolean IsFamilyOrAssembly {
			get {
				return (Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.FamORAssem;
			}
		}
		public Boolean IsStatic {
			get {
				return (Attributes & MethodAttributes.Static) != 0;
			}
		}
		public Boolean IsFinal {
			get {
				return (Attributes & MethodAttributes.Final) != 0;
			}
		}
		public Boolean IsVirtual {
			get {
				return (Attributes & MethodAttributes.Virtual) != 0;
			}
		}
		public Boolean IsHideBySig {
			get {
				return (Attributes & MethodAttributes.HideBySig) != 0;
			}
		}
		public Boolean IsAbstract {
			get {
				return (Attributes & MethodAttributes.Abstract) != 0;
			}
		}
		public Boolean IsSpecialName {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.SpecialName) != 0;
			}
		}
		public Boolean IsConstructor {
			get {
				int attr = (int)Attributes;
				return ((attr & (int)MethodAttributes.RTSpecialName) != 0
					&& (Name == ".ctor"));
			}
		}

		internal virtual int get_next_table_index (object obj, int table, bool inc) {
			if (this is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)this;
				return mb.get_next_table_index (obj, table, inc);
			}
			if (this is ConstructorBuilder) {
				ConstructorBuilder mb = (ConstructorBuilder)this;
				return mb.get_next_table_index (obj, table, inc);
			}
			throw new Exception ("Method is not a builder method");
		}
	}
}
