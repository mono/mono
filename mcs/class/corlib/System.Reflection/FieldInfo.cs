//
// System.Reflection.FieldInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO: Mucho left to implement.
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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class FieldInfo : MemberInfo {
		public abstract FieldAttributes Attributes {get;}
		public abstract RuntimeFieldHandle FieldHandle {get;}

		protected FieldInfo () {}
		
		public abstract Type FieldType { get; }

		public abstract object GetValue(object obj);

		public override MemberTypes MemberType {
			get { return MemberTypes.Field;}
		}

		public bool IsLiteral
		{
			get {return (Attributes & FieldAttributes.Literal) != 0;}
		} 

		public bool IsStatic
		{
			get {return (Attributes & FieldAttributes.Static) != 0;}
		} 

		public bool IsInitOnly
		{
			get {return (Attributes & FieldAttributes.InitOnly) != 0;}
		} 
		public Boolean IsPublic
		{ 
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public;
			}
		}
		public Boolean IsPrivate
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private;
			}
		}
		public Boolean IsFamily
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family;
			}
		}
		public Boolean IsAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly;
			}
		}
		public Boolean IsFamilyAndAssembly
		{
			get {
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem;
			}
		}
		public Boolean IsFamilyOrAssembly
		{
			get
			{
				return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem;
			}
		}
		public Boolean IsPinvokeImpl
		{
			get
			{
				return (Attributes & FieldAttributes.PinvokeImpl) == FieldAttributes.PinvokeImpl;
			}
		}
		public Boolean IsSpecialName
		{
			get
			{
				return (Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName;
			}
		}
		public Boolean IsNotSerialized
		{
			get
			{
				return (Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized;
			}
		}

		public abstract void SetValue (object obj, object val, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		[DebuggerHidden]
		[DebuggerStepThrough]
		public void SetValue (object obj, object value)
		{
			SetValue (obj, value, 0, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern FieldInfo internal_from_handle (IntPtr handle);

		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle)
		{
			return internal_from_handle (handle.Value);
		}

		internal abstract int GetFieldOffset ();

		[CLSCompliant(false)]
		[MonoTODO]
		public virtual object GetValueDirect (TypedReference obj)
		{
			throw new NotImplementedException ();
		}

		[CLSCompliant(false)]
		[MonoTODO]
		public virtual void SetValueDirect (TypedReference obj, object value)
		{
			throw new NotImplementedException ();
		}

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			/* FIXME: Add support for MarshalAsAttribute */

			if (IsNotSerialized)
				count ++;

			if (DeclaringType.IsExplicitLayout)
				count ++;

			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if (IsNotSerialized)
				attrs [count ++] = new NonSerializedAttribute ();
			if (DeclaringType.IsExplicitLayout)
				attrs [count ++] = new FieldOffsetAttribute (GetFieldOffset ());

			return attrs;
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public virtual Type[] OptionalCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual Type[] RequiredCustomModifiers {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public abstract FieldInfo Mono_GetGenericFieldDefinition ();
#endif
	}
}
