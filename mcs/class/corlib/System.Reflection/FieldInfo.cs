//
// System.Reflection.FieldInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Diagnostics;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_FieldInfo))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class FieldInfo : MemberInfo, _FieldInfo {

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

		public abstract void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture);

		[DebuggerHidden]
		[DebuggerStepThrough]
		public void SetValue (object obj, object value)
		{
			SetValue (obj, value, 0, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern FieldInfo internal_from_handle_type (IntPtr field_handle, IntPtr type_handle);

		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle)
		{
			if (handle.Value == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			return internal_from_handle_type (handle.Value, IntPtr.Zero);
		}

		[ComVisible (false)]
		public static FieldInfo GetFieldFromHandle (RuntimeFieldHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.Value == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			FieldInfo fi = internal_from_handle_type (handle.Value, declaringType.Value);
			if (fi == null)
				throw new ArgumentException ("The field handle and the type handle are incompatible.");
			return fi;
		}

		//
		// Note: making this abstract imposes an implementation requirement
		//       on any class that derives from it.  However, since it's also
		//       internal, that means only classes inside corlib can derive
		//       from FieldInfo.  See
		//
		//          errors/cs0534-4.cs errors/CS0534-4-lib.cs
		//
		//          class/Microsoft.JScript/Microsoft.JScript/JSFieldInfo.cs
		//
		internal virtual int GetFieldOffset ()
		{
			throw new SystemException ("This method should not be called");
		}

		[CLSCompliant(false)]
		[MonoTODO("Not implemented")]
		public virtual object GetValueDirect (TypedReference obj)
		{
			throw new NotImplementedException ();
		}

		[CLSCompliant(false)]
		[MonoTODO("Not implemented")]
		public virtual void SetValueDirect (TypedReference obj, object value)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern UnmanagedMarshal GetUnmanagedMarshal ();

		internal virtual UnmanagedMarshal UMarshal {
			get {
				return GetUnmanagedMarshal ();
			}
		}

		internal object[] GetPseudoCustomAttributes ()
		{
			int count = 0;

			if (IsNotSerialized)
				count ++;

			if (DeclaringType.IsExplicitLayout)
				count ++;

			UnmanagedMarshal marshalAs = UMarshal;
			if (marshalAs != null)
				count ++;

			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if (IsNotSerialized)
				attrs [count ++] = new NonSerializedAttribute ();
			if (DeclaringType.IsExplicitLayout)
				attrs [count ++] = new FieldOffsetAttribute (GetFieldOffset ());
			if (marshalAs != null)
				attrs [count ++] = marshalAs.ToMarshalAsAttribute ();

			return attrs;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern Type[] GetTypeModifiers (bool optional);

		public virtual Type[] GetOptionalCustomModifiers () {
			Type[] types = GetTypeModifiers (true);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public virtual Type[] GetRequiredCustomModifiers () {
			Type[] types = GetTypeModifiers (false);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public virtual object GetRawConstantValue ()
		{
			throw new NotSupportedException ("This non-CLS method is not implemented.");
		}


#if NET_4_0
		public override bool Equals (object obj)
		{
			return obj == (object) this;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (FieldInfo left, FieldInfo right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (FieldInfo left, FieldInfo right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}
#endif
		void _FieldInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _FieldInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _FieldInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _FieldInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
