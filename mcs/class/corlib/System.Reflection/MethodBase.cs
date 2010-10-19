//
// System.Reflection/MethodBase.cs
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

using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_MethodBase))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class MethodBase: MemberInfo, _MethodBase {

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static MethodBase GetCurrentMethod ();

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle)
		{
			return GetMethodFromIntPtr (handle.Value, IntPtr.Zero);
		}

		static MethodBase GetMethodFromIntPtr (IntPtr handle, IntPtr declaringType)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			MethodBase res = GetMethodFromHandleInternalType (handle, declaringType);
			if (res == null)
				throw new ArgumentException ("The handle is invalid.");			
			return res;
		}

		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle)
		{
			MethodBase res = GetMethodFromIntPtr (handle.Value, IntPtr.Zero);
			Type t = res.DeclaringType;
			if (t.IsGenericType || t.IsGenericTypeDefinition)
				throw new ArgumentException ("Cannot resolve method because it's declared in a generic class.");
			return res;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static MethodBase GetMethodFromHandleInternalType (IntPtr method_handle, IntPtr type_handle);

		[ComVisible (false)]
		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
		{
			return GetMethodFromIntPtr (handle.Value, declaringType.Value);
		}

		public abstract MethodImplAttributes GetMethodImplementationFlags();

		public abstract ParameterInfo[] GetParameters();
		
		//
		// This is a quick version for our own use. We should override
		// it where possible so that it does not allocate an array.
		//
		internal virtual int GetParameterCount ()
		{
			throw new NotImplementedException ("must be implemented");
		}

		[DebuggerHidden]
		[DebuggerStepThrough]		
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
		[ComVisibleAttribute (true)]
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

		[ComVisible (true)]
		public virtual Type [] GetGenericArguments ()
		{
			throw new NotSupportedException ();
		}

		public virtual bool ContainsGenericParameters {
			get {
				return false;
			}
		}

		public virtual bool IsGenericMethodDefinition {
			get {
				return false;
			}
		}

		public virtual bool IsGenericMethod {
			get {
				return false;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static MethodBody GetMethodBodyInternal (IntPtr handle);

		internal static MethodBody GetMethodBody (IntPtr handle) {
			return GetMethodBodyInternal (handle);
		}

		public virtual MethodBody GetMethodBody () {
			throw new NotSupportedException ();
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

		public static bool operator == (MethodBase left, MethodBase right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (MethodBase left, MethodBase right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}
#endif

		void _MethodBase.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _MethodBase.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodBase.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodBase.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
