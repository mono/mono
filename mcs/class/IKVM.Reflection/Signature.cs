/*
  Copyright (C) 2009-2012 Jeroen Frijters

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
using System.IO;
using System.Text;
using CallingConvention = System.Runtime.InteropServices.CallingConvention;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Writer;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection
{
	abstract class Signature
	{
		internal const byte DEFAULT = 0x00;
		internal const byte VARARG = 0x05;
		internal const byte GENERIC = 0x10;
		internal const byte HASTHIS = 0x20;
		internal const byte EXPLICITTHIS = 0x40;
		internal const byte FIELD = 0x06;
		internal const byte LOCAL_SIG = 0x07;
		internal const byte PROPERTY = 0x08;
		internal const byte GENERICINST = 0x0A;
		internal const byte SENTINEL = 0x41;
		internal const byte ELEMENT_TYPE_VOID = 0x01;
		internal const byte ELEMENT_TYPE_BOOLEAN = 0x02;
		internal const byte ELEMENT_TYPE_CHAR = 0x03;
		internal const byte ELEMENT_TYPE_I1 = 0x04;
		internal const byte ELEMENT_TYPE_U1 = 0x05;
		internal const byte ELEMENT_TYPE_I2 = 0x06;
		internal const byte ELEMENT_TYPE_U2 = 0x07;
		internal const byte ELEMENT_TYPE_I4 = 0x08;
		internal const byte ELEMENT_TYPE_U4 = 0x09;
		internal const byte ELEMENT_TYPE_I8 = 0x0a;
		internal const byte ELEMENT_TYPE_U8 = 0x0b;
		internal const byte ELEMENT_TYPE_R4 = 0x0c;
		internal const byte ELEMENT_TYPE_R8 = 0x0d;
		internal const byte ELEMENT_TYPE_STRING = 0x0e;
		internal const byte ELEMENT_TYPE_PTR = 0x0f;
		internal const byte ELEMENT_TYPE_BYREF = 0x10;
		internal const byte ELEMENT_TYPE_VALUETYPE = 0x11;
		internal const byte ELEMENT_TYPE_CLASS = 0x12;
		internal const byte ELEMENT_TYPE_VAR = 0x13;
		internal const byte ELEMENT_TYPE_ARRAY = 0x14;
		internal const byte ELEMENT_TYPE_GENERICINST = 0x15;
		internal const byte ELEMENT_TYPE_TYPEDBYREF = 0x16;
		internal const byte ELEMENT_TYPE_I = 0x18;
		internal const byte ELEMENT_TYPE_U = 0x19;
		internal const byte ELEMENT_TYPE_FNPTR = 0x1b;
		internal const byte ELEMENT_TYPE_OBJECT = 0x1c;
		internal const byte ELEMENT_TYPE_SZARRAY = 0x1d;
		internal const byte ELEMENT_TYPE_MVAR = 0x1e;
		internal const byte ELEMENT_TYPE_CMOD_REQD = 0x1f;
		internal const byte ELEMENT_TYPE_CMOD_OPT = 0x20;
		internal const byte ELEMENT_TYPE_PINNED = 0x45;

		internal abstract void WriteSig(ModuleBuilder module, ByteBuffer bb);

		private static Type ReadGenericInst(ModuleReader module, ByteReader br, IGenericContext context)
		{
			Type type;
			switch (br.ReadByte())
			{
				case ELEMENT_TYPE_CLASS:
					type = ReadTypeDefOrRefEncoded(module, br, context).MarkNotValueType();
					break;
				case ELEMENT_TYPE_VALUETYPE:
					type = ReadTypeDefOrRefEncoded(module, br, context).MarkValueType();
					break;
				default:
					throw new BadImageFormatException();
			}
			if (!type.__IsMissing && !type.IsGenericTypeDefinition)
			{
				throw new BadImageFormatException();
			}
			int genArgCount = br.ReadCompressedUInt();
			Type[] args = new Type[genArgCount];
			CustomModifiers[] mods = null;
			for (int i = 0; i < genArgCount; i++)
			{
				// LAMESPEC the Type production (23.2.12) doesn't include CustomMod* for genericinst, but C++ uses it, the verifier allows it and ildasm also supports it
				CustomModifiers cm = CustomModifiers.Read(module, br, context);
				if (!cm.IsEmpty)
				{
					if (mods == null)
					{
						mods = new CustomModifiers[genArgCount];
					}
					mods[i] = cm;
				}
				args[i] = ReadType(module, br, context);
			}
			return GenericTypeInstance.Make(type, args, mods);
		}

		internal static Type ReadTypeSpec(ModuleReader module, ByteReader br, IGenericContext context)
		{
			// LAMESPEC a TypeSpec can contain custom modifiers (C++/CLI generates "newarr (TypeSpec with custom modifiers)")
			CustomModifiers.Skip(br);
			// LAMESPEC anything can be adorned by (useless) custom modifiers
			// also, VAR and MVAR are also used in TypeSpec (contrary to what the spec says)
			return ReadType(module, br, context);
		}

		private static Type ReadFunctionPointer(ModuleReader module, ByteReader br, IGenericContext context)
		{
			__StandAloneMethodSig sig = MethodSignature.ReadStandAloneMethodSig(module, br, context);
			if (module.universe.EnableFunctionPointers)
			{
				return FunctionPointerType.Make(module.universe, sig);
			}
			else
			{
				// by default, like .NET we return System.IntPtr here
				return module.universe.System_IntPtr;
			}
		}

		internal static Type[] ReadMethodSpec(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.ReadByte() != GENERICINST)
			{
				throw new BadImageFormatException();
			}
			Type[] args = new Type[br.ReadCompressedUInt()];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = ReadType(module, br, context);
			}
			return args;
		}

		private static int[] ReadArraySizes(ByteReader br)
		{
			int num = br.ReadCompressedUInt();
			if (num == 0)
			{
				return null;
			}
			int[] arr = new int[num];
			for (int i = 0; i < num; i++)
			{
				arr[i] = br.ReadCompressedUInt();
			}
			return arr;
		}

		private static int[] ReadArrayBounds(ByteReader br)
		{
			int num = br.ReadCompressedUInt();
			if (num == 0)
			{
				return null;
			}
			int[] arr = new int[num];
			for (int i = 0; i < num; i++)
			{
				arr[i] = br.ReadCompressedInt();
			}
			return arr;
		}

		private static Type ReadTypeOrVoid(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.PeekByte() == ELEMENT_TYPE_VOID)
			{
				br.ReadByte();
				return module.universe.System_Void;
			}
			else
			{
				return ReadType(module, br, context);
			}
		}

		// see ECMA 335 CLI spec June 2006 section 23.2.12 for this production
		protected static Type ReadType(ModuleReader module, ByteReader br, IGenericContext context)
		{
			CustomModifiers mods;
			switch (br.ReadByte())
			{
				case ELEMENT_TYPE_CLASS:
					return ReadTypeDefOrRefEncoded(module, br, context).MarkNotValueType();
				case ELEMENT_TYPE_VALUETYPE:
					return ReadTypeDefOrRefEncoded(module, br, context).MarkValueType();
				case ELEMENT_TYPE_BOOLEAN:
					return module.universe.System_Boolean;
				case ELEMENT_TYPE_CHAR:
					return module.universe.System_Char;
				case ELEMENT_TYPE_I1:
					return module.universe.System_SByte;
				case ELEMENT_TYPE_U1:
					return module.universe.System_Byte;
				case ELEMENT_TYPE_I2:
					return module.universe.System_Int16;
				case ELEMENT_TYPE_U2:
					return module.universe.System_UInt16;
				case ELEMENT_TYPE_I4:
					return module.universe.System_Int32;
				case ELEMENT_TYPE_U4:
					return module.universe.System_UInt32;
				case ELEMENT_TYPE_I8:
					return module.universe.System_Int64;
				case ELEMENT_TYPE_U8:
					return module.universe.System_UInt64;
				case ELEMENT_TYPE_R4:
					return module.universe.System_Single;
				case ELEMENT_TYPE_R8:
					return module.universe.System_Double;
				case ELEMENT_TYPE_I:
					return module.universe.System_IntPtr;
				case ELEMENT_TYPE_U:
					return module.universe.System_UIntPtr;
				case ELEMENT_TYPE_STRING:
					return module.universe.System_String;
				case ELEMENT_TYPE_OBJECT:
					return module.universe.System_Object;
				case ELEMENT_TYPE_VAR:
					return context.GetGenericTypeArgument(br.ReadCompressedUInt());
				case ELEMENT_TYPE_MVAR:
					return context.GetGenericMethodArgument(br.ReadCompressedUInt());
				case ELEMENT_TYPE_GENERICINST:
					return ReadGenericInst(module, br, context);
				case ELEMENT_TYPE_SZARRAY:
					mods = CustomModifiers.Read(module, br, context);
					return ReadType(module, br, context).__MakeArrayType(mods);
				case ELEMENT_TYPE_ARRAY:
					mods = CustomModifiers.Read(module, br, context);
					return ReadType(module, br, context).__MakeArrayType(br.ReadCompressedUInt(), ReadArraySizes(br), ReadArrayBounds(br), mods);
				case ELEMENT_TYPE_PTR:
					mods = CustomModifiers.Read(module, br, context);
					return ReadTypeOrVoid(module, br, context).__MakePointerType(mods);
				case ELEMENT_TYPE_FNPTR:
					return ReadFunctionPointer(module, br, context);
				default:
					throw new BadImageFormatException();
			}
		}

		internal static void ReadLocalVarSig(ModuleReader module, ByteReader br, IGenericContext context, List<LocalVariableInfo> list)
		{
			if (br.Length < 2 || br.ReadByte() != LOCAL_SIG)
			{
				throw new BadImageFormatException("Invalid local variable signature");
			}
			int count = br.ReadCompressedUInt();
			for (int i = 0; i < count; i++)
			{
				if (br.PeekByte() == ELEMENT_TYPE_TYPEDBYREF)
				{
					br.ReadByte();
					list.Add(new LocalVariableInfo(i, module.universe.System_TypedReference, false, new CustomModifiers()));
				}
				else
				{
					CustomModifiers mods1 = CustomModifiers.Read(module, br, context);
					bool pinned = false;
					if (br.PeekByte() == ELEMENT_TYPE_PINNED)
					{
						br.ReadByte();
						pinned = true;
					}
					CustomModifiers mods2 = CustomModifiers.Read(module, br, context);
					Type type = ReadTypeOrByRef(module, br, context);
					list.Add(new LocalVariableInfo(i, type, pinned, CustomModifiers.Combine(mods1, mods2)));
				}
			}
		}

		private static Type ReadTypeOrByRef(ModuleReader module, ByteReader br, IGenericContext context)
		{
			if (br.PeekByte() == ELEMENT_TYPE_BYREF)
			{
				br.ReadByte();
				// LAMESPEC it is allowed (by C++/CLI, ilasm and peverify) to have custom modifiers after the BYREF
				// (which makes sense, as it is analogous to pointers)
				CustomModifiers mods = CustomModifiers.Read(module, br, context);
				// C++/CLI generates void& local variables, so we need to use ReadTypeOrVoid here
				return ReadTypeOrVoid(module, br, context).__MakeByRefType(mods);
			}
			else
			{
				return ReadType(module, br, context);
			}
		}

		protected static Type ReadRetType(ModuleReader module, ByteReader br, IGenericContext context)
		{
			switch (br.PeekByte())
			{
				case ELEMENT_TYPE_VOID:
					br.ReadByte();
					return module.universe.System_Void;
				case ELEMENT_TYPE_TYPEDBYREF:
					br.ReadByte();
					return module.universe.System_TypedReference;
				default:
					return ReadTypeOrByRef(module, br, context);
			}
		}

		protected static Type ReadParam(ModuleReader module, ByteReader br, IGenericContext context)
		{
			switch (br.PeekByte())
			{
				case ELEMENT_TYPE_TYPEDBYREF:
					br.ReadByte();
					return module.universe.System_TypedReference;
				default:
					return ReadTypeOrByRef(module, br, context);
			}
		}

		protected static void WriteType(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			while (type.HasElementType)
			{
				if (type.__IsVector)
				{
					bb.Write(ELEMENT_TYPE_SZARRAY);
				}
				else if (type.IsArray)
				{
					int rank = type.GetArrayRank();
					bb.Write(ELEMENT_TYPE_ARRAY);
					// LAMESPEC the Type production (23.2.12) doesn't include CustomMod* for arrays, but the verifier allows it and ildasm also supports it
					WriteCustomModifiers(module, bb, type.__GetCustomModifiers());
					WriteType(module, bb, type.GetElementType());
					bb.WriteCompressedUInt(rank);
					int[] sizes = type.__GetArraySizes();
					bb.WriteCompressedUInt(sizes.Length);
					for (int i = 0; i < sizes.Length; i++)
					{
						bb.WriteCompressedUInt(sizes[i]);
					}
					int[] lobounds = type.__GetArrayLowerBounds();
					bb.WriteCompressedUInt(lobounds.Length);
					for (int i = 0; i < lobounds.Length; i++)
					{
						bb.WriteCompressedInt(lobounds[i]);
					}
					return;
				}
				else if (type.IsByRef)
				{
					bb.Write(ELEMENT_TYPE_BYREF);
				}
				else if (type.IsPointer)
				{
					bb.Write(ELEMENT_TYPE_PTR);
				}
				WriteCustomModifiers(module, bb, type.__GetCustomModifiers());
				type = type.GetElementType();
			}
			Universe u = module.universe;
			if (type == u.System_Void)
			{
				bb.Write(ELEMENT_TYPE_VOID);
			}
			else if (type == u.System_Int32)
			{
				bb.Write(ELEMENT_TYPE_I4);
			}
			else if (type == u.System_Boolean)
			{
				bb.Write(ELEMENT_TYPE_BOOLEAN);
			}
			else if (type == u.System_String)
			{
				bb.Write(ELEMENT_TYPE_STRING);
			}
			else if (type == u.System_Char)
			{
				bb.Write(ELEMENT_TYPE_CHAR);
			}
			else if (type == u.System_SByte)
			{
				bb.Write(ELEMENT_TYPE_I1);
			}
			else if (type == u.System_Byte)
			{
				bb.Write(ELEMENT_TYPE_U1);
			}
			else if (type == u.System_Int16)
			{
				bb.Write(ELEMENT_TYPE_I2);
			}
			else if (type == u.System_UInt16)
			{
				bb.Write(ELEMENT_TYPE_U2);
			}
			else if (type == u.System_UInt32)
			{
				bb.Write(ELEMENT_TYPE_U4);
			}
			else if (type == u.System_Int64)
			{
				bb.Write(ELEMENT_TYPE_I8);
			}
			else if (type == u.System_UInt64)
			{
				bb.Write(ELEMENT_TYPE_U8);
			}
			else if (type == u.System_Single)
			{
				bb.Write(ELEMENT_TYPE_R4);
			}
			else if (type == u.System_Double)
			{
				bb.Write(ELEMENT_TYPE_R8);
			}
			else if (type == u.System_IntPtr)
			{
				bb.Write(ELEMENT_TYPE_I);
			}
			else if (type == u.System_UIntPtr)
			{
				bb.Write(ELEMENT_TYPE_U);
			}
			else if (type == u.System_TypedReference)
			{
				bb.Write(ELEMENT_TYPE_TYPEDBYREF);
			}
			else if (type == u.System_Object)
			{
				bb.Write(ELEMENT_TYPE_OBJECT);
			}
			else if (type.IsGenericParameter)
			{
				if (type is UnboundGenericMethodParameter || type.DeclaringMethod != null)
				{
					bb.Write(ELEMENT_TYPE_MVAR);
				}
				else
				{
					bb.Write(ELEMENT_TYPE_VAR);
				}
				bb.WriteCompressedUInt(type.GenericParameterPosition);
			}
			else if (!type.__IsMissing && type.IsGenericType)
			{
				WriteGenericSignature(module, bb, type);
			}
			else if (type.__IsFunctionPointer)
			{
				bb.Write(ELEMENT_TYPE_FNPTR);
				WriteStandAloneMethodSig(module, bb, type.__MethodSignature);
			}
			else
			{
				if (type.IsValueType)
				{
					bb.Write(ELEMENT_TYPE_VALUETYPE);
				}
				else
				{
					bb.Write(ELEMENT_TYPE_CLASS);
				}
				bb.WriteTypeDefOrRefEncoded(module.GetTypeToken(type).Token);
			}
		}

		private static void WriteGenericSignature(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			Type[] typeArguments = type.GetGenericArguments();
			CustomModifiers[] customModifiers = type.__GetGenericArgumentsCustomModifiers();
			if (!type.IsGenericTypeDefinition)
			{
				type = type.GetGenericTypeDefinition();
			}
			bb.Write(ELEMENT_TYPE_GENERICINST);
			if (type.IsValueType)
			{
				bb.Write(ELEMENT_TYPE_VALUETYPE);
			}
			else
			{
				bb.Write(ELEMENT_TYPE_CLASS);
			}
			bb.WriteTypeDefOrRefEncoded(module.GetTypeToken(type).Token);
			bb.WriteCompressedUInt(typeArguments.Length);
			for (int i = 0; i < typeArguments.Length; i++)
			{
				WriteCustomModifiers(module, bb, customModifiers[i]);
				WriteType(module, bb, typeArguments[i]);
			}
		}

		protected static void WriteCustomModifiers(ModuleBuilder module, ByteBuffer bb, CustomModifiers modifiers)
		{
			foreach (CustomModifiers.Entry entry in modifiers)
			{
				bb.Write(entry.IsRequired ? ELEMENT_TYPE_CMOD_REQD : ELEMENT_TYPE_CMOD_OPT);
				bb.WriteTypeDefOrRefEncoded(module.GetTypeTokenForMemberRef(entry.Type));
			}
		}

		internal static Type ReadTypeDefOrRefEncoded(ModuleReader module, ByteReader br, IGenericContext context)
		{
			int encoded = br.ReadCompressedUInt();
			switch (encoded & 3)
			{
				case 0:
					return module.ResolveType((TypeDefTable.Index << 24) + (encoded >> 2), null, null);
				case 1:
					return module.ResolveType((TypeRefTable.Index << 24) + (encoded >> 2), null, null);
				case 2:
					return module.ResolveType((TypeSpecTable.Index << 24) + (encoded >> 2), context);
				default:
					throw new BadImageFormatException();
			}
		}

		internal static void WriteStandAloneMethodSig(ModuleBuilder module, ByteBuffer bb, __StandAloneMethodSig sig)
		{
			if (sig.IsUnmanaged)
			{
				switch (sig.UnmanagedCallingConvention)
				{
					case CallingConvention.Cdecl:
						bb.Write((byte)0x01);	// C
						break;
					case CallingConvention.StdCall:
					case CallingConvention.Winapi:
						bb.Write((byte)0x02);	// STDCALL
						break;
					case CallingConvention.ThisCall:
						bb.Write((byte)0x03);	// THISCALL
						break;
					case CallingConvention.FastCall:
						bb.Write((byte)0x04);	// FASTCALL
						break;
					default:
						throw new ArgumentOutOfRangeException("callingConvention");
				}
			}
			else
			{
				CallingConventions callingConvention = sig.CallingConvention;
				byte flags = 0;
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
			}
			Type[] parameterTypes = sig.ParameterTypes;
			Type[] optionalParameterTypes = sig.OptionalParameterTypes;
			bb.WriteCompressedUInt(parameterTypes.Length + optionalParameterTypes.Length);
			WriteCustomModifiers(module, bb, sig.GetReturnTypeCustomModifiers());
			WriteType(module, bb, sig.ReturnType);
			int index = 0;
			foreach (Type t in parameterTypes)
			{
				WriteCustomModifiers(module, bb, sig.GetParameterCustomModifiers(index++));
				WriteType(module, bb, t);
			}
			// note that optional parameters are only allowed for managed signatures (but we don't enforce that)
			if (optionalParameterTypes.Length > 0)
			{
				bb.Write(SENTINEL);
				foreach (Type t in optionalParameterTypes)
				{
					WriteCustomModifiers(module, bb, sig.GetParameterCustomModifiers(index++));
					WriteType(module, bb, t);
				}
			}
		}

		internal static void WriteTypeSpec(ModuleBuilder module, ByteBuffer bb, Type type)
		{
			WriteType(module, bb, type);
		}

		internal static void WriteMethodSpec(ModuleBuilder module, ByteBuffer bb, Type[] genArgs)
		{
			bb.Write(GENERICINST);
			bb.WriteCompressedUInt(genArgs.Length);
			foreach (Type arg in genArgs)
			{
				WriteType(module, bb, arg);
			}
		}

		// this reads just the optional parameter types, from a MethodRefSig
		internal static Type[] ReadOptionalParameterTypes(ModuleReader module, ByteReader br, IGenericContext context, out CustomModifiers[] customModifiers)
		{
			br.ReadByte();
			int paramCount = br.ReadCompressedUInt();
			CustomModifiers.Skip(br);
			ReadRetType(module, br, context);
			for (int i = 0; i < paramCount; i++)
			{
				if (br.PeekByte() == SENTINEL)
				{
					br.ReadByte();
					Type[] types = new Type[paramCount - i];
					customModifiers = new CustomModifiers[types.Length];
					for (int j = 0; j < types.Length; j++)
					{
						customModifiers[j] = CustomModifiers.Read(module, br, context);
						types[j] = ReadType(module, br, context);
					}
					return types;
				}
				CustomModifiers.Skip(br);
				ReadType(module, br, context);
			}
			customModifiers = Empty<CustomModifiers>.Array;
			return Type.EmptyTypes;
		}

		protected static Type[] BindTypeParameters(IGenericBinder binder, Type[] types)
		{
			if (types == null || types.Length == 0)
			{
				return Type.EmptyTypes;
			}
			Type[] expanded = new Type[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				expanded[i] = types[i].BindTypeParameters(binder);
			}
			return expanded;
		}

		internal static void WriteSignatureHelper(ModuleBuilder module, ByteBuffer bb, byte flags, ushort paramCount, List<Type> args)
		{
			bb.Write(flags);
			if (flags != FIELD)
			{
				bb.WriteCompressedUInt(paramCount);
			}
			foreach (Type type in args)
			{
				if (type == MarkerType.ModOpt)
				{
					bb.Write(ELEMENT_TYPE_CMOD_OPT);
				}
				else if (type == MarkerType.ModReq)
				{
					bb.Write(ELEMENT_TYPE_CMOD_REQD);
				}
				else if (type == MarkerType.Sentinel)
				{
					bb.Write(SENTINEL);
				}
				else if (type == MarkerType.Pinned)
				{
					bb.Write(ELEMENT_TYPE_PINNED);
				}
				else if (type == null)
				{
					bb.Write(ELEMENT_TYPE_VOID);
				}
				else
				{
					WriteType(module, bb, type);
				}
			}
		}
	}
}
