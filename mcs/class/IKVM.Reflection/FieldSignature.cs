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
using System.IO;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Writer;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	sealed class FieldSignature : Signature
	{
		private readonly Type fieldType;
		private readonly Type[] optionalCustomModifiers;
		private readonly Type[] requiredCustomModifiers;

		internal static FieldSignature Create(Type fieldType, Type[] optionalCustomModifiers, Type[] requiredCustomModifiers)
		{
			return new FieldSignature(fieldType, Util.Copy(optionalCustomModifiers), Util.Copy(requiredCustomModifiers));
		}

		private FieldSignature(Type fieldType, Type[] optionalCustomModifiers, Type[] requiredCustomModifiers)
		{
			this.fieldType = fieldType;
			this.optionalCustomModifiers = optionalCustomModifiers;
			this.requiredCustomModifiers = requiredCustomModifiers;
		}

		public override bool Equals(object obj)
		{
			FieldSignature other = obj as FieldSignature;
			return other != null
				&& other.fieldType.Equals(fieldType)
				&& Util.ArrayEquals(other.optionalCustomModifiers, optionalCustomModifiers)
				&& Util.ArrayEquals(other.requiredCustomModifiers, requiredCustomModifiers);
		}

		public override int GetHashCode()
		{
			return fieldType.GetHashCode() ^ Util.GetHashCode(optionalCustomModifiers) ^ Util.GetHashCode(requiredCustomModifiers);
		}

		internal Type FieldType
		{
			get { return fieldType; }
		}

		internal Type[] GetOptionalCustomModifiers()
		{
			return Util.Copy(optionalCustomModifiers);
		}

		internal Type[] GetRequiredCustomModifiers()
		{
			return Util.Copy(requiredCustomModifiers);
		}

		internal FieldSignature ExpandTypeParameters(Type declaringType)
		{
			return new FieldSignature(
				fieldType.BindTypeParameters(declaringType),
				BindTypeParameters(declaringType, optionalCustomModifiers),
				BindTypeParameters(declaringType, requiredCustomModifiers));
		}

		internal static FieldSignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.ReadByte() != FIELD)
			{
				throw new BadImageFormatException();
			}
			Type fieldType;
			Type[] optionalCustomModifiers;
			Type[] requiredCustomModifiers;
			ReadCustomModifiers(module, br, context, out requiredCustomModifiers, out optionalCustomModifiers);
			fieldType = ReadType(module, br, context);
			return new FieldSignature(fieldType, optionalCustomModifiers, requiredCustomModifiers);
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			bb.Write(FIELD);
			WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_OPT, optionalCustomModifiers);
			WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_REQD, requiredCustomModifiers);
			WriteType(module, bb, fieldType);
		}
	}
}
