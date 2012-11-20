/*
  Copyright (C) 2008-2012 Jeroen Frijters

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
using System.Runtime.InteropServices;
using IKVM.Reflection;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public abstract class SignatureHelper
	{
		protected readonly byte type;
		protected ushort paramCount;

		sealed class Lazy : SignatureHelper
		{
			private readonly List<Type> args = new List<Type>();

			internal Lazy(byte type)
				: base(type)
			{
			}

			internal override Type ReturnType
			{
				get { return args[0]; }
			}

			public override byte[] GetSignature()
			{
				throw new NotSupportedException();
			}

			internal override ByteBuffer GetSignature(ModuleBuilder module)
			{
				ByteBuffer bb = new ByteBuffer(16);
				Signature.WriteSignatureHelper(module, bb, type, paramCount, args);
				return bb;
			}

			public override void AddSentinel()
			{
				args.Add(MarkerType.Sentinel);
			}

			public override void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers)
			{
				if (pinned)
				{
					args.Add(MarkerType.Pinned);
				}
				foreach (CustomModifiers.Entry mod in customModifiers)
				{
					args.Add(mod.IsRequired ? MarkerType.ModReq : MarkerType.ModOpt);
					args.Add(mod.Type);
				}
				args.Add(argument);
				paramCount++;
			}
		}

		sealed class Eager : SignatureHelper
		{
			private readonly ModuleBuilder module;
			private readonly ByteBuffer bb = new ByteBuffer(16);
			private readonly Type returnType;

			internal Eager(ModuleBuilder module, byte type, Type returnType)
				: base(type)
			{
				this.module = module;
				this.returnType = returnType;
				bb.Write(type);
				if (type != Signature.FIELD)
				{
					// space for parameterCount
					bb.Write((byte)0);
				}
			}

			internal override Type ReturnType
			{
				get { return returnType; }
			}

			public override byte[] GetSignature()
			{
				return GetSignature(null).ToArray();
			}

			internal override ByteBuffer GetSignature(ModuleBuilder module)
			{
				if (type != Signature.FIELD)
				{
					bb.Position = 1;
					bb.Insert(MetadataWriter.GetCompressedUIntLength(paramCount) - bb.GetCompressedUIntLength());
					bb.WriteCompressedUInt(paramCount);
				}
				return bb;
			}

			public override void AddSentinel()
			{
				bb.Write(Signature.SENTINEL);
			}

			public override void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers)
			{
				if (pinned)
				{
					bb.Write(Signature.ELEMENT_TYPE_PINNED);
				}
				foreach (CustomModifiers.Entry mod in customModifiers)
				{
					bb.Write(mod.IsRequired ? Signature.ELEMENT_TYPE_CMOD_REQD : Signature.ELEMENT_TYPE_CMOD_OPT);
					Signature.WriteTypeSpec(module, bb, mod.Type);
				}
				Signature.WriteTypeSpec(module, bb, argument ?? module.universe.System_Void);
				paramCount++;
			}
		}

		private SignatureHelper(byte type)
		{
			this.type = type;
		}

		internal bool HasThis
		{
			get { return (type & Signature.HASTHIS) != 0; }
		}

		internal abstract Type ReturnType
		{
			get;
		}

		internal int ParameterCount
		{
			get { return paramCount; }
		}

		private static SignatureHelper Create(Module mod, byte type, Type returnType)
		{
			ModuleBuilder mb = mod as ModuleBuilder;
			return mb == null
				? (SignatureHelper)new Lazy(type)
				: new Eager(mb, type, returnType);
		}

		public static SignatureHelper GetFieldSigHelper(Module mod)
		{
			return Create(mod, Signature.FIELD, null);
		}

		public static SignatureHelper GetLocalVarSigHelper()
		{
			return new Lazy(Signature.LOCAL_SIG);
		}

		public static SignatureHelper GetLocalVarSigHelper(Module mod)
		{
			return Create(mod, Signature.LOCAL_SIG, null);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper sig = Create(mod, Signature.PROPERTY, returnType);
			sig.AddArgument(returnType);
			sig.paramCount = 0;
			sig.AddArguments(parameterTypes, null, null);
			return sig;
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return GetPropertySigHelper(mod, CallingConventions.Standard, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			byte type = Signature.PROPERTY;
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				type |= Signature.HASTHIS;
			}
			SignatureHelper sig = Create(mod, type, returnType);
			sig.AddArgument(returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
			sig.paramCount = 0;
			sig.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
			return sig;
		}

		public static SignatureHelper GetMethodSigHelper(CallingConvention unmanagedCallingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, unmanagedCallingConvention, returnType);
		}

		public static SignatureHelper GetMethodSigHelper(CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper(null, callingConvention, returnType);
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConvention unmanagedCallConv, Type returnType)
		{
			byte type;
			switch (unmanagedCallConv)
			{
				case CallingConvention.Cdecl:
					type = 0x01;	// C
					break;
				case CallingConvention.StdCall:
				case CallingConvention.Winapi:
					type = 0x02;	// STDCALL
					break;
				case CallingConvention.ThisCall:
					type = 0x03;	// THISCALL
					break;
				case CallingConvention.FastCall:
					type = 0x04;	// FASTCALL
					break;
				default:
					throw new ArgumentOutOfRangeException("unmanagedCallConv");
			}
			SignatureHelper sig = Create(mod, type, returnType);
			sig.AddArgument(returnType);
			sig.paramCount = 0;
			return sig;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType)
		{
			byte type = 0;
			if ((callingConvention & CallingConventions.HasThis) != 0)
			{
				type |= Signature.HASTHIS;
			}
			if ((callingConvention & CallingConventions.ExplicitThis) != 0)
			{
				type |= Signature.EXPLICITTHIS;
			}
			if ((callingConvention & CallingConventions.VarArgs) != 0)
			{
				type |= Signature.VARARG;
			}
			SignatureHelper sig = Create(mod, type, returnType);
			sig.AddArgument(returnType);
			sig.paramCount = 0;
			return sig;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper sig = Create(mod, 0, returnType);
			sig.AddArgument(returnType);
			sig.paramCount = 0;
			sig.AddArguments(parameterTypes, null, null);
			return sig;
		}

		public abstract byte[] GetSignature();

		internal abstract ByteBuffer GetSignature(ModuleBuilder module);

		public abstract void AddSentinel();

		public void AddArgument(Type clsArgument)
		{
			AddArgument(clsArgument, false);
		}

		public void AddArgument(Type argument, bool pinned)
		{
			__AddArgument(argument, pinned, new CustomModifiers());
		}

		public void AddArgument(Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			__AddArgument(argument, false, CustomModifiers.FromReqOpt(requiredCustomModifiers, optionalCustomModifiers));
		}

		public abstract void __AddArgument(Type argument, bool pinned, CustomModifiers customModifiers);

		public void AddArguments(Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			if (arguments != null)
			{
				for (int i = 0; i < arguments.Length; i++)
				{
					__AddArgument(arguments[i], false, CustomModifiers.FromReqOpt(Util.NullSafeElementAt(requiredCustomModifiers, i), Util.NullSafeElementAt(optionalCustomModifiers, i)));
				}
			}
		}
	}
}
