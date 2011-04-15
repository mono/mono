//
// System.TypedReference.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace System 
{
	[CLSCompliant (false)]
	[ComVisible (true)]
	public struct TypedReference 
	{
#pragma warning disable 169, 649
		RuntimeTypeHandle type;
		IntPtr value;
		IntPtr klass;
#pragma warning restore 169, 649

		public override bool Equals (object o)
		{
			throw new NotSupportedException (Locale.GetText ("This operation is not supported for this type."));
		}

		public override int GetHashCode ()
		{
			if (type.Value == IntPtr.Zero)
				return 0;
			return Type.GetTypeFromHandle (type).GetHashCode ();
		}

		public static Type GetTargetType (TypedReference value)
		{
			return Type.GetTypeFromHandle (value.type);
		}

		[MonoTODO]
		[CLSCompliant (false)]
		[ReflectionPermission (SecurityAction.LinkDemand, MemberAccess = true)]
		public static TypedReference MakeTypedReference (object target, FieldInfo[] flds) 
		{
			if (target == null) {
				throw new ArgumentNullException ("target");
			}
			if (flds == null) {
				throw new ArgumentNullException ("flds");
			}
			if (flds.Length == 0) {
				throw new ArgumentException (Locale.GetText ("flds has no elements"));
			}
			throw new NotImplementedException ();
		}

		/* how can we set something in value if it's passed by value? */
		[MonoTODO]
		[CLSCompliant (false)]
		public static void SetTypedReference (TypedReference target, object value) 
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}
			throw new NotImplementedException ();
		}

		public static RuntimeTypeHandle TargetTypeToken (TypedReference value)
		{
			return value.type;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		public extern static object ToObject (TypedReference value);
	}
}
