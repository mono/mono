//
// System.Reflection/MonoMethod.cs
// The class used to represent methods from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Reflection {
	
	internal struct MonoMethodInfo 
	{
		internal Type parent;
		internal Type ret;
		internal MethodAttributes attrs;
		internal MethodImplAttributes iattrs;
		internal CallingConventions callconv;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_method_info (IntPtr handle, out MonoMethodInfo info);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern ParameterInfo[] get_parameter_info (IntPtr handle);
	};
	
	/*
	 * Note: most of this class needs to be duplicated for the contructor, since
	 * the .NET reflection class hierarchy is so broken.
	 */
	[Serializable()]
	internal class MonoMethod : MethodInfo, ISerializable
	{
		internal IntPtr mhandle;
		string name;
		Type reftype;

		internal MonoMethod () {
		}

		internal MonoMethod (RuntimeMethodHandle mhandle) {
			this.mhandle = mhandle.Value;
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern MonoMethod get_base_definition (MonoMethod method);

		public override MethodInfo GetBaseDefinition ()
		{
			return get_base_definition (this);
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
		 * InternalInvoke() receives the parameters correctly converted by the 
		 * binder to match the types of the method signature.
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
			} catch (TargetException) {
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

		public override CallingConventions CallingConvention { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.callconv;
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern DllImportAttribute GetDllImportAttribute (IntPtr mhandle);

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			/* MS.NET doesn't report MethodImplAttribute */

			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (mhandle, out info);
			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				count ++;
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0)
				count ++;
			
			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
				attrs [count ++] = new PreserveSigAttribute ();
			if ((info.attrs & MethodAttributes.PinvokeImpl) != 0) {
				DllImportAttribute attr = GetDllImportAttribute (mhandle);
				if ((info.iattrs & MethodImplAttributes.PreserveSig) != 0)
					attr.PreserveSig = true;
				attrs [count ++] = attr;
			}

			return attrs;
		}

		public override string ToString () {
			string parms = "";
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					parms = parms + ", ";
				Type pt = p[i].ParameterType;
				if (pt.IsClass && pt.Namespace != "")
					parms = parms + pt.Namespace + "." + pt.Name;
				else
					parms = parms + pt.Name;
			}
			if (ReturnType.IsClass && ReturnType.Namespace != "")
				return ReturnType.Namespace + "." + ReturnType.Name + " " + Name + "(" + parms + ")";
			string generic = "";
#if NET_2_0 || BOOTSTRAP_NET_2_0
			if (HasGenericParameters) {
				Type[] gen_params = GetGenericArguments ();
				generic = "[";
				for (int j = 0; j < gen_params.Length; j++) {
					if (j > 0)
						generic += ",";
					generic += gen_params [j].Name;
				}
				generic += "]";
			}
#endif
			return ReturnType.Name + " " + Name + generic + "(" + parms + ")";
		}

	
		// ISerializable
		public void GetObjectData(SerializationInfo info, StreamingContext context) 
		{
			ReflectionSerializationHolder.Serialize ( info, Name, ReflectedType, ToString(), MemberTypes.Method);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern MethodInfo BindGenericParameters (Type [] types);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern Type [] GetGenericArguments ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetGenericMethodDefinition_impl ();

		public override MethodInfo GetGenericMethodDefinition ()
		{
			MethodInfo res = GetGenericMethodDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

		public override extern bool Mono_IsInflatedMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override extern bool HasGenericParameters {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override extern bool IsGenericMethodDefinition {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}
#endif
	}
	
	internal class MonoCMethod : ConstructorInfo, ISerializable
	{
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
			} catch (TargetException) {
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

		public override CallingConventions CallingConvention { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.callconv;
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

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetGenericMethodDefinition_impl ();

		public override MethodInfo GetGenericMethodDefinition ()
		{
			MethodInfo res = GetGenericMethodDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

		public override extern bool Mono_IsInflatedMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override bool HasGenericParameters {
			get {
				return false;
			}
		}

		public override bool IsGenericMethodDefinition {
			get {
				return false;
			}
		}
#endif

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

		// ISerializable
		public void GetObjectData(SerializationInfo info, StreamingContext context) 
		{
			ReflectionSerializationHolder.Serialize ( info, Name, ReflectedType, ToString(), MemberTypes.Constructor);
		}
	}
}
