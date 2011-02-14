/*
  Copyright (C) 2009-2010 Jeroen Frijters

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
using System.Diagnostics;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection
{
	sealed class MethodSignature : Signature
	{
		private readonly Type returnType;
		private readonly Type[] parameterTypes;
		private readonly Type[][][] modifiers;	// see PackedCustomModifiers
		private readonly CallingConventions callingConvention;
		private readonly int genericParamCount;

		private MethodSignature(Type returnType, Type[] parameterTypes, Type[][][] modifiers, CallingConventions callingConvention, int genericParamCount)
		{
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
			this.modifiers = modifiers;
			this.callingConvention = callingConvention;
			this.genericParamCount = genericParamCount;
		}

		public override bool Equals(object obj)
		{
			MethodSignature other = obj as MethodSignature;
			return other != null
				&& other.callingConvention == callingConvention
				&& other.genericParamCount == genericParamCount
				&& other.returnType.Equals(returnType)
				&& Util.ArrayEquals(other.parameterTypes, parameterTypes)
				&& Util.ArrayEquals(other.modifiers, modifiers);
		}

		public override int GetHashCode()
		{
			return genericParamCount ^ 77 * (int)callingConvention
				^ 3 * returnType.GetHashCode()
				^ Util.GetHashCode(parameterTypes) * 5
				^ Util.GetHashCode(modifiers) * 55;
		}

		private sealed class UnboundGenericMethodContext : IGenericContext
		{
			private readonly IGenericContext original;

			internal UnboundGenericMethodContext(IGenericContext original)
			{
				this.original = original;
			}

			public Type GetGenericTypeArgument(int index)
			{
				return original.GetGenericTypeArgument(index);
			}

			public Type GetGenericMethodArgument(int index)
			{
				return UnboundGenericMethodParameter.Make(index);
			}
		}

		internal static MethodSignature ReadSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			CallingConventions callingConvention;
			int genericParamCount;
			Type returnType;
			Type[] parameterTypes;
			byte flags = br.ReadByte();
			switch (flags & 7)
			{
				case DEFAULT:
					callingConvention = CallingConventions.Standard;
					break;
				case VARARG:
					callingConvention = CallingConventions.VarArgs;
					break;
				default:
					throw new BadImageFormatException();
			}
			if ((flags & HASTHIS) != 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			if ((flags & EXPLICITTHIS) != 0)
			{
				callingConvention |= CallingConventions.ExplicitThis;
			}
			genericParamCount = 0;
			if ((flags & GENERIC) != 0)
			{
				genericParamCount = br.ReadCompressedInt();
				context = new UnboundGenericMethodContext(context);
			}
			int paramCount = br.ReadCompressedInt();
			Type[][][] modifiers = null;
			Type[] optionalCustomModifiers;
			Type[] requiredCustomModifiers;
			ReadCustomModifiers(module, br, context, out requiredCustomModifiers, out optionalCustomModifiers);
			returnType = ReadRetType(module, br, context);
			parameterTypes = new Type[paramCount];
			PackedCustomModifiers.SetModifiers(ref modifiers, 0, 0, optionalCustomModifiers, paramCount + 1);
			PackedCustomModifiers.SetModifiers(ref modifiers, 0, 1, requiredCustomModifiers, paramCount + 1);
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if ((callingConvention & CallingConventions.VarArgs) != 0 && br.PeekByte() == SENTINEL)
				{
					Array.Resize(ref parameterTypes, i);
					if (modifiers != null)
					{
						Array.Resize(ref modifiers, i + 1);
					}
					break;
				}
				ReadCustomModifiers(module, br, context, out requiredCustomModifiers, out optionalCustomModifiers);
				PackedCustomModifiers.SetModifiers(ref modifiers, i + 1, 0, optionalCustomModifiers, paramCount + 1);
				PackedCustomModifiers.SetModifiers(ref modifiers, i + 1, 1, requiredCustomModifiers, paramCount + 1);
				parameterTypes[i] = ReadParam(module, br, context);
			}
			return new MethodSignature(returnType, parameterTypes, modifiers, callingConvention, genericParamCount);
		}

		internal static __StandAloneMethodSig ReadStandAloneMethodSig(ModuleReader module, ByteReader br, IGenericContext context)
		{
			CallingConventions callingConvention = 0;
			System.Runtime.InteropServices.CallingConvention unmanagedCallingConvention = 0;
			bool unmanaged;
			byte flags = br.ReadByte();
			switch (flags & 7)
			{
				case DEFAULT:
					callingConvention = CallingConventions.Standard;
					unmanaged = false;
					break;
				case 0x01:	// C
					unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
					unmanaged = true;
					break;
				case 0x02:	// STDCALL
					unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall;
					unmanaged = true;
					break;
				case 0x03:	// THISCALL
					unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.ThisCall;
					unmanaged = true;
					break;
				case 0x04:	// FASTCALL
					unmanagedCallingConvention = System.Runtime.InteropServices.CallingConvention.FastCall;
					unmanaged = true;
					break;
				case VARARG:
					callingConvention = CallingConventions.VarArgs;
					unmanaged = false;
					break;
				default:
					throw new BadImageFormatException();
			}
			if ((flags & HASTHIS) != 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			if ((flags & EXPLICITTHIS) != 0)
			{
				callingConvention |= CallingConventions.ExplicitThis;
			}
			if ((flags & GENERIC) != 0)
			{
				throw new BadImageFormatException();
			}
			int paramCount = br.ReadCompressedInt();
			SkipCustomModifiers(br);
			Type returnType = ReadRetType(module, br, context);
			List<Type> parameterTypes = new List<Type>();
			List<Type> optionalParameterTypes = new List<Type>();
			List<Type> curr = parameterTypes;
			for (int i = 0; i < paramCount; i++)
			{
				if (br.PeekByte() == SENTINEL)
				{
					br.ReadByte();
					curr = optionalParameterTypes;
				}
				SkipCustomModifiers(br);
				curr.Add(ReadParam(module, br, context));
			}
			return new __StandAloneMethodSig(unmanaged, unmanagedCallingConvention, callingConvention, returnType, parameterTypes.ToArray(), optionalParameterTypes.ToArray());
		}

		internal int GetParameterCount()
		{
			return parameterTypes.Length;
		}

		internal Type GetParameterType(int index)
		{
			return parameterTypes[index];
		}

		internal Type GetReturnType(IGenericBinder binder)
		{
			return returnType.BindTypeParameters(binder);
		}

		internal Type[] GetReturnTypeOptionalCustomModifiers(IGenericBinder binder)
		{
			return BindTypeParameters(binder, modifiers, 0, 0);
		}

		internal Type[] GetReturnTypeRequiredCustomModifiers(IGenericBinder binder)
		{
			return BindTypeParameters(binder, modifiers, 0, 1);
		}

		internal Type GetParameterType(IGenericBinder binder, int index)
		{
			return parameterTypes[index].BindTypeParameters(binder);
		}

		internal Type[] GetParameterOptionalCustomModifiers(IGenericBinder binder, int index)
		{
			return BindTypeParameters(binder, modifiers, index + 1, 0);
		}

		internal Type[] GetParameterRequiredCustomModifiers(IGenericBinder binder, int index)
		{
			return BindTypeParameters(binder, modifiers, index + 1, 1);
		}

		internal CallingConventions CallingConvention
		{
			get { return callingConvention; }
		}

		internal int GenericParameterCount
		{
			get { return genericParamCount; }
		}

		private sealed class Binder : IGenericBinder
		{
			private readonly Type declaringType;
			private readonly Type[] methodArgs;

			internal Binder(Type declaringType, Type[] methodArgs)
			{
				this.declaringType = declaringType;
				this.methodArgs = methodArgs;
			}

			public Type BindTypeParameter(Type type)
			{
				return declaringType.GetGenericTypeArgument(type.GenericParameterPosition);
			}

			public Type BindMethodParameter(Type type)
			{
				if (methodArgs == null)
				{
					return type;
				}
				return methodArgs[type.GenericParameterPosition];
			}
		}

		internal MethodSignature Bind(Type type, Type[] methodArgs)
		{
			Binder binder = new Binder(type, methodArgs);
			return new MethodSignature(returnType.BindTypeParameters(binder),
				BindTypeParameters(binder, parameterTypes),
				BindTypeParameters(binder, modifiers),
				callingConvention, genericParamCount);
		}

		private sealed class Unbinder : IGenericBinder
		{
			internal static readonly Unbinder Instance = new Unbinder();

			private Unbinder()
			{
			}

			public Type BindTypeParameter(Type type)
			{
				return type;
			}

			public Type BindMethodParameter(Type type)
			{
				return UnboundGenericMethodParameter.Make(type.GenericParameterPosition);
			}
		}

		internal static MethodSignature MakeFromBuilder(Type returnType, Type[] parameterTypes, Type[][][] modifiers, CallingConventions callingConvention, int genericParamCount)
		{
			if (genericParamCount > 0)
			{
				returnType = returnType.BindTypeParameters(Unbinder.Instance);
				parameterTypes = BindTypeParameters(Unbinder.Instance, parameterTypes);
				modifiers = BindTypeParameters(Unbinder.Instance, modifiers);
			}
			return new MethodSignature(returnType, parameterTypes, modifiers, callingConvention, genericParamCount);
		}

		internal bool MatchParameterTypes(Type[] types)
		{
			if (types == parameterTypes)
			{
				return true;
			}
			if (types == null)
			{
				return parameterTypes.Length == 0;
			}
			if (parameterTypes == null)
			{
				return types.Length == 0;
			}
			if (types.Length == parameterTypes.Length)
			{
				for (int i = 0; i < types.Length; i++)
				{
					if (!Util.TypeEquals(types[i], parameterTypes[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			WriteSigImpl(module, bb, parameterTypes.Length);
		}

		internal void WriteMethodRefSig(ModuleBuilder module, ByteBuffer bb, Type[] optionalParameterTypes)
		{
			WriteSigImpl(module, bb, parameterTypes.Length + optionalParameterTypes.Length);
			if (optionalParameterTypes.Length > 0)
			{
				bb.Write(SENTINEL);
				foreach (Type type in optionalParameterTypes)
				{
					WriteType(module, bb, type);
				}
			}
		}

		private void WriteSigImpl(ModuleBuilder module, ByteBuffer bb, int parameterCount)
		{
			byte first;
			if ((callingConvention & CallingConventions.Any) == CallingConventions.VarArgs)
			{
				Debug.Assert(genericParamCount == 0);
				first = VARARG;
			}
			else if (genericParamCount > 0)
			{
				first = GENERIC;
			}
			else
			{
				first = DEFAULT;
			}
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				first |= HASTHIS;
			}
			if ((callingConvention & CallingConventions.ExplicitThis) != 0)
			{
				first |= EXPLICITTHIS;
			}
			bb.Write(first);
			if (genericParamCount > 0)
			{
				bb.WriteCompressedInt(genericParamCount);
			}
			bb.WriteCompressedInt(parameterCount);
			// RetType
			if (modifiers != null && modifiers[0] != null)
			{
				WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_OPT, modifiers[0][0]);
				WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_REQD, modifiers[0][1]);
			}
			WriteType(module, bb, returnType);
			// Param
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				if (modifiers != null && modifiers[i + 1] != null)
				{
					WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_OPT, modifiers[i + 1][0]);
					WriteCustomModifiers(module, bb, ELEMENT_TYPE_CMOD_REQD, modifiers[i + 1][1]);
				}
				WriteType(module, bb, parameterTypes[i]);
			}
		}
	}

	static class PackedCustomModifiers
	{
		// modifiers are packed in a very specific way (and required to be so, otherwise equality checks will fail)
		// For modifiers[x][y][z]:
		//  x = parameter index, 0 = return type, 1 = first parameters, ...
		//  y = 0 = optional custom modifiers, 1 = required custom modifiers
		//  z = the custom modifiers
		// At any level the reference can be null (and *must* be null, if there are no modifiers below that level).
		// Empty arrays are not allowed at any level.

		// this can be used to "add" elements to the modifiers array (and the elements are assumed to already be in normalized form)
		internal static void SetModifiers(ref Type[][][] modifiers, int index, int optOrReq, Type[] add, int count)
		{
			if (add != null)
			{
				if (modifiers == null)
				{
					modifiers = new Type[count][][];
				}
				if (modifiers[index] == null)
				{
					modifiers[index] = new Type[2][];
				}
				modifiers[index][optOrReq] = add;
			}
		}

		// this method make a copy of the incoming arrays (where necessary) and returns a normalized modifiers array
		internal static Type[][][] CreateFromExternal(Type[] returnOptional, Type[] returnRequired, Type[][] parameterOptional, Type[][] parameterRequired, int parameterCount)
		{
			Type[][][] modifiers = null;
			SetModifiers(ref modifiers, 0, 0, NormalizeAndCopy(returnOptional), parameterCount + 1);
			SetModifiers(ref modifiers, 0, 1, NormalizeAndCopy(returnRequired), parameterCount + 1);
			for (int i = 0; i < parameterCount; i++)
			{
				SetModifiers(ref modifiers, i + 1, 0, NormalizeAndCopy(parameterOptional, i), parameterCount + 1);
				SetModifiers(ref modifiers, i + 1, 1, NormalizeAndCopy(parameterRequired, i), parameterCount + 1);
			}
			return modifiers;
		}

		private static Type[] NormalizeAndCopy(Type[] array)
		{
			if (array == null || array.Length == 0)
			{
				return null;
			}
			Type[] copy = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
					if (copy == null)
					{
						copy = new Type[array.Length];
					}
					copy[i] = array[i];
				}
			}
			return copy;
		}

		private static Type[] NormalizeAndCopy(Type[][] array, int index)
		{
			if (array == null || array.Length == 0)
			{
				return null;
			}
			return NormalizeAndCopy(array[index]);
		}
	}
}
