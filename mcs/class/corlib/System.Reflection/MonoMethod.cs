//
// System.Reflection/MonoMethod.cs
// The class used to represent methods from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Reflection.Emit;
using System.Security;
using System.Threading;
using System.Text;


namespace System.Reflection {
	
	internal struct MonoMethodInfo 
	{
#pragma warning disable 649	
		private Type parent;
		private Type ret;
		internal MethodAttributes attrs;
		internal MethodImplAttributes iattrs;
		private CallingConventions callconv;
#pragma warning restore 649		

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void get_method_info (IntPtr handle, out MonoMethodInfo info);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int get_method_attributes (IntPtr handle);
		
		internal static MonoMethodInfo GetMethodInfo (IntPtr handle)
		{
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (handle, out info);
			return info;
		}

		internal static Type GetDeclaringType (IntPtr handle)
		{
			return GetMethodInfo (handle).parent;
		}

		internal static Type GetReturnType (IntPtr handle)
		{
			return GetMethodInfo (handle).ret;
		}

		internal static MethodAttributes GetAttributes (IntPtr handle)
		{
			return (MethodAttributes)get_method_attributes (handle);
		}

		internal static CallingConventions GetCallingConvention (IntPtr handle)
		{
			return GetMethodInfo (handle).callconv;
		}

		internal static MethodImplAttributes GetMethodImplementationFlags (IntPtr handle)
		{
			return GetMethodInfo (handle).iattrs;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern ParameterInfo[] get_parameter_info (IntPtr handle, MemberInfo member);

		static internal ParameterInfo[] GetParametersInfo (IntPtr handle, MemberInfo member)
		{
			return get_parameter_info (handle, member);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern UnmanagedMarshal get_retval_marshal (IntPtr handle);

		static internal ParameterInfo GetReturnParameterInfo (MonoMethod method)
		{
			return new ParameterInfo (GetReturnType (method.mhandle), method, get_retval_marshal (method.mhandle));
		}
	};
	
	/*
	 * Note: most of this class needs to be duplicated for the contructor, since
	 * the .NET reflection class hierarchy is so broken.
	 */
	[Serializable()]
	internal class MonoMethod : MethodInfo, ISerializable
	{
#pragma warning disable 649
		internal IntPtr mhandle;
		string name;
		Type reftype;
#pragma warning restore 649

		internal MonoMethod () {
		}

		internal MonoMethod (RuntimeMethodHandle mhandle) {
			this.mhandle = mhandle.Value;
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern string get_name (MethodBase method);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern MonoMethod get_base_method (MonoMethod method, bool definition);

		public override MethodInfo GetBaseDefinition ()
		{
			return get_base_method (this, true);
		}

		internal override MethodInfo GetBaseMethod ()
		{
			return get_base_method (this, false);
		}

		public override ParameterInfo ReturnParameter {
			get {
				return MonoMethodInfo.GetReturnParameterInfo (this);
			}
		}

		public override Type ReturnType {
			get {
				return MonoMethodInfo.GetReturnType (mhandle);
			}
		}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes { 
			get {
				return MonoMethodInfo.GetReturnParameterInfo (this);
			}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return MonoMethodInfo.GetMethodImplementationFlags (mhandle);
		}

		public override ParameterInfo[] GetParameters ()
		{
			ParameterInfo[] src = MonoMethodInfo.GetParametersInfo (mhandle, this);
			ParameterInfo[] res = new ParameterInfo [src.Length];
			src.CopyTo (res, 0);
			return res;
		}
		
		internal override int GetParameterCount ()
		{
			var pi = MonoMethodInfo.GetParametersInfo (mhandle, this);
			return pi == null ? 0 : pi.Length;
		}

		/*
		 * InternalInvoke() receives the parameters correctly converted by the 
		 * binder to match the types of the method signature.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters, out Exception exc);

		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) 
		{
			if (binder == null)
				binder = Binder.DefaultBinder;
			/*Avoid allocating an array every time*/
			ParameterInfo[] pinfo = MonoMethodInfo.GetParametersInfo (mhandle, this);

			if ((parameters == null && pinfo.Length != 0) || (parameters != null && parameters.Length != pinfo.Length))
				throw new TargetParameterCountException ("parameters do not match signature");
			
			if ((invokeAttr & BindingFlags.ExactBinding) == 0) {
				if (!Binder.ConvertArgs (binder, parameters, pinfo, culture))
					throw new ArgumentException ("failed to convert parameters");
			} else {
				for (int i = 0; i < pinfo.Length; i++)
					if (parameters[i].GetType() != pinfo[i].ParameterType)
						throw new ArgumentException ("parameters do not match signature");
			}

#if !NET_2_1
			if (SecurityManager.SecurityEnabled) {
				// sadly Attributes doesn't tell us which kind of security action this is so
				// we must do it the hard way - and it also means that we can skip calling
				// Attribute (which is another an icall)
				SecurityManager.ReflectedLinkDemandInvoke (this);
			}
#endif

			if (ContainsGenericParameters)
				throw new InvalidOperationException ("Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true.");

			Exception exc;
			object o = null;

			try {
				// The ex argument is used to distinguish exceptions thrown by the icall
				// from the exceptions thrown by the called method (which need to be
				// wrapped in TargetInvocationException).
				o = InternalInvoke (obj, parameters, out exc);
			} catch (ThreadAbortException) {
				throw;
#if NET_2_1
			} catch (MethodAccessException) {
				throw;
#endif
			} catch (Exception e) {
				throw new TargetInvocationException (e);
			}

			if (exc != null)
				throw exc;
			return o;
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return new RuntimeMethodHandle (mhandle);} 
		}
		public override MethodAttributes Attributes { 
			get {
				return MonoMethodInfo.GetAttributes (mhandle);
			} 
		}

		public override CallingConventions CallingConvention { 
			get {
				return MonoMethodInfo.GetCallingConvention (mhandle);
			}
		}
		
		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				return MonoMethodInfo.GetDeclaringType (mhandle);
			}
		}
		public override string Name {
			get {
				if (name != null)
					return name;
				return get_name (this);
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

			MonoMethodInfo info = MonoMethodInfo.GetMethodInfo (mhandle);
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

		static bool ShouldPrintFullName (Type type) {
			return type.IsClass && (!type.IsPointer ||
 				(!type.GetElementType ().IsPrimitive && !type.GetElementType ().IsNested));
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			Type retType = ReturnType;
			if (ShouldPrintFullName (retType))
				sb.Append (retType.ToString ());
			else
				sb.Append (retType.Name);
			sb.Append (" ");
			sb.Append (Name);
			if (IsGenericMethod) {
				Type[] gen_params = GetGenericArguments ();
				sb.Append ("[");
				for (int j = 0; j < gen_params.Length; j++) {
					if (j > 0)
						sb.Append (",");
					sb.Append (gen_params [j].Name);
				}
				sb.Append ("]");
			}
			sb.Append ("(");
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					sb.Append (", ");
				Type pt = p[i].ParameterType;
				bool byref = pt.IsByRef;
				if (byref)
					pt = pt.GetElementType ();
				if (ShouldPrintFullName (pt))
					sb.Append (pt.ToString ());
				else
					sb.Append (pt.Name);
				if (byref)
					sb.Append (" ByRef");
			}
			if ((CallingConvention & CallingConventions.VarArgs) != 0) {
				if (p.Length > 0)
					sb.Append (", ");
				sb.Append ("...");
			}
			
			sb.Append (")");
			return sb.ToString ();
		}

	
		// ISerializable
		public void GetObjectData(SerializationInfo info, StreamingContext context) 
		{
			Type[] genericArguments = IsGenericMethod && !IsGenericMethodDefinition
				? GetGenericArguments () : null;
			MemberInfoSerializationHolder.Serialize ( info, Name, ReflectedType, ToString(), MemberTypes.Method, genericArguments);
		}

		public override MethodInfo MakeGenericMethod (Type [] methodInstantiation)
		{
			if (methodInstantiation == null)
				throw new ArgumentNullException ("methodInstantiation");

			if (!IsGenericMethodDefinition)
				throw new InvalidOperationException ("not a generic method definition");

			/*FIXME add GetGenericArgumentsLength() internal vcall to speed this up*/
			if (GetGenericArguments ().Length != methodInstantiation.Length)
				throw new ArgumentException ("Incorrect length");

			bool hasUserType = false;
			foreach (Type type in methodInstantiation) {
				if (type == null)
					throw new ArgumentNullException ();
				if (!(type is MonoType))
					hasUserType = true;
			}

			if (hasUserType)
				return new MethodOnTypeBuilderInst (this, methodInstantiation);

			MethodInfo ret = MakeGenericMethod_impl (methodInstantiation);
			if (ret == null)
				throw new ArgumentException (String.Format ("The method has {0} generic parameter(s) but {1} generic argument(s) were provided.", GetGenericArguments ().Length, methodInstantiation.Length));
			return ret;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo MakeGenericMethod_impl (Type [] types);

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

		public override extern bool IsGenericMethodDefinition {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override extern bool IsGenericMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericMethod) {
					foreach (Type arg in GetGenericArguments ())
						if (arg.ContainsGenericParameters)
							return true;
				}
				return DeclaringType.ContainsGenericParameters;
			}
		}

		public override MethodBody GetMethodBody () {
			return GetMethodBody (mhandle);
		}

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif
	}
	
	internal class MonoCMethod : ConstructorInfo, ISerializable
	{
#pragma warning disable 649		
		internal IntPtr mhandle;
		string name;
		Type reftype;
#pragma warning restore 649		
		
		public override MethodImplAttributes GetMethodImplementationFlags ()
		{
			return MonoMethodInfo.GetMethodImplementationFlags (mhandle);
		}

		public override ParameterInfo[] GetParameters ()
		{
			return MonoMethodInfo.GetParametersInfo (mhandle, this);
		}

		internal override int GetParameterCount ()
		{
			var pi = MonoMethodInfo.GetParametersInfo (mhandle, this);
			return pi == null ? 0 : pi.Length;
		}

		/*
		 * InternalInvoke() receives the parameters corretcly converted by the binder
		 * to match the types of the method signature.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern Object InternalInvoke (Object obj, Object[] parameters, out Exception exc);

		public override Object Invoke (Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) 
		{
			if (binder == null)
				binder = Binder.DefaultBinder;

			ParameterInfo[] pinfo = GetParameters ();

			if ((parameters == null && pinfo.Length != 0) || (parameters != null && parameters.Length != pinfo.Length))
				throw new TargetParameterCountException ("parameters do not match signature");
			
			if ((invokeAttr & BindingFlags.ExactBinding) == 0) {
				if (!Binder.ConvertArgs (binder, parameters, pinfo, culture))
					throw new ArgumentException ("failed to convert parameters");
			} else {
				for (int i = 0; i < pinfo.Length; i++)
					if (parameters[i].GetType() != pinfo[i].ParameterType)
						throw new ArgumentException ("parameters do not match signature");
			}

#if !NET_2_1
			if (SecurityManager.SecurityEnabled) {
				// sadly Attributes doesn't tell us which kind of security action this is so
				// we must do it the hard way - and it also means that we can skip calling
				// Attribute (which is another an icall)
				SecurityManager.ReflectedLinkDemandInvoke (this);
			}
#endif

			if (obj == null && DeclaringType.ContainsGenericParameters)
				throw new MemberAccessException ("Cannot create an instance of " + DeclaringType + " because Type.ContainsGenericParameters is true.");

			if ((invokeAttr & BindingFlags.CreateInstance) != 0 && DeclaringType.IsAbstract) {
				throw new MemberAccessException (String.Format ("Cannot create an instance of {0} because it is an abstract class", DeclaringType));
			}

			Exception exc = null;
			object o = null;

			try {
				o = InternalInvoke (obj, parameters, out exc);
#if NET_2_1
			} catch (MethodAccessException) {
				throw;
#endif
			} catch (Exception e) {
				throw new TargetInvocationException (e);
			}

			if (exc != null)
				throw exc;
			return (obj == null) ? o : null;
		}

		public override Object Invoke (BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return Invoke (null, invokeAttr, binder, parameters, culture);
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return new RuntimeMethodHandle (mhandle);} 
		}
		public override MethodAttributes Attributes { 
			get {
				return MonoMethodInfo.GetAttributes (mhandle);
			} 
		}

		public override CallingConventions CallingConvention { 
			get {
				return MonoMethodInfo.GetCallingConvention (mhandle);
			}
		}
		
		public override Type ReflectedType {
			get {
				return reftype;
			}
		}
		public override Type DeclaringType {
			get {
				return MonoMethodInfo.GetDeclaringType (mhandle);
			}
		}
		public override string Name {
			get {
				if (name != null)
					return name;
				return MonoMethod.get_name (this);
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

		public override MethodBody GetMethodBody () {
			return GetMethodBody (mhandle);
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Void ");
			sb.Append (Name);
			sb.Append ("(");
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					sb.Append (", ");
				sb.Append (p[i].ParameterType.Name);
			}
			if (CallingConvention == CallingConventions.Any)
				sb.Append (", ...");
			sb.Append (")");
			return sb.ToString ();
		}

		// ISerializable
		public void GetObjectData(SerializationInfo info, StreamingContext context) 
		{
			MemberInfoSerializationHolder.Serialize ( info, Name, ReflectedType, ToString(), MemberTypes.Constructor);
		}

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif
	}
}
