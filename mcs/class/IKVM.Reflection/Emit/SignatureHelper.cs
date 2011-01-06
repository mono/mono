/*
  Copyright (C) 2008, 2010 Jeroen Frijters

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
	public sealed class SignatureHelper
	{
		private readonly ModuleBuilder module;
		private readonly byte type;
		private readonly List<Type> args = new List<Type>();
		private readonly List<LocalBuilder> locals = new List<LocalBuilder>();
		private readonly List<Type[]> requiredCustomModifiers = new List<Type[]>();
		private readonly List<Type[]> optionalCustomModifiers = new List<Type[]>();
		private readonly List<Type> optionalArgs = new List<Type>();
		private Type returnType;
		private Type[] returnTypeRequiredCustomModifiers;
		private Type[] returnTypeOptionalCustomModifiers;
		private CallingConventions callingConvention;
		private CallingConvention unmanagedCallConv;
		private bool unmanaged;
		private bool optional;

		private SignatureHelper(ModuleBuilder module, byte type)
		{
			this.module = module;
			this.type = type;
		}

		internal bool HasThis
		{
			get { return (callingConvention & CallingConventions.HasThis) != 0; }
		}

		internal Type ReturnType
		{
			get { return returnType; }
		}

		internal int ParameterCount
		{
			get { return args.Count + optionalArgs.Count; }
		}

		public static SignatureHelper GetFieldSigHelper(Module mod)
		{
			return new SignatureHelper(mod as ModuleBuilder, Signature.FIELD);
		}

		public static SignatureHelper GetLocalVarSigHelper()
		{
			return new SignatureHelper(null, Signature.LOCAL_SIG);
		}

		public static SignatureHelper GetLocalVarSigHelper(Module mod)
		{
			return new SignatureHelper(mod as ModuleBuilder, Signature.LOCAL_SIG);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper sig = new SignatureHelper(mod as ModuleBuilder, Signature.PROPERTY);
			sig.returnType = returnType;
			sig.returnTypeOptionalCustomModifiers = Type.EmptyTypes;
			sig.returnTypeRequiredCustomModifiers = Type.EmptyTypes;
			foreach (Type type in parameterTypes)
			{
				sig.AddArgument(type);
			}
			return sig;
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			return GetPropertySigHelper(mod, CallingConventions.Standard, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
		}

		public static SignatureHelper GetPropertySigHelper(Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
		{
			SignatureHelper sig = new SignatureHelper(mod as ModuleBuilder, Signature.PROPERTY);
			sig.callingConvention = callingConvention;
			sig.returnType = returnType;
			sig.returnTypeOptionalCustomModifiers = requiredReturnTypeCustomModifiers;
			sig.returnTypeRequiredCustomModifiers = optionalReturnTypeCustomModifiers;
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
			SignatureHelper sig = new SignatureHelper(mod as ModuleBuilder, 0);
			sig.returnType = returnType;
			sig.unmanaged = true;
			sig.unmanagedCallConv = unmanagedCallConv;
			return sig;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType)
		{
			SignatureHelper sig = new SignatureHelper(mod as ModuleBuilder, 0);
			sig.returnType = returnType;
			sig.callingConvention = callingConvention;
			return sig;
		}

		public static SignatureHelper GetMethodSigHelper(Module mod, Type returnType, Type[] parameterTypes)
		{
			SignatureHelper sig = new SignatureHelper(mod as ModuleBuilder, 0);
			sig.returnType = returnType;
			sig.callingConvention = CallingConventions.Standard;
			foreach (Type type in parameterTypes)
			{
				sig.AddArgument(type);
			}
			return sig;
		}

		public byte[] GetSignature()
		{
			if (module == null)
			{
				throw new NotSupportedException();
			}
			return GetSignature(module).ToArray();
		}

		internal ByteBuffer GetSignature(ModuleBuilder module)
		{
			ByteBuffer bb = new ByteBuffer(16);
			switch (type)
			{
				case 0:
					if (unmanaged)
					{
						Signature.WriteStandAloneMethodSig(module, bb, unmanagedCallConv, returnType, args.ToArray());
					}
					else
					{
						Signature.WriteStandAloneMethodSig(module, bb, callingConvention, returnType, args.ToArray(), optionalArgs.ToArray());
					}
					break;
				case Signature.FIELD:
					FieldSignature.Create(args[0], optionalCustomModifiers[0], requiredCustomModifiers[0]).WriteSig(module, bb);
					break;
				case Signature.PROPERTY:
					Signature.WritePropertySig(module, bb, callingConvention, returnType, returnTypeRequiredCustomModifiers, returnTypeOptionalCustomModifiers, args.ToArray(), requiredCustomModifiers.ToArray(), optionalCustomModifiers.ToArray());
					break;
				case Signature.LOCAL_SIG:
					Signature.WriteLocalVarSig(module, bb, locals);
					break;
				default:
					throw new InvalidOperationException();
			}
			return bb;
		}

		public void AddSentinel()
		{
			optional = true;
			callingConvention |= CallingConventions.VarArgs;
		}

		public void AddArgument(Type clsArgument)
		{
			AddArgument(clsArgument, false);
		}

		public void AddArgument(Type argument, bool pinned)
		{
			AddArgument(argument, pinned, Type.EmptyTypes, Type.EmptyTypes);
		}

		public void AddArgument(Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			AddArgument(argument, false, requiredCustomModifiers, optionalCustomModifiers);
		}

		private void AddArgument(Type argument, bool pinned, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
		{
			if (type == Signature.LOCAL_SIG)
			{
				locals.Add(new LocalBuilder(argument, 0, pinned));
			}
			else if (optional)
			{
				this.optionalArgs.Add(argument);
			}
			else
			{
				this.args.Add(argument);
				this.requiredCustomModifiers.Add(requiredCustomModifiers);
				this.optionalCustomModifiers.Add(optionalCustomModifiers);
			}
		}

		public void AddArguments(Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
		{
			for (int i = 0; i < arguments.Length; i++)
			{
				AddArgument(arguments[i], false, requiredCustomModifiers[i], optionalCustomModifiers[i]);
			}
		}
	}
}
