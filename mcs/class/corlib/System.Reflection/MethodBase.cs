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

namespace System.Reflection {

	public abstract class MethodBase: MemberInfo {

		public static MethodBase GetCurrentMethod()
		{
			return null;
		}

		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle handle)
		{
			return null;
		}

		public abstract MethodImplAttributes GetMethodImplementationFlags();

		public abstract ParameterInfo[] GetParameters();
		
		public Object Invoke(Object obj, Object[] parameters) {
			return null;
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
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Public) != 0;
			}
		}
		public Boolean IsPrivate {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Private) != 0;
			}
		}
		public Boolean IsFamily {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Family) != 0;
			}
		}
		public Boolean IsAssembly {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Assembly) != 0;
			}
		}
		public Boolean IsFamilyAndAssembly {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.FamANDAssem) != 0;
			}
		}
		public Boolean IsFamilyOrAssembly {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.FamORAssem) != 0;
			}
		}
		public Boolean IsStatic {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Static) != 0;
			}
		}
		public Boolean IsFinal {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Final) != 0;
			}
		}
		public Boolean IsVirtual {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Virtual) != 0;
			}
		}
		public Boolean IsHideBySig {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.HideBySig) != 0;
			}
		}
		public Boolean IsAbstract {
			get {
				int attr = (int)Attributes;
				return (attr & (int)MethodAttributes.Abstract) != 0;
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
	}
}
