/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
		private readonly PackedCustomModifiers modifiers;
		private readonly CallingConventions callingConvention;
		private readonly int genericParamCount;

		internal MethodSignature(Type returnType, Type[] parameterTypes, PackedCustomModifiers modifiers, CallingConventions callingConvention, int genericParamCount)
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
				&& other.modifiers.Equals(modifiers);
		}

		public override int GetHashCode()
		{
			return genericParamCount ^ 77 * (int)callingConvention
				^ 3 * returnType.GetHashCode()
				^ Util.GetHashCode(parameterTypes) * 5
				^ modifiers.GetHashCode() * 55;
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
				genericParamCount = br.ReadCompressedUInt();
				context = new UnboundGenericMethodContext(context);
			}
			int paramCount = br.ReadCompressedUInt();
			CustomModifiers[] modifiers = null;
			PackedCustomModifiers.Pack(ref modifiers, 0, CustomModifiers.Read(module, br, context), paramCount + 1);
			returnType = ReadRetType(module, br, context);
			parameterTypes = new Type[paramCount];
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
				PackedCustomModifiers.Pack(ref modifiers, i + 1, CustomModifiers.Read(module, br, context), paramCount + 1);
				parameterTypes[i] = ReadParam(module, br, context);
			}
			return new MethodSignature(returnType, parameterTypes, PackedCustomModifiers.Wrap(modifiers), callingConvention, genericParamCount);
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
			int paramCount = br.ReadCompressedUInt();
			CustomModifiers[] customModifiers = null;
			PackedCustomModifiers.Pack(ref customModifiers, 0, CustomModifiers.Read(module, br, context), paramCount + 1);
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
				PackedCustomModifiers.Pack(ref customModifiers, i + 1, CustomModifiers.Read(module, br, context), paramCount + 1);
				curr.Add(ReadParam(module, br, context));
			}
			return new __StandAloneMethodSig(unmanaged, unmanagedCallingConvention, callingConvention, returnType, parameterTypes.ToArray(), optionalParameterTypes.ToArray(), PackedCustomModifiers.Wrap(customModifiers));
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

		internal CustomModifiers GetReturnTypeCustomModifiers(IGenericBinder binder)
		{
			return modifiers.GetReturnTypeCustomModifiers().Bind(binder);
		}

		internal Type GetParameterType(IGenericBinder binder, int index)
		{
			return parameterTypes[index].BindTypeParameters(binder);
		}

		internal CustomModifiers GetParameterCustomModifiers(IGenericBinder binder, int index)
		{
			return modifiers.GetParameterCustomModifiers(index).Bind(binder);
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
				modifiers.Bind(binder),
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

		internal static MethodSignature MakeFromBuilder(Type returnType, Type[] parameterTypes, PackedCustomModifiers modifiers, CallingConventions callingConvention, int genericParamCount)
		{
			if (genericParamCount > 0)
			{
				returnType = returnType.BindTypeParameters(Unbinder.Instance);
				parameterTypes = BindTypeParameters(Unbinder.Instance, parameterTypes);
				modifiers = modifiers.Bind(Unbinder.Instance);
			}
			return new MethodSignature(returnType, parameterTypes, modifiers, callingConvention, genericParamCount);
		}

		internal bool MatchParameterTypes(MethodSignature other)
		{
			return Util.ArrayEquals(other.parameterTypes, parameterTypes);
		}

		internal bool MatchParameterTypes(Type[] types)
		{
			return Util.ArrayEquals(types, parameterTypes);
		}

		internal override void WriteSig(ModuleBuilder module, ByteBuffer bb)
		{
			WriteSigImpl(module, bb, parameterTypes.Length);
		}

		internal void WriteMethodRefSig(ModuleBuilder module, ByteBuffer bb, Type[] optionalParameterTypes, CustomModifiers[] customModifiers)
		{
			WriteSigImpl(module, bb, parameterTypes.Length + optionalParameterTypes.Length);
			if (optionalParameterTypes.Length > 0)
			{
				bb.Write(SENTINEL);
				for (int i = 0; i < optionalParameterTypes.Length; i++)
				{
					WriteCustomModifiers(module, bb, Util.NullSafeElementAt(customModifiers, i));
					WriteType(module, bb, optionalParameterTypes[i]);
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
				bb.WriteCompressedUInt(genericParamCount);
			}
			bb.WriteCompressedUInt(parameterCount);
			// RetType
			WriteCustomModifiers(module, bb, modifiers.GetReturnTypeCustomModifiers());
			WriteType(module, bb, returnType);
			// Param
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				WriteCustomModifiers(module, bb, modifiers.GetParameterCustomModifiers(i));
				WriteType(module, bb, parameterTypes[i]);
			}
		}
	}

	struct PackedCustomModifiers
	{
		// element 0 is the return type, the rest are the parameters
		private readonly CustomModifiers[] customModifiers;

		private PackedCustomModifiers(CustomModifiers[] customModifiers)
		{
			this.customModifiers = customModifiers;
		}

		public override int GetHashCode()
		{
			return Util.GetHashCode(customModifiers);
		}

		public override bool Equals(object obj)
		{
			PackedCustomModifiers? other = obj as PackedCustomModifiers?;
			return other != null && Equals(other.Value);
		}

		internal bool Equals(PackedCustomModifiers other)
		{
			return Util.ArrayEquals(customModifiers, other.customModifiers);
		}

		internal CustomModifiers GetReturnTypeCustomModifiers()
		{
			if (customModifiers == null)
			{
				return new CustomModifiers();
			}
			return customModifiers[0];
		}

		internal CustomModifiers GetParameterCustomModifiers(int index)
		{
			if (customModifiers == null)
			{
				return new CustomModifiers();
			}
			return customModifiers[index + 1];
		}

		internal PackedCustomModifiers Bind(IGenericBinder binder)
		{
			if (customModifiers == null)
			{
				return new PackedCustomModifiers();
			}
			CustomModifiers[] expanded = new CustomModifiers[customModifiers.Length];
			for (int i = 0; i < customModifiers.Length; i++)
			{
				expanded[i] = customModifiers[i].Bind(binder);
			}
			return new PackedCustomModifiers(expanded);
		}

		// this method make a copy of the incoming arrays (where necessary) and returns a normalized modifiers array
		internal static PackedCustomModifiers CreateFromExternal(Type[] returnOptional, Type[] returnRequired, Type[][] parameterOptional, Type[][] parameterRequired, int parameterCount)
		{
			CustomModifiers[] modifiers = null;
			Pack(ref modifiers, 0, CustomModifiers.FromReqOpt(returnRequired, returnOptional), parameterCount + 1);
			for (int i = 0; i < parameterCount; i++)
			{
				Pack(ref modifiers, i + 1, CustomModifiers.FromReqOpt(Util.NullSafeElementAt(parameterRequired, i), Util.NullSafeElementAt(parameterOptional, i)), parameterCount + 1);
			}
			return new PackedCustomModifiers(modifiers);
		}

		internal static PackedCustomModifiers CreateFromExternal(CustomModifiers returnTypeCustomModifiers, CustomModifiers[] parameterTypeCustomModifiers, int parameterCount)
		{
			CustomModifiers[] customModifiers = null;
			Pack(ref customModifiers, 0, returnTypeCustomModifiers, parameterCount + 1);
			if (parameterTypeCustomModifiers != null)
			{
				for (int i = 0; i < parameterCount; i++)
				{
					Pack(ref customModifiers, i + 1, parameterTypeCustomModifiers[i], parameterCount + 1);
				}
			}
			return new PackedCustomModifiers(customModifiers);
		}

		internal static PackedCustomModifiers Wrap(CustomModifiers[] modifiers)
		{
			return new PackedCustomModifiers(modifiers);
		}

		internal static void Pack(ref CustomModifiers[] array, int index, CustomModifiers mods, int count)
		{
			if (!mods.IsEmpty)
			{
				if (array == null)
				{
					array = new CustomModifiers[count];
				}
				array[index] = mods;
			}
		}
	}
}
