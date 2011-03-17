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
using System.Text;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	sealed class MethodDefImpl : MethodInfo
	{
		private readonly ModuleReader module;
		private readonly int index;
		private readonly TypeDefImpl declaringType;
		private MethodSignature lazyMethodSignature;
		private ParameterInfo returnParameter;
		private ParameterInfo[] parameters;
		private Type[] typeArgs;

		internal MethodDefImpl(ModuleReader module, TypeDefImpl declaringType, int index)
		{
			this.module = module;
			this.index = index;
			this.declaringType = declaringType;
		}

		public override MethodBody GetMethodBody()
		{
			return GetMethodBody(this);
		}

		internal MethodBody GetMethodBody(IGenericContext context)
		{
			if ((GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL)
			{
				// method is not IL
				return null;
			}
			int rva = module.MethodDef.records[index].RVA;
			return rva == 0 ? null : new MethodBody(module, rva, context);
		}

		public override CallingConventions CallingConvention
		{
			get { return this.MethodSignature.CallingConvention; }
		}

		public override MethodAttributes Attributes
		{
			get { return (MethodAttributes)module.MethodDef.records[index].Flags; }
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return (MethodImplAttributes)module.MethodDef.records[index].ImplFlags;
		}

		public override ParameterInfo[] GetParameters()
		{
			PopulateParameters();
			return (ParameterInfo[])parameters.Clone();
		}

		private void PopulateParameters()
		{
			if (parameters == null)
			{
				MethodSignature methodSignature = this.MethodSignature;
				parameters = new ParameterInfo[methodSignature.GetParameterCount()];
				int parameter = module.MethodDef.records[index].ParamList - 1;
				int end = module.MethodDef.records.Length > index + 1 ? module.MethodDef.records[index + 1].ParamList - 1 : module.Param.records.Length;
				for (; parameter < end; parameter++)
				{
					int seq = module.Param.records[parameter].Sequence - 1;
					if (seq == -1)
					{
						returnParameter = new ParameterInfoImpl(this, seq, parameter);
					}
					else
					{
						parameters[seq] = new ParameterInfoImpl(this, seq, parameter);
					}
				}
				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] == null)
					{
						parameters[i] = new ParameterInfoImpl(this, i, -1);
					}
				}
				if (returnParameter == null)
				{
					returnParameter = new ParameterInfoImpl(this, -1, -1);
				}
			}
		}

		internal override int ParameterCount
		{
			get { return this.MethodSignature.GetParameterCount(); }
		}

		public override ParameterInfo ReturnParameter
		{
			get
			{
				PopulateParameters();
				return returnParameter;
			}
		}

		public override Type ReturnType
		{
			get
			{
				return this.ReturnParameter.ParameterType;
			}
		}

		public override Type DeclaringType
		{
			get { return declaringType.IsModulePseudoType ? null : declaringType; }
		}

		public override string Name
		{
			get { return module.GetString(module.MethodDef.records[index].Name); }
		}

		public override int MetadataToken
		{
			get { return (MethodDefTable.Index << 24) + index + 1; }
		}

		public override bool IsGenericMethodDefinition
		{
			get
			{
				PopulateGenericArguments();
				return typeArgs.Length > 0;
			}
		}

		public override bool IsGenericMethod
		{
			get { return IsGenericMethodDefinition; }
		}

		public override Type[] GetGenericArguments()
		{
			PopulateGenericArguments();
			return Util.Copy(typeArgs);
		}

		private void PopulateGenericArguments()
		{
			if (typeArgs == null)
			{
				int token = this.MetadataToken;
				int first = module.GenericParam.FindFirstByOwner(token);
				if (first == -1)
				{
					typeArgs = Type.EmptyTypes;
				}
				else
				{
					List<Type> list = new List<Type>();
					int len = module.GenericParam.records.Length;
					for (int i = first; i < len && module.GenericParam.records[i].Owner == token; i++)
					{
						list.Add(new GenericTypeParameter(module, i));
					}
					typeArgs = list.ToArray();
				}
			}
		}

		internal override Type GetGenericMethodArgument(int index)
		{
			PopulateGenericArguments();
			return typeArgs[index];
		}

		internal override int GetGenericMethodArgumentCount()
		{
			PopulateGenericArguments();
			return typeArgs.Length;
		}

		public override MethodInfo GetGenericMethodDefinition()
		{
			if (this.IsGenericMethodDefinition)
			{
				return this;
			}
			throw new InvalidOperationException();
		}

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			return new GenericMethodInstance(declaringType, this, typeArguments);
		}

		public override Module Module
		{
			get { return module; }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			List<CustomAttributeData> list = module.GetCustomAttributes(this.MetadataToken, attributeType);
			if ((this.Attributes & MethodAttributes.PinvokeImpl) != 0
				&& (attributeType == null || attributeType.IsAssignableFrom(module.universe.System_Runtime_InteropServices_DllImportAttribute)))
			{
				CreateDllImportPseudoCustomAttribute(list);
			}
			return list;
		}

		private void CreateDllImportPseudoCustomAttribute(List<CustomAttributeData> attribs)
		{
			int token = this.MetadataToken;
			// TODO use binary search?
			for (int i = 0; i < module.ImplMap.records.Length; i++)
			{
				if (module.ImplMap.records[i].MemberForwarded == token)
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

					Type type = module.universe.System_Runtime_InteropServices_DllImportAttribute;
					ConstructorInfo constructor = type.GetPseudoCustomAttributeConstructor(module.universe.System_String);
					List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
					int flags = module.ImplMap.records[i].MappingFlags;
					string entryPoint = module.GetString(module.ImplMap.records[i].ImportName);
					string dllName = module.GetString(module.ModuleRef.records[(module.ImplMap.records[i].ImportScope & 0xFFFFFF) - 1]);
					System.Runtime.InteropServices.CharSet? charSet;
					switch (flags & CharSetMask)
					{
						case CharSetAnsi:
							charSet = System.Runtime.InteropServices.CharSet.Ansi;
							break;
						case CharSetUnicode:
							charSet = System.Runtime.InteropServices.CharSet.Unicode;
							break;
						case CharSetAuto:
							charSet = System.Runtime.InteropServices.CharSet.Auto;
							break;
						case CharSetNotSpec:
						default:
							charSet = null;
							break;
					}
					System.Runtime.InteropServices.CallingConvention callingConvention;
					switch (flags & CallConvMask)
					{
						case CallConvCdecl:
							callingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
							break;
						case CallConvFastcall:
							callingConvention = System.Runtime.InteropServices.CallingConvention.FastCall;
							break;
						case CallConvStdcall:
							callingConvention = System.Runtime.InteropServices.CallingConvention.StdCall;
							break;
						case CallConvThiscall:
							callingConvention = System.Runtime.InteropServices.CallingConvention.ThisCall;
							break;
						case CallConvWinapi:
							callingConvention = System.Runtime.InteropServices.CallingConvention.Winapi;
							break;
						default:
							callingConvention = 0;
							break;
					}
					AddNamedArgument(list, type, "EntryPoint", entryPoint);
					AddNamedArgument(list, type, "ExactSpelling", flags, NoMangle);
					AddNamedArgument(list, type, "SetLastError", flags, SupportsLastError);
					AddNamedArgument(list, type, "PreserveSig", (int)GetMethodImplementationFlags(), (int)MethodImplAttributes.PreserveSig);
					AddNamedArgument(list, type, "CallingConvention", module.universe.System_Runtime_InteropServices_CallingConvention, (int)callingConvention);
					if (charSet.HasValue)
					{
						AddNamedArgument(list, type, "CharSet", module.universe.System_Runtime_InteropServices_CharSet, (int)charSet.Value);
					}
					if ((flags & (BestFitOn | BestFitOff)) != 0)
					{
						AddNamedArgument(list, type, "BestFitMapping", flags, BestFitOn);
					}
					if ((flags & (CharMapErrorOn | CharMapErrorOff)) != 0)
					{
						AddNamedArgument(list, type, "ThrowOnUnmappableChar", flags, CharMapErrorOn);
					}
					attribs.Add(new CustomAttributeData(module, constructor, new object[] { dllName }, list));
					return;
				}
			}
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, string value)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_String, value);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, int value)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_Int32, value);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, int flags, int flagMask)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_Boolean, (flags & flagMask) != 0);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type attributeType, string fieldName, Type valueType, object value)
		{
			// some fields are not available on the .NET Compact Framework version of DllImportAttribute
			FieldInfo field = attributeType.FindField(fieldName, FieldSignature.Create(valueType, null, null));
			if (field != null)
			{
				list.Add(new CustomAttributeNamedArgument(field, new CustomAttributeTypedArgument(valueType, value)));
			}
		}

		internal override MethodSignature MethodSignature
		{
			get { return lazyMethodSignature ?? (lazyMethodSignature = MethodSignature.ReadSig(module, module.GetBlob(module.MethodDef.records[index].Signature), this)); }
		}

		internal override int ImportTo(Emit.ModuleBuilder module)
		{
			return module.ImportMethodOrField(declaringType, this.Name, this.MethodSignature);
		}
	}

	sealed class ParameterInfoImpl : ParameterInfo
	{
		private readonly MethodDefImpl method;
		private readonly int position;
		private readonly int index;

		internal ParameterInfoImpl(MethodDefImpl method, int position, int index)
		{
			this.method = method;
			this.position = position;
			this.index = index;
		}

		public override string Name
		{
			get { return index == -1 ? null : ((ModuleReader)this.Module).GetString(this.Module.Param.records[index].Name); }
		}

		public override Type ParameterType
		{
			get { return position == -1 ? method.MethodSignature.GetReturnType(method) : method.MethodSignature.GetParameterType(method, position); }
		}

		public override ParameterAttributes Attributes
		{
			get { return index == -1 ? ParameterAttributes.None : (ParameterAttributes)this.Module.Param.records[index].Flags; }
		}

		public override int Position
		{
			get { return position; }
		}

		public override object RawDefaultValue
		{
			get
			{
				if ((this.Attributes & ParameterAttributes.HasDefault) != 0)
				{
					return this.Module.Constant.GetRawConstantValue(this.Module, this.MetadataToken);
				}
				Universe universe = this.Module.universe;
				if (this.ParameterType == universe.System_Decimal)
				{
					Type attr = universe.System_Runtime_CompilerServices_DecimalConstantAttribute;
					if (attr != null)
					{
						foreach (CustomAttributeData cad in GetCustomAttributesData(attr))
						{
							IList<CustomAttributeTypedArgument> args = cad.ConstructorArguments;
							if (args.Count == 5)
							{
								if (args[0].ArgumentType == universe.System_Byte
									&& args[1].ArgumentType == universe.System_Byte
									&& args[2].ArgumentType == universe.System_Int32
									&& args[3].ArgumentType == universe.System_Int32
									&& args[4].ArgumentType == universe.System_Int32)
								{
									return new Decimal((int)args[4].Value, (int)args[3].Value, (int)args[2].Value, (byte)args[1].Value != 0, (byte)args[0].Value);
								}
								else if (args[0].ArgumentType == universe.System_Byte
									&& args[1].ArgumentType == universe.System_Byte
									&& args[2].ArgumentType == universe.System_UInt32
									&& args[3].ArgumentType == universe.System_UInt32
									&& args[4].ArgumentType == universe.System_UInt32)
								{
									return new Decimal(unchecked((int)(uint)args[4].Value), unchecked((int)(uint)args[3].Value), unchecked((int)(uint)args[2].Value), (byte)args[1].Value != 0, (byte)args[0].Value);
								}
							}
						}
					}
				}
				if ((this.Attributes & ParameterAttributes.Optional) != 0)
				{
					return Missing.Value;
				}
				return null;
			}
		}

		public override Type[] GetRequiredCustomModifiers()
		{
			return Util.Copy(position == -1 ? method.MethodSignature.GetReturnTypeRequiredCustomModifiers(method) : method.MethodSignature.GetParameterRequiredCustomModifiers(method, position));
		}

		public override Type[] GetOptionalCustomModifiers()
		{
			return Util.Copy(position == -1 ? method.MethodSignature.GetReturnTypeOptionalCustomModifiers(method) : method.MethodSignature.GetParameterOptionalCustomModifiers(method, position));
		}

		public override MemberInfo Member
		{
			get
			{
				// return the right ConstructorInfo wrapper
				return method.Module.ResolveMethod(method.MetadataToken);
			}
		}

		public override int MetadataToken
		{
			get
			{
				// for parameters that don't have a row in the Param table, we return 0x08000000 (because index is -1 in that case),
				// just like .NET
				return (ParamTable.Index << 24) + index + 1;
			}
		}

		internal override Module Module
		{
			get { return method.Module; }
		}

		internal override IList<CustomAttributeData> GetCustomAttributesData(Type attributeType)
		{
			IList<CustomAttributeData> list = base.GetCustomAttributesData(attributeType);
			if ((this.Attributes & ParameterAttributes.HasFieldMarshal) != 0
				&& (attributeType == null || attributeType.IsAssignableFrom(this.Module.universe.System_Runtime_InteropServices_MarshalAsAttribute)))
			{
				list.Add(MarshalSpec.GetMarshalAsAttribute(this.Module, this.MetadataToken));
			}
			return list;
		}
	}
}
