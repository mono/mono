//
// System.Reflection.MemberInfo.cs
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
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif
namespace System.Reflection {

#if NET_2_0
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_MemberInfo))]
#endif
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	#if !DISABLE_SECURITY
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	#endif
	public abstract class MemberInfo : ICustomAttributeProvider, _MemberInfo {

		protected MemberInfo ()
		{
		}
		
		public abstract Type DeclaringType {
			get;
		}

		public abstract MemberTypes MemberType {
			get;
		}

		public abstract string Name {
			get;
		}

		public abstract Type ReflectedType {
			get;
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public virtual Module Module {
			get {
				return DeclaringType.Module;
			}
		}
#endif

#if ONLY_1_1
		public new Type GetType ()
		{
			return base.GetType ();
		}
#endif

		public abstract bool IsDefined (Type attributeType, bool inherit);

		public abstract object [] GetCustomAttributes (bool inherit);

		public abstract object [] GetCustomAttributes (Type attributeType, bool inherit);

#if NET_2_0 || BOOTSTRAP_NET_2_0
		public
#else
		internal
#endif
		virtual extern int MetadataToken {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		void _MemberInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _MemberInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MemberInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MemberInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
