//
// System.Reflection/PropertyInfo.cs
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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_PropertyInfo))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class PropertyInfo : MemberInfo, _PropertyInfo {

		public abstract PropertyAttributes Attributes { get; }
		public abstract bool CanRead { get; }
		public abstract bool CanWrite { get; }

		public bool IsSpecialName {
			get {return (Attributes & PropertyAttributes.SpecialName) != 0;}
		}

		public override MemberTypes MemberType {
			get {return MemberTypes.Property;}
		}
		public abstract Type PropertyType { get; }
	
		protected PropertyInfo () { }

		public MethodInfo[] GetAccessors ()
		{
			return GetAccessors (false);
		}
		
		public abstract MethodInfo[] GetAccessors (bool nonPublic);

		public MethodInfo GetGetMethod()
		{
			return GetGetMethod (false);
		}
		public abstract MethodInfo GetGetMethod(bool nonPublic);
		
		public abstract ParameterInfo[] GetIndexParameters();

		public MethodInfo GetSetMethod()
		{
			return GetSetMethod (false);
		}
		
		public abstract MethodInfo GetSetMethod (bool nonPublic);
		
		[DebuggerHidden]
		[DebuggerStepThrough]
		public virtual object GetValue (object obj, object[] index)
		{
			return GetValue(obj, BindingFlags.Default, null, index, null);
		}
		
		public abstract object GetValue (object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);
		
		[DebuggerHidden]
		[DebuggerStepThrough]
		public virtual void SetValue (object obj, object value, object[] index)
		{
			SetValue (obj, value, BindingFlags.Default, null, index, null);
		}
		
		public abstract void SetValue (object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture);

		public virtual Type[] GetOptionalCustomModifiers () {
			return Type.EmptyTypes;
		}

		public virtual Type[] GetRequiredCustomModifiers () {
			return Type.EmptyTypes;
		}

		static NotImplementedException CreateNIE ()
		{
			return new NotImplementedException ();
		}

		public virtual object GetConstantValue () {
			throw CreateNIE ();
		}

		public virtual object GetRawConstantValue() {
			throw CreateNIE ();
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

		public static bool operator == (PropertyInfo left, PropertyInfo right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (PropertyInfo left, PropertyInfo right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}
#endif

		void _PropertyInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _PropertyInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _PropertyInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _PropertyInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
