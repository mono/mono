/*
  Copyright (C) 2009 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace IKVM.Reflection
{
	public struct CustomAttributeNamedArgument
	{
		private readonly MemberInfo member;
		private readonly CustomAttributeTypedArgument value;

		internal CustomAttributeNamedArgument(MemberInfo member, CustomAttributeTypedArgument value)
		{
			this.member = member;
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			return this == obj as CustomAttributeNamedArgument?;
		}

		public override int GetHashCode()
		{
			return member.GetHashCode() ^ 53 * value.GetHashCode();
		}

		public MemberInfo MemberInfo
		{
			get { return member; }
		}

		public CustomAttributeTypedArgument TypedValue
		{
			get { return value; }
		}

		public bool IsField
		{
			get { return member.MemberType == MemberTypes.Field; }
		}

		public string MemberName
		{
			get { return member.Name; }
		}

		public static bool operator ==(CustomAttributeNamedArgument arg1, CustomAttributeNamedArgument arg2)
		{
			return arg1.member.Equals(arg2.member) && arg1.value == arg2.value;
		}

		public static bool operator !=(CustomAttributeNamedArgument arg1, CustomAttributeNamedArgument arg2)
		{
			return !(arg1 == arg2);
		}
	}
}
