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

namespace IKVM.Reflection
{
	public abstract class FieldInfo : MemberInfo
	{
		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Field; }
		}

		public abstract FieldAttributes Attributes { get; }
		public abstract void __GetDataFromRVA(byte[] data, int offset, int length);
		public abstract Object GetRawConstantValue();
		internal abstract FieldSignature FieldSignature { get; }

		public Type FieldType
		{
			get { return this.FieldSignature.FieldType; }
		}

		public Type[] GetOptionalCustomModifiers()
		{
			return this.FieldSignature.GetOptionalCustomModifiers();
		}

		public Type[] GetRequiredCustomModifiers()
		{
			return this.FieldSignature.GetRequiredCustomModifiers();
		}

		public bool IsStatic
		{
			get { return (Attributes & FieldAttributes.Static) != 0; }
		}

		public bool IsLiteral
		{
			get { return (Attributes & FieldAttributes.Literal) != 0; }
		}

		public bool IsInitOnly
		{
			get { return (Attributes & FieldAttributes.InitOnly) != 0; }
		}

		public bool IsNotSerialized
		{
			get { return (Attributes & FieldAttributes.NotSerialized) != 0; }
		}

		public bool IsSpecialName
		{
			get { return (Attributes & FieldAttributes.SpecialName) != 0; }
		}

		public bool IsPublic
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public; }
		}

		public bool IsPrivate
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private; }
		}

		public bool IsFamily
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family; }
		}

		public bool IsFamilyOrAssembly
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem; }
		}

		public bool IsAssembly
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family; }
		}

		public bool IsFamilyAndAssembly
		{
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem; }
		}

		public bool IsPinvokeImpl
		{
			get { return (Attributes & FieldAttributes.PinvokeImpl) != 0; }
		}

		internal abstract int ImportTo(Emit.ModuleBuilder module);

		internal virtual FieldInfo BindTypeParameters(Type type)
		{
			return new GenericFieldInstance(this.DeclaringType.BindTypeParameters(type), this);
		}
	}
}
