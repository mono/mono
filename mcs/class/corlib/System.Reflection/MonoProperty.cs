//
// System.Reflection/MonoProperty.cs
// The class used to represent Properties from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Patrik Torstensson (patrik.torstensson@labs2.com)
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
using System.Security;

namespace System.Reflection {
	
	internal struct MonoPropertyInfo {
		public Type parent;
		public Type declaring_type;
		public String name;
		public MethodInfo get_method;
		public MethodInfo set_method;
		public PropertyAttributes attrs;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_property_info (MonoProperty prop, ref MonoPropertyInfo info,
							       PInfo req_info);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern Type[] GetTypeModifiers (MonoProperty prop, bool optional);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object get_default_value (MonoProperty prop);
	}

	[Flags]
	internal enum PInfo {
		Attributes = 1,
		GetMethod  = 1 << 1,
		SetMethod  = 1 << 2,
		ReflectedType = 1 << 3,
		DeclaringType = 1 << 4,
		Name = 1 << 5
		
	}

	internal delegate object GetterAdapter (object _this);
	internal delegate R Getter<T,R> (T _this);

	[Serializable]
	internal class MonoProperty : PropertyInfo, ISerializable {
#pragma warning disable 649
		internal IntPtr klass;
		internal IntPtr prop;
		MonoPropertyInfo info;
		PInfo cached;
		GetterAdapter cached_getter;

#pragma warning restore 649

		void CachePropertyInfo (PInfo flags)
		{
			if ((cached & flags) != flags) {
				MonoPropertyInfo.get_property_info (this, ref info, flags);
				cached |= flags;
			}
		}
		
		public override PropertyAttributes Attributes {
			get {
				CachePropertyInfo (PInfo.Attributes);
				return info.attrs;
			}
		}
		
		public override bool CanRead {
			get {
				CachePropertyInfo (PInfo.GetMethod);
				return (info.get_method != null);
			}
		}
		
		public override bool CanWrite {
			get {
				CachePropertyInfo (PInfo.SetMethod);
				return (info.set_method != null);
			}
		}

		public override Type PropertyType {
			get {
				CachePropertyInfo (PInfo.GetMethod | PInfo.SetMethod);

				if (info.get_method != null) {
					return info.get_method.ReturnType;
				} else {
					ParameterInfo[] parameters = info.set_method.GetParameters ();
					
					return parameters [parameters.Length - 1].ParameterType;
				}
			}
		}

		public override Type ReflectedType {
			get {
				CachePropertyInfo (PInfo.ReflectedType);
				return info.parent;
			}
		}
		
		public override Type DeclaringType {
			get {
				CachePropertyInfo (PInfo.DeclaringType);
				return info.declaring_type;
			}
		}
		
		public override string Name {
			get {
				CachePropertyInfo (PInfo.Name);
				return info.name;
			}
		}

		public override MethodInfo[] GetAccessors (bool nonPublic)
		{
			int nget = 0;
			int nset = 0;
			
			CachePropertyInfo (PInfo.GetMethod | PInfo.SetMethod);

			if (info.set_method != null && (nonPublic || info.set_method.IsPublic))
				nset = 1;
			if (info.get_method != null && (nonPublic || info.get_method.IsPublic))
				nget = 1;

			MethodInfo[] res = new MethodInfo [nget + nset];
			int n = 0;
			if (nset != 0)
				res [n++] = info.set_method;
			if (nget != 0)
				res [n++] = info.get_method;
			return res;
		}

		public override MethodInfo GetGetMethod (bool nonPublic)
		{
			CachePropertyInfo (PInfo.GetMethod);
			if (info.get_method != null && (nonPublic || info.get_method.IsPublic))
				return info.get_method;
			else
				return null;
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			CachePropertyInfo (PInfo.GetMethod | PInfo.SetMethod);
			ParameterInfo[] res;
			if (info.get_method != null) {
				res = info.get_method.GetParameters ();
			} else if (info.set_method != null) {
				ParameterInfo[] src = info.set_method.GetParameters ();
				res = new ParameterInfo [src.Length - 1];
				Array.Copy (src, res, res.Length);
			} else
				return new ParameterInfo [0];

			for (int i = 0; i < res.Length; ++i) {
				ParameterInfo pinfo = res [i];
				res [i] = new ParameterInfo (pinfo, this);
			}
			return res;	
		}
		
		public override MethodInfo GetSetMethod (bool nonPublic)
		{
			CachePropertyInfo (PInfo.SetMethod);
			if (info.set_method != null && (nonPublic || info.set_method.IsPublic))
				return info.set_method;
			else
				return null;
		}


		/*TODO verify for attribute based default values, just like ParameterInfo*/
		public override object GetConstantValue ()
		{
			return MonoPropertyInfo.get_default_value (this);
		}

		public override object GetRawConstantValue() {
			return MonoPropertyInfo.get_default_value (this);
		}

		// According to MSDN the inherit parameter is ignored here and
		// the behavior always defaults to inherit = false
		//
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, false);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, false);
		}
		
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, false);
		}


		delegate object GetterAdapter (object _this);
		delegate R Getter<T,R> (T _this);
		delegate R StaticGetter<R> ();

#pragma warning disable 169
		// Used via reflection
		static object GetterAdapterFrame<T,R> (Getter<T,R> getter, object obj)
		{
			return getter ((T)obj);
		}

		static object StaticGetterAdapterFrame<R> (StaticGetter<R> getter, object obj)
		{
			return getter ();
		}
#pragma warning restore 169

		/*
		 * The idea behing this optimization is to use a pair of delegates to simulate the same effect of doing a reflection call.
		 * The first delegate cast the this argument to the right type and the second does points to the target method.
		 */
		static GetterAdapter CreateGetterDelegate (MethodInfo method)
		{
			Type[] typeVector;
			Type getterType;
			object getterDelegate;
			MethodInfo adapterFrame;
			Type getterDelegateType;
			string frameName;

			if (method.IsStatic) {
				typeVector = new Type[] { method.ReturnType };
				getterDelegateType = typeof (StaticGetter<>);
				frameName = "StaticGetterAdapterFrame";
			} else {
				typeVector = new Type[] { method.DeclaringType, method.ReturnType };
				getterDelegateType = typeof (Getter<,>);
				frameName = "GetterAdapterFrame";
			}

			getterType = getterDelegateType.MakeGenericType (typeVector);
#if NET_2_1
			// with Silverlight a coreclr failure (e.g. Transparent caller creating a delegate on a Critical method)
			// would normally throw an ArgumentException, so we set throwOnBindFailure to false and check for a null
			// delegate that we can transform into a MethodAccessException
			getterDelegate = Delegate.CreateDelegate (getterType, method, false);
			if (getterDelegate == null)
				throw new MethodAccessException ();
#else
			getterDelegate = Delegate.CreateDelegate (getterType, method);
#endif
			adapterFrame = typeof (MonoProperty).GetMethod (frameName, BindingFlags.Static | BindingFlags.NonPublic);
			adapterFrame = adapterFrame.MakeGenericMethod (typeVector);
			return (GetterAdapter)Delegate.CreateDelegate (typeof (GetterAdapter), getterDelegate, adapterFrame, true);
		}
			
		public override object GetValue (object obj, object[] index)
		{
			if (index == null || index.Length == 0) {
				/*FIXME we should check if the number of arguments matches the expected one, otherwise the error message will be pretty criptic.*/
#if !MONOTOUCH
				if (cached_getter == null) {
					if (!DeclaringType.IsValueType) { //FIXME find a way to build an invoke delegate for value types.
						MethodInfo method = GetGetMethod (true);
						if (method == null)
							throw new ArgumentException ("Get Method not found for '" + Name + "'");
						cached_getter = CreateGetterDelegate (method);
						return cached_getter (obj);
					}
				} else {
					return cached_getter (obj);
				}
#endif
			}

			return GetValue (obj, BindingFlags.Default, null, index, null);
		}

		public override object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			object ret = null;

			MethodInfo method = GetGetMethod (true);
			if (method == null)
				throw new ArgumentException ("Get Method not found for '" + Name + "'");

			try {
				if (index == null || index.Length == 0) 
					ret = method.Invoke (obj, invokeAttr, binder, null, culture);
				else
					ret = method.Invoke (obj, invokeAttr, binder, index, culture);
			}
			catch (SecurityException se) {
				throw new TargetInvocationException (se);
			}

			return ret;
		}

		public override void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			MethodInfo method = GetSetMethod (true);
			if (method == null)
				throw new ArgumentException ("Set Method not found for '" + Name + "'");
			
			object [] parms;
			if (index == null || index.Length == 0) 
				parms = new object [] {value};
			else {
				int ilen = index.Length;
				parms = new object [ilen+ 1];
				index.CopyTo (parms, 0);
				parms [ilen] = value;
			}

			method.Invoke (obj, invokeAttr, binder, parms, culture);
		}

		public override string ToString () {
			return PropertyType.ToString () + " " + Name;
		}

		public override Type[] GetOptionalCustomModifiers () {
			Type[] types = MonoPropertyInfo.GetTypeModifiers (this, true);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public override Type[] GetRequiredCustomModifiers () {
			Type[] types = MonoPropertyInfo.GetTypeModifiers (this, false);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		// ISerializable
		public void GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			MemberInfoSerializationHolder.Serialize (info, Name, ReflectedType,
				ToString(), MemberTypes.Property);
		}

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif
	}
}
