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
	sealed class PropertySignature : Signature
	{
		private CallingConventions callingConvention;
		private readonly Type propertyType;
		private readonly Type[] optionalCustomModifiers;
		private readonly Type[] requiredCustomModifiers;
		private readonly Type[] parameterTypes;
		private readonly Type[][] parameterOptionalCustomModifiers;
		private readonly Type[][] parameterRequiredCustomModifiers;

		internal static PropertySignature Create(CallingConventions callingConvention, Type propertyType, Type[] optionalCustomModifiers, Type[] requiredCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeOptionalCustomModifiers, Type[][] parameterTypeRequiredCustomModifiers)
		{
			return new PropertySignature(callingConvention, propertyType, Util.Copy(optionalCustomModifiers), Util.Copy(requiredCustomModifiers), Util.Copy(parameterTypes), Util.Copy(parameterTypeOptionalCustomModifiers), Util.Copy(parameterTypeRequiredCustomModifiers));
		}

		private PropertySignature(CallingConventions callingConvention, Type propertyType, Type[] optionalCustomModifiers, Type[] requiredCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeOptionalCustomModifiers, Type[][] parameterTypeRequiredCustomModifiers)
		{
			this.callingConvention = callingConvention;
			this.propertyType = propertyType;
			this.optionalCustomModifiers = optionalCustomModifiers;
			this.requiredCustomModifiers = requiredCustomModifiers;
			this.parameterTypes = parameterTypes;
			this.parameterOptionalCustomModifiers = parameterTypeOptionalCustomModifiers;
			this.parameterRequiredCustomModifiers = parameterTypeRequiredCustomModifiers;
		}

		public override bool Equals(object obj)
		{
			PropertySignature other = obj as PropertySignature;
			return other != null
				&& other.propertyType.Equals(propertyType)
				&& Util.ArrayEquals(other.optionalCustomModifiers, optionalCustomModifiers)
				&& Util.ArrayEquals(other.requiredCustomModifiers, requiredCustomModifiers);
		}

		public override int GetHashCode()
		{
			return propertyType.GetHashCode() ^ Util.GetHashCode(optionalCustomModifiers) ^ Util.GetHashCode(requiredCustomModifiers);
		}

		internal int ParameterCount
		{
			get { return parameterTypes.Length; }
		}

		internal bool HasThis
		{
			set
			{
				if (value)
				{
					callingConvention |= CallingConventions.HasThis;
				}
				else
				{
					callingConvention &= ~CallingConventions.HasThis;
				}
			}
		}

		internal Type PropertyType
		{
			get { return propertyType; }
		}

		internal Type[] GetOptionalCustomModifiers()
		{
			return Util.Copy(optionalCustomModifiers);
		}

		internal Type[] GetRequiredCustomModifiers()
		{
			return Util.Copy(requiredCustomModifiers);
		}

		internal PropertySignature ExpandTypeParameters(Type declaringType)
		{
			return new PropertySignature(
				callingConvention,
				propertyType.BindTypeParameters(declaringType),
				BindTypeParameters(declaringType, optionalCustomModifiers),
				BindTypeParameters(declaringType, requiredCustomModifiers),
				BindTypeParameters(declaringType, parameterTypes),
				BindTypeParameters(declaringType, parameterOptionalCustomModifiers),
				BindTypeParameters(declaringType, parameterRequiredCustomModifiers));
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			byte flags = PROPERTY;
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				flags |= HASTHIS;
			}
			if ((callingConvention & CallingConventions.ExplicitThis) != 0)
			{
				flags |= EXPLICITTHIS;
			}
			if ((callingConvention & CallingConventions.VarArgs) != 0)
			{
				flags |= VARARG;
			}
			bb.Write(flags);
			bb.WriteCompressedInt(parameterTypes == null ? 0 : parameterTypes.Length);
			WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_REQD, requiredCustomModifiers);
			WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_OPT, optionalCustomModifiers);
			WriteType(module, bb, propertyType);
			if (parameterTypes != null)
			{
				for (int i = 0; i < parameterTypes.Length; i++)
				{
					if (parameterRequiredCustomModifiers != null)
					{
						WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_REQD, parameterRequiredCustomModifiers[i]);
					}
					if (parameterOptionalCustomModifiers != null)
					{
						WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_OPT, parameterOptionalCustomModifiers[i]);
					}
					WriteType(module, bb, parameterTypes[i]);
				}
			}
		}

		internal Type GetParameter(int parameter)
		{
			return parameterTypes[parameter];
		}

		internal Type[] GetOptionalCustomModifiers(int parameter)
		{
			return parameterOptionalCustomModifiers == null ? Type.EmptyTypes : parameterOptionalCustomModifiers[parameter];
		}

		internal Type[] GetRequiredCustomModifiers(int parameter)
		{
			return parameterRequiredCustomModifiers == null ? Type.EmptyTypes : parameterRequiredCustomModifiers[parameter];
		}

		internal static PropertySignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			byte flags = br.ReadByte();
			if ((flags & PROPERTY) == 0)
			{
				throw new BadImageFormatException();
			}
			CallingConventions callingConvention = CallingConventions.Standard;
			if ((flags & HASTHIS) != 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			if ((flags & EXPLICITTHIS) != 0)
			{
				callingConvention |= CallingConventions.ExplicitThis;
			}
			Type returnType;
			Type[] returnTypeRequiredCustomModifiers;
			Type[] returnTypeOptionalCustomModifiers;
			Type[] parameterTypes;
			Type[][] parameterRequiredCustomModifiers;
			Type[][] parameterOptionalCustomModifiers;
			int paramCount = br.ReadCompressedInt();
			ReadCustomModifiers(module, br, context, out returnTypeRequiredCustomModifiers, out returnTypeOptionalCustomModifiers);
			returnType = ReadRetType(module, br, context);
			parameterTypes = new Type[paramCount];
			parameterRequiredCustomModifiers = null;
			parameterOptionalCustomModifiers = null;
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if (IsCustomModifier(br.PeekByte()))
				{
					if (parameterOptionalCustomModifiers == null)
					{
						parameterOptionalCustomModifiers = new Type[parameterTypes.Length][];
						parameterRequiredCustomModifiers = new Type[parameterTypes.Length][];
					}
					ReadCustomModifiers(module, br, context, out parameterRequiredCustomModifiers[i], out parameterOptionalCustomModifiers[i]);
				}
				parameterTypes[i] = ReadParam(module, br, context);
			}
			return new PropertySignature(callingConvention, returnType, returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers,
				parameterTypes, parameterOptionalCustomModifiers, parameterRequiredCustomModifiers);
		}
	}
}
