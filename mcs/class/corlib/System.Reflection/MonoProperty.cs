//
// System.Reflection/MonoProperty.cs
// The class used to represent Properties from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Patrik Torstensson (patrik.torstensson@labs2.com)
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

namespace System.Reflection {
	
	internal struct MonoPropertyInfo {
		public Type parent;
		public String name;
		public MethodInfo get_method;
		public MethodInfo set_method;
		public PropertyAttributes attrs;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_property_info (MonoProperty prop, out MonoPropertyInfo info,
							       PInfo req_info);
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
	internal class MonoProperty : PropertyInfo {
		internal IntPtr klass;
		internal IntPtr prop;
		
		public override PropertyAttributes Attributes {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.Attributes);
				return info.attrs;
			}
		}
		
		public override bool CanRead {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.GetMethod);
				return (info.get_method != null);
			}
		}
		
		public override bool CanWrite {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.SetMethod);
				return (info.set_method != null);
			}
		}

		public override Type PropertyType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.GetMethod | PInfo.SetMethod);
				
				if (info.get_method != null) {
					return info.get_method.ReturnType;
				} else {
					ParameterInfo[] parameters = info.set_method.GetParameters();
					
					return parameters [parameters.Length - 1].ParameterType;
				}
			}
		}

		public override Type ReflectedType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.ReflectedType);
				return info.parent;
			}
		}
		
		public override Type DeclaringType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.DeclaringType);
				return info.parent;
			}
		}
		
		public override string Name {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info, PInfo.Name);
				return info.name;
			}
		}

		public override MethodInfo[] GetAccessors (bool nonPublic)
		{
			MonoPropertyInfo info;
			int nget = 0;
			int nset = 0;
			
			MonoPropertyInfo.get_property_info (this, out info, PInfo.GetMethod | PInfo.SetMethod);
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
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info, PInfo.GetMethod);
			if (info.get_method != null && (nonPublic || info.get_method.IsPublic))
				return info.get_method;
			else
				return null;
		}

		public override ParameterInfo[] GetIndexParameters()
		{
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info, PInfo.GetMethod);
			if (info.get_method != null)
				return info.get_method.GetParameters ();
			return new ParameterInfo [0];
		}
		
		public override MethodInfo GetSetMethod (bool nonPublic)
		{
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info, PInfo.SetMethod);
			if (info.set_method != null && (nonPublic || info.set_method.IsPublic))
				return info.set_method;
			else
				return null;
		}
		
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
		{
			object ret = null;

			MethodInfo method = GetGetMethod (true);
			if (method == null)
				throw new ArgumentException ("Get Method not found for '" + Name + "'");
			
			if (index == null || index.Length == 0) 
				ret = method.Invoke (obj, invokeAttr, binder, null, culture);
			else
				ret = method.Invoke (obj, invokeAttr, binder, index, culture);

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

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MonoTODO]
		public override Type[] OptionalCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Type[] RequiredCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}
#endif
	}
}

