//
// System.Reflection/ConstructorInfo.cs
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

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ConstructorInfo))]	
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public abstract class ConstructorInfo : MethodBase, _ConstructorInfo {

		[ComVisible (true)]
		public static readonly string ConstructorName = ".ctor";
		[ComVisible (true)]
		public static readonly string TypeConstructorName = ".cctor";

		protected ConstructorInfo() {
		}
		
		[ComVisible (true)]
		public override MemberTypes MemberType {
			get {return MemberTypes.Constructor;}
		}

		[DebuggerStepThrough]
		[DebuggerHidden]
		public object Invoke (object[] parameters)
		{
			if (parameters == null)
				parameters = new object [0];

			return Invoke (BindingFlags.CreateInstance, null, parameters, null);
		}

		public abstract object Invoke (BindingFlags invokeAttr, Binder binder, object[] parameters,
					       CultureInfo culture);

		void _ConstructorInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ConstructorInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}

		object _ConstructorInfo.Invoke_2 (object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return this.Invoke (obj, invokeAttr, binder, parameters, culture);
		}

		object _ConstructorInfo.Invoke_3 (object obj, object[] parameters)
		{
			return base.Invoke (obj, parameters);
		}

		object _ConstructorInfo.Invoke_4 (BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
		{
			return this.Invoke (invokeAttr, binder, parameters, culture);
		}

		object _ConstructorInfo.Invoke_5 (object[] parameters)
		{
			return this.Invoke (parameters);
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

		public static bool operator == (ConstructorInfo left, ConstructorInfo right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (ConstructorInfo left, ConstructorInfo right)
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
