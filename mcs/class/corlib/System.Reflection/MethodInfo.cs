//
// System.Reflection/MethodInfo.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_MethodInfo))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class MethodInfo: MethodBase, _MethodInfo {

		public abstract MethodInfo GetBaseDefinition();

		internal virtual MethodInfo GetBaseMethod ()
		{
			return this;
		}

		protected MethodInfo() {
		}


		public override MemberTypes MemberType { get {return MemberTypes.Method;} }

		public virtual Type ReturnType {
			get { return null; }
		}

		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; }

		void _MethodInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _MethodInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}

		[ComVisible (true)]
		public virtual MethodInfo GetGenericMethodDefinition ()
		{
			throw new NotSupportedException ();
		}

		public virtual MethodInfo MakeGenericMethod (params Type [] typeArguments)
		{
			throw new NotSupportedException (this.GetType().ToString ());
		}

		// GetGenericArguments, IsGenericMethod, IsGenericMethodDefinition
		// and ContainsGenericParameters are implemented in the derived classes.
		[ComVisible (true)]
		public override Type [] GetGenericArguments () {
			return Type.EmptyTypes;
		}

#if !NET_4_0 && !MOONLIGHT
		public override bool IsGenericMethod {
			get {
				return false;
			}
		}

		public override bool IsGenericMethodDefinition {
			get {
				return false;
			}
		}

		public override bool ContainsGenericParameters {
			get {
				return false;
			}
		}
#endif

		public virtual ParameterInfo ReturnParameter {
			get {
				throw new NotSupportedException ();
			}
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

		public static bool operator == (MethodInfo left, MethodInfo right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (MethodInfo left, MethodInfo right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}
#endif

	}
}
