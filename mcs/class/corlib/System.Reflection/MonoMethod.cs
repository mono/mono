//
// System.Reflection/MonoMethod.cs
// The class used to represent methods from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {
	internal struct MonoMethodInfo {
		internal Type parent;
		internal Type ret;
		internal MethodAttributes attrs;
		internal MethodImplAttributes iattrs;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_method_info (IntPtr handle, out MonoMethodInfo info);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern ParameterInfo[] get_parameter_info (IntPtr handle);
	};
	
	/*
	 * Note: most of this class needs to be duplicated for the contructor, since
	 * the .NET reflection class hierarchy is so broken.
	 */
	internal class MonoMethod : MethodInfo {
		internal IntPtr mhandle;
		string name;
		Type reftype;
		
		[MonoTODO]
		public override MethodInfo GetBaseDefinition() {
			return this; /* FIXME */
		}
		public override Type ReturnType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.ret;
			}
		}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes { 
			get {
				return new ParameterInfo (ReturnType, this);
			}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (mhandle, out info);
			return info.iattrs;
		}

		public override ParameterInfo[] GetParameters() {
			return MonoMethodInfo.get_parameter_info (mhandle);
		}

		/*
		 * InternalInvoke() receives the parameters corretcly converted by the binder
		 * to match the types of the method signature.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters);
		
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			if (binder == null)
				binder = Binder.DefaultBinder;
			ParameterInfo[] pinfo = GetParameters ();
			if (!Binder.ConvertArgs (binder, parameters, pinfo, culture))
				throw new ArgumentException ("parameters");
			try {
				return InternalInvoke (obj, parameters);
			} catch (TargetException e) {
				throw;
			} catch (Exception e) {
				throw new TargetInvocationException (e);
			}
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return new RuntimeMethodHandle (mhandle);} 
		}
		public override MethodAttributes Attributes { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.attrs;
			} 
		}
		
		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				return name;
			}
		}
		
		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override string ToString () {
			string parms = "";
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					parms = parms + ", ";
				parms = parms + p [i].ParameterType.Name;
			}
			return ReturnType.Name+" "+Name+"("+parms+")";
		}
	}
	
	internal class MonoCMethod : ConstructorInfo {
		internal IntPtr mhandle;
		string name;
		Type reftype;
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (mhandle, out info);
			return info.iattrs;
		}

		public override ParameterInfo[] GetParameters() {
			return MonoMethodInfo.get_parameter_info (mhandle);
		}

		/*
		 * InternalInvoke() receives the parameters corretcly converted by the binder
		 * to match the types of the method signature.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters);
		
		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			if (binder == null)
				binder = Binder.DefaultBinder;
			ParameterInfo[] pinfo = GetParameters ();
			if (!Binder.ConvertArgs (binder, parameters, pinfo, culture))
				throw new ArgumentException ("parameters");
			try {
				return InternalInvoke (obj, parameters);
			} catch (TargetException e) {
				throw;
			} catch (Exception e) {
				throw new TargetInvocationException (e);
			}
		}

		public override Object Invoke (BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return Invoke (null, invokeAttr, binder, parameters, culture);
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return new RuntimeMethodHandle (mhandle);} 
		}
		public override MethodAttributes Attributes { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.attrs;
			} 
		}
		
		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				return name;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override string ToString () {
			string parms = "";
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					parms = parms + ", ";
				parms = parms + p [i].ParameterType.Name;
			}
			return "Void "+Name+"("+parms+")";
		}
	}
}
