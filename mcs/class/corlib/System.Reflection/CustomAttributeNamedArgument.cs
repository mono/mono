//
// System.Reflection/CustomAttributeNamedArgument.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//   Carlos Alberto Cortez (calberto.cortez@gmail.com)
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
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	[Serializable]
	public struct CustomAttributeNamedArgument {
		CustomAttributeTypedArgument typedArgument;
		MemberInfo memberInfo;
		
#if NET_4_0
		public
#endif
		CustomAttributeNamedArgument (MemberInfo memberInfo, object value)
		{
			this.memberInfo = memberInfo;
			this.typedArgument = (CustomAttributeTypedArgument) value;
		}
		
#if NET_4_0
		public CustomAttributeNamedArgument (MemberInfo memberInfo, CustomAttributeTypedArgument typedArgument)
		{
			this.memberInfo = memberInfo;
			this.typedArgument = typedArgument;
		}
#endif

		public MemberInfo MemberInfo {
			get {
				return memberInfo;
			}
		}

		public CustomAttributeTypedArgument TypedValue {
			get {
				return typedArgument;
			}
		}

		public override string ToString ()
		{
			return memberInfo.Name + " = " + typedArgument.ToString ();
		}

		public override bool Equals (object obj)
		{
			if (!(obj is CustomAttributeNamedArgument))
				return false;
			CustomAttributeNamedArgument other = (CustomAttributeNamedArgument) obj;
			return  other.memberInfo == memberInfo &&
				typedArgument.Equals (other.typedArgument);
		}

		public override int GetHashCode ()
		{
			return (memberInfo.GetHashCode () << 16) + typedArgument.GetHashCode ();
		}

		public static bool operator == (CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
		{
			return left.Equals (right);
		}

		public static bool operator != (CustomAttributeNamedArgument left, CustomAttributeNamedArgument right)
		{
			return !left.Equals (right);
		}
	}

}

