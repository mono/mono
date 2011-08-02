/*
  Copyright (C) 2008-2011 Jeroen Frijters

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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class MethodBuilder : MethodInfo
	{
		private readonly TypeBuilder typeBuilder;
		private readonly string name;
		private readonly int pseudoToken;
		private int nameIndex;
		private int signature;
		private Type returnType;
		private Type[] parameterTypes;
		private Type[][][] modifiers;	// see PackedCustomModifiers
		private MethodAttributes attributes;
		private MethodImplAttributes implFlags;
		private ILGenerator ilgen;
		private int rva = -1;
		private CallingConventions callingConvention;
		private List<ParameterBuilder> parameters;
		private GenericTypeParameterBuilder[] gtpb;
		private List<CustomAttributeBuilder> declarativeSecurity;
		private MethodSignature methodSignature;
		private bool initLocals = true;

		internal MethodBuilder(TypeBuilder typeBuilder, string name, MethodAttributes attributes, CallingConventions callingConvention)
		{
			this.typeBuilder = typeBuilder;
			this.name = name;
			this.pseudoToken = typeBuilder.ModuleBuilder.AllocPseudoToken();
			this.attributes = attributes;
			if ((attributes & MethodAttributes.Static) == 0)
			{
				callingConvention |= CallingConventions.HasThis;
			}
			this.callingConvention = callingConvention;
		}

		public ILGenerator GetILGenerator()
		{
			return GetILGenerator(16);
		}

		public ILGenerator GetILGenerator(int streamSize)
		{
			if (rva != -1)
			{
				throw new InvalidOperationException();
			}
			if (ilgen == null)
			{
				ilgen = new ILGenerator(typeBuilder.ModuleBuilder, streamSize);
			}
			return ilgen;
		}

		public void __ReleaseILGenerator()
		{
			if (ilgen != null)
			{
				if (this.ModuleBuilder.symbolWriter != null)
				{
					this.ModuleBuilder.symbolWriter.OpenMethod(new SymbolToken(-pseudoToken | 0x06000000));
				}
				rva = ilgen.WriteBody(initLocals);
				if (this.ModuleBuilder.symbolWriter != null)
				{
					this.ModuleBuilder.symbolWriter.CloseMethod();
				}
				ilgen = null;
			}
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		private void SetDllImportPseudoCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			CallingConvention? callingConvention = customBuilder.GetFieldValue<CallingConvention>("CallingConvention");
			CharSet? charSet = customBuilder.GetFieldValue<CharSet>("CharSet");
			SetDllImportPseudoCustomAttribute((string)customBuilder.GetConstructorArgument(0),
				(string)customBuilder.GetFieldValue("EntryPoint"),
				callingConvention,
				charSet,
				(bool?)customBuilder.GetFieldValue("BestFitMapping"),
				(bool?)customBuilder.GetFieldValue("ThrowOnUnmappableChar"),
				(bool?)customBuilder.GetFieldValue("SetLastError"),
				(bool?)customBuilder.GetFieldValue("PreserveSig"),
				(bool?)customBuilder.GetFieldValue("ExactSpelling"));
		}

		internal void SetDllImportPseudoCustomAttribute(string dllName, string entryName, CallingConvention? nativeCallConv, CharSet? nativeCharSet,
			bool? bestFitMapping, bool? throwOnUnmappableChar, bool? setLastError, bool? preserveSig, bool? exactSpelling)
		{
			const short NoMangle = 0x0001;
			const short CharSetMask = 0x0006;
			const short CharSetNotSpec = 0x0000;
			const short CharSetAnsi = 0x0002;
			const short CharSetUnicode = 0x0004;
			const short CharSetAuto = 0x0006;
			const short SupportsLastError = 0x0040;
			const short CallConvMask = 0x0700;
			const short CallConvWinapi = 0x0100;
			const short CallConvCdecl = 0x0200;
			const short CallConvStdcall = 0x0300;
			const short CallConvThiscall = 0x0400;
			const short CallConvFastcall = 0x0500;
			// non-standard flags
			const short BestFitOn = 0x0010;
			const short BestFitOff = 0x0020;
			const short CharMapErrorOn = 0x1000;
			const short CharMapErrorOff = 0x2000;
			short flags = CharSetNotSpec | CallConvWinapi;
			if (bestFitMapping.HasValue)
			{
				flags |= bestFitMapping.Value ? BestFitOn : BestFitOff;
			}
			if (throwOnUnmappableChar.HasValue)
			{
				flags |= throwOnUnmappableChar.Value ? CharMapErrorOn : CharMapErrorOff;
			}
			if (nativeCallConv.HasValue)
			{
				flags &= ~CallConvMask;
				switch (nativeCallConv.Value)
				{
					case System.Runtime.InteropServices.CallingConvention.Cdecl:
						flags |= CallConvCdecl;
						break;
					case System.Runtime.InteropServices.CallingConvention.FastCall:
						flags |= CallConvFastcall;
						break;
					case System.Runtime.InteropServices.CallingConvention.StdCall:
						flags |= CallConvStdcall;
						break;
					case System.Runtime.InteropServices.CallingConvention.ThisCall:
						flags |= CallConvThiscall;
						break;
					case System.Runtime.InteropServices.CallingConvention.Winapi:
						flags |= CallConvWinapi;
						break;
				}
			}
			if (nativeCharSet.HasValue)
			{
				flags &= ~CharSetMask;
				switch (nativeCharSet.Value)
				{
					case CharSet.Ansi:
					case CharSet.None:
						flags |= CharSetAnsi;
						break;
					case CharSet.Auto:
						flags |= CharSetAuto;
						break;
					case CharSet.Unicode:
						flags |= CharSetUnicode;
						break;
				}
			}
			if (exactSpelling.HasValue && exactSpelling.Value)
			{
				flags |= NoMangle;
			}
			if (!preserveSig.HasValue || preserveSig.Value)
			{
				implFlags |= MethodImplAttributes.PreserveSig;
			}
			if (setLastError.HasValue && setLastError.Value)
			{
				flags |= SupportsLastError;
			}
			ImplMapTable.Record rec = new ImplMapTable.Record();
			rec.MappingFlags = flags;
			rec.MemberForwarded = pseudoToken;
			rec.ImportName = this.ModuleBuilder.Strings.Add(entryName ?? name);
			rec.ImportScope = this.ModuleBuilder.ModuleRef.FindOrAddRecord(dllName == null ? 0 : this.ModuleBuilder.Strings.Add(dllName));
			this.ModuleBuilder.ImplMap.AddRecord(rec);
		}

		private void SetMethodImplAttribute(CustomAttributeBuilder customBuilder)
		{
			MethodImplOptions opt;
			switch (customBuilder.Constructor.ParameterCount)
			{
				case 0:
					opt = 0;
					break;
				case 1:
					{
						object val = customBuilder.GetConstructorArgument(0);
						if (val is short)
						{
							opt = (MethodImplOptions)(short)val;
						}
						else if (val is int)
						{
							opt = (MethodImplOptions)(int)val;
						}
						else
						{
							opt = (MethodImplOptions)val;
						}
						break;
					}
				default:
					throw new NotSupportedException();
			}
			MethodCodeType? type = customBuilder.GetFieldValue<MethodCodeType>("MethodCodeType");
			implFlags = (MethodImplAttributes)opt;
			if (type.HasValue)
			{
				implFlags |= (MethodImplAttributes)type;
			}
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Universe u = this.ModuleBuilder.universe;
			Type type = customBuilder.Constructor.DeclaringType;
			if (type == u.System_Runtime_InteropServices_DllImportAttribute)
			{
				attributes |= MethodAttributes.PinvokeImpl;
				SetDllImportPseudoCustomAttribute(customBuilder.DecodeBlob(this.Module.Assembly));
			}
			else if (type == u.System_Runtime_CompilerServices_MethodImplAttribute)
			{
				SetMethodImplAttribute(customBuilder.DecodeBlob(this.Module.Assembly));
			}
			else if (type == u.System_Runtime_InteropServices_PreserveSigAttribute)
			{
				implFlags |= MethodImplAttributes.PreserveSig;
			}
			else if (type == u.System_Runtime_CompilerServices_SpecialNameAttribute)
			{
				attributes |= MethodAttributes.SpecialName;
			}
			else
			{
				if (type == u.System_Security_SuppressUnmanagedCodeSecurityAttribute)
				{
					attributes |= MethodAttributes.HasSecurity;
				}
				this.ModuleBuilder.SetCustomAttribute(pseudoToken, customBuilder);
			}
		}

		public void __AddDeclarativeSecurity(CustomAttributeBuilder customBuilder)
		{
			attributes |= MethodAttributes.HasSecurity;
			if (declarativeSecurity == null)
			{
				declarativeSecurity = new List<CustomAttributeBuilder>();
			}
			declarativeSecurity.Add(customBuilder);
		}

		public void AddDeclarativeSecurity(System.Security.Permissions.SecurityAction securityAction, System.Security.PermissionSet permissionSet)
		{
			this.ModuleBuilder.AddDeclarativeSecurity(pseudoToken, securityAction, permissionSet);
			this.attributes |= MethodAttributes.HasSecurity;
		}

		public void SetImplementationFlags(MethodImplAttributes attributes)
		{
			implFlags = attributes;
		}

		public ParameterBuilder DefineParameter(int position, ParameterAttributes attributes, string strParamName)
		{
			// the parameter is named "position", but it is actually a sequence number (i.e. 0 = return parameter, 1 = first parameter)
			int sequence = position--;
			if (parameters == null)
			{
				parameters = new List<ParameterBuilder>();
			}
			this.ModuleBuilder.Param.AddVirtualRecord();
			ParameterBuilder pb = new ParameterBuilder(this.ModuleBuilder, sequence, attributes, strParamName);
			if (parameters.Count == 0 || position > parameters[parameters.Count - 1].Position)
			{
				parameters.Add(pb);
			}
			else
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					if (parameters[i].Position > position)
					{
						parameters.Insert(i, pb);
						break;
					}
				}
			}
			return pb;
		}

		public void SetParameters(params Type[] parameterTypes)
		{
			this.parameterTypes = Util.Copy(parameterTypes);
		}

		public void SetReturnType(Type returnType)
		{
			this.returnType = returnType ?? this.Module.universe.System_Void;
		}

		public void SetSignature(Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			this.returnType = returnType ?? this.Module.universe.System_Void;
			this.parameterTypes = Util.Copy(parameterTypes);
			this.modifiers = PackedCustomModifiers.CreateFromExternal(returnTypeOptionalCustomModifiers, returnTypeRequiredCustomModifiers,
				parameterTypeOptionalCustomModifiers, parameterTypeRequiredCustomModifiers, this.parameterTypes.Length);
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters(params string[] names)
		{
			gtpb = new GenericTypeParameterBuilder[names.Length];
			for (int i = 0; i < names.Length; i++)
			{
				gtpb[i] = new GenericTypeParameterBuilder(names[i], null, this, i);
			}
			return (GenericTypeParameterBuilder[])gtpb.Clone();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(typeBuilder, this, typeArguments);
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (gtpb == null)
			{
				throw new InvalidOperationException();
			}
			return this;
		}

		public override Type[] GetGenericArguments()
		{
			return Util.Copy(gtpb);
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			return gtpb[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			return gtpb == null ? 0 : gtpb.Length;
		}

		public override Type ReturnType
		{
			get { return returnType; }
		}

		public override ParameterInfo ReturnParameter
		{
			get { return new ParameterInfoImpl(this, -1); }
		}

		public override MethodAttributes Attributes
		{
			get { return attributes; }
		}

		public void __SetAttributes(MethodAttributes attributes)
		{
			this.attributes = attributes;
		}

		public void __SetCallingConvention(CallingConventions callingConvention)
		{
			this.callingConvention = callingConvention;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return implFlags;
		}

		private sealed class ParameterInfoImpl : ParameterInfo
		{
			private readonly MethodBuilder method;
			private readonly int parameter;

			internal ParameterInfoImpl(MethodBuilder method, int parameter)
			{
				this.method = method;
				this.parameter = parameter;
			}

			private ParameterBuilder ParameterBuilder
			{
				get
				{
					if (method.parameters != null)
					{
						foreach (ParameterBuilder pb in method.parameters)
						{
							if (pb.Position == parameter)
							{
								return pb;
							}
						}
					}
					return null;
				}
			}

			public override string Name
			{
				get
				{
					ParameterBuilder pb = this.ParameterBuilder;
					return pb != null ? pb.Name : null;
				}
			}

			public override Type ParameterType
			{
				get { return parameter == -1 ? method.returnType : method.parameterTypes[parameter]; }
			}

			public override ParameterAttributes Attributes
			{
				get
				{
					ParameterBuilder pb = this.ParameterBuilder;
					return pb != null ? (ParameterAttributes)pb.Attributes : ParameterAttributes.None;
				}
			}

			public override int Position
			{
				get { return parameter; }
			}

			public override object RawDefaultValue
			{
				get
				{
					ParameterBuilder pb = this.ParameterBuilder;
					if (pb != null && (pb.Attributes & (int)ParameterAttributes.HasDefault) != 0)
					{
						return method.ModuleBuilder.Constant.GetRawConstantValue(method.ModuleBuilder, pb.PseudoToken);
					}
					if (pb != null && (pb.Attributes & (int)ParameterAttributes.Optional) != 0)
					{
						return Missing.Value;
					}
					return null;
				}
			}

			private Type[] GetCustomModifiers(int optOrReq)
			{
				if (method.modifiers == null || method.modifiers[parameter + 1] == null)
				{
					return Type.EmptyTypes;
				}
				return Util.Copy(method.modifiers[parameter + 1][optOrReq]);
			}

			public override Type[] GetOptionalCustomModifiers()
			{
				return GetCustomModifiers(0);
			}

			public override Type[] GetRequiredCustomModifiers()
			{
				return GetCustomModifiers(1);
			}

			public override MemberInfo Member
			{
				get { return method; }
			}

			public override int MetadataToken
			{
				get
				{
					ParameterBuilder pb = this.ParameterBuilder;
					return pb != null ? pb.PseudoToken : 0x08000000;
				}
			}

			internal override Module Module
			{
				get { return method.Module; }
			}
		}

		public override ParameterInfo[] GetParameters()
		{
			ParameterInfo[] parameters = new ParameterInfo[parameterTypes.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = new ParameterInfoImpl(this, i);
			}
			return parameters;
		}

		internal override int ParameterCount
		{
			get { return parameterTypes.Length; }
		}

		public override Type DeclaringType
		{
			get { return typeBuilder.IsModulePseudoType ? null : typeBuilder; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override CallingConventions CallingConvention
		{
			get { return callingConvention; }
		}

		public override int MetadataToken
		{
			get { return pseudoToken; }
		}

		public override bool IsGenericMethod
		{
			get { return gtpb != null; }
		}

		public override bool IsGenericMethodDefinition
		{
			get { return gtpb != null; }
		}

		public override Module Module
		{
			get { return typeBuilder.Module; }
		}

		public Module GetModule()
		{
			return typeBuilder.Module;
		}

		public MethodToken GetToken()
		{
			return new MethodToken(pseudoToken);
		}

		public override MethodBody GetMethodBody()
		{
			throw new NotSupportedException();
		}

		public bool InitLocals
		{
			get { return initLocals; }
			set { initLocals = value; }
		}

		public void __AddUnmanagedExport(string name, int ordinal)
		{
			this.ModuleBuilder.AddUnmanagedExport(name, ordinal, this, new RelativeVirtualAddress(0xFFFFFFFF));
		}

		internal void Bake()
		{
			this.nameIndex = this.ModuleBuilder.Strings.Add(name);
			this.signature = this.ModuleBuilder.GetSignatureBlobIndex(this.MethodSignature);

			__ReleaseILGenerator();

			if (declarativeSecurity != null)
			{
				this.ModuleBuilder.AddDeclarativeSecurity(pseudoToken, declarativeSecurity);
			}
		}

		internal ModuleBuilder ModuleBuilder
		{
			get { return typeBuilder.ModuleBuilder; }
		}

		internal void WriteMethodDefRecord(int baseRVA, MetadataWriter mw, ref int paramList)
		{
			if (rva != -1)
			{
				mw.Write(rva + baseRVA);
			}
			else
			{
				mw.Write(0);
			}
			mw.Write((short)implFlags);
			mw.Write((short)attributes);
			mw.WriteStringIndex(nameIndex);
			mw.WriteBlobIndex(signature);
			mw.WriteParam(paramList);
			if (parameters != null)
			{
				paramList += parameters.Count;
			}
		}

		internal void WriteParamRecords(MetadataWriter mw)
		{
			if (parameters != null)
			{
				foreach (ParameterBuilder pb in parameters)
				{
					pb.WriteParamRecord(mw);
				}
			}
		}

		internal void FixupToken(int token, ref int parameterToken)
		{
			typeBuilder.ModuleBuilder.RegisterTokenFixup(this.pseudoToken, token);
			if (parameters != null)
			{
				foreach (ParameterBuilder pb in parameters)
				{
					pb.FixupToken(parameterToken++);
				}
			}
		}

		internal override MethodSignature MethodSignature
		{
			get
			{
				if (methodSignature == null)
				{
					methodSignature = MethodSignature.MakeFromBuilder(returnType, parameterTypes, modifiers, callingConvention, gtpb == null ? 0 : gtpb.Length);
				}
				return methodSignature;
			}
		}

		internal override int ImportTo(ModuleBuilder other)
		{
			if (typeBuilder.IsGenericTypeDefinition)
			{
				return other.ImportMember(TypeBuilder.GetMethod(typeBuilder, this));
			}
			else if (other == typeBuilder.ModuleBuilder)
			{
				return pseudoToken;
			}
			else
			{
				return other.ImportMethodOrField(typeBuilder, name, this.MethodSignature);
			}
		}

		internal void CheckBaked()
		{
			typeBuilder.CheckBaked();
		}
	}
}
