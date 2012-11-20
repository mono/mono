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
using System.Text;
using System.IO;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection
{
	public sealed class CustomAttributeData
	{
		internal static readonly IList<CustomAttributeData> EmptyList = new List<CustomAttributeData>(0).AsReadOnly();

		/*
		 * There are several states a CustomAttributeData object can be in:
		 * 
		 * 1) Unresolved Custom Attribute
		 *    - customAttributeIndex >= 0
		 *    - declSecurityIndex == -1
		 *    - declSecurityBlob == null
		 *    - lazyConstructor = null
		 *    - lazyConstructorArguments = null
		 *    - lazyNamedArguments = null
		 * 
		 * 2) Resolved Custom Attribute
		 *    - customAttributeIndex >= 0
		 *    - declSecurityIndex == -1
		 *    - declSecurityBlob == null
		 *    - lazyConstructor != null
		 *    - lazyConstructorArguments != null
		 *    - lazyNamedArguments != null
		 *    
		 * 3) Pre-resolved Custom Attribute
		 *    - customAttributeIndex = -1
		 *    - declSecurityIndex == -1
		 *    - declSecurityBlob == null
		 *    - lazyConstructor != null
		 *    - lazyConstructorArguments != null
		 *    - lazyNamedArguments != null
		 *    
		 * 4) Pseudo Custom Attribute, .NET 1.x declarative security or result of CustomAttributeBuilder.ToData()
		 *    - customAttributeIndex = -1
		 *    - declSecurityIndex == -1
		 *    - declSecurityBlob == null
		 *    - lazyConstructor != null
		 *    - lazyConstructorArguments != null
		 *    - lazyNamedArguments != null
		 *    
		 * 5) Unresolved declarative security
		 *    - customAttributeIndex = -1
		 *    - declSecurityIndex >= 0
		 *    - declSecurityBlob != null
		 *    - lazyConstructor != null
		 *    - lazyConstructorArguments != null
		 *    - lazyNamedArguments == null
		 * 
		 * 6) Resolved declarative security
		 *    - customAttributeIndex = -1
		 *    - declSecurityIndex >= 0
		 *    - declSecurityBlob == null
		 *    - lazyConstructor != null
		 *    - lazyConstructorArguments != null
		 *    - lazyNamedArguments != null
		 * 
		 */
		private readonly Module module;
		private readonly int customAttributeIndex;
		private readonly int declSecurityIndex;
		private readonly byte[] declSecurityBlob;
		private ConstructorInfo lazyConstructor;
		private IList<CustomAttributeTypedArgument> lazyConstructorArguments;
		private IList<CustomAttributeNamedArgument> lazyNamedArguments;

		// 1) Unresolved Custom Attribute
		internal CustomAttributeData(Module module, int index)
		{
			this.module = module;
			this.customAttributeIndex = index;
			this.declSecurityIndex = -1;
		}

		// 4) Pseudo Custom Attribute, .NET 1.x declarative security
		internal CustomAttributeData(Module module, ConstructorInfo constructor, object[] args, List<CustomAttributeNamedArgument> namedArguments)
			: this(module, constructor, WrapConstructorArgs(args, constructor.MethodSignature), namedArguments)
		{
		}

		private static List<CustomAttributeTypedArgument> WrapConstructorArgs(object[] args, MethodSignature sig)
		{
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>();
			for (int i = 0; i < args.Length; i++)
			{
				list.Add(new CustomAttributeTypedArgument(sig.GetParameterType(i), args[i]));
			}
			return list;
		}

		// 4) Pseudo Custom Attribute, .NET 1.x declarative security or result of CustomAttributeBuilder.ToData()
		internal CustomAttributeData(Module module, ConstructorInfo constructor, List<CustomAttributeTypedArgument> constructorArgs, List<CustomAttributeNamedArgument> namedArguments)
		{
			this.module = module;
			this.customAttributeIndex = -1;
			this.declSecurityIndex = -1;
			this.lazyConstructor = constructor;
			lazyConstructorArguments = constructorArgs.AsReadOnly();
			if (namedArguments == null)
			{
				this.lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
			}
			else
			{
				this.lazyNamedArguments = namedArguments.AsReadOnly();
			}
		}

		// 3) Pre-resolved Custom Attribute
		internal CustomAttributeData(Assembly asm, ConstructorInfo constructor, ByteReader br)
		{
			this.module = asm.ManifestModule;
			this.customAttributeIndex = -1;
			this.declSecurityIndex = -1;
			this.lazyConstructor = constructor;
			if (br.Length == 0)
			{
				// it's legal to have an empty blob
				lazyConstructorArguments = Empty<CustomAttributeTypedArgument>.Array;
				lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
			}
			else
			{
				if (br.ReadUInt16() != 1)
				{
					throw new BadImageFormatException();
				}
				lazyConstructorArguments = ReadConstructorArguments(asm, br, constructor);
				lazyNamedArguments = ReadNamedArguments(asm, br, br.ReadUInt16(), constructor.DeclaringType);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append('[');
			sb.Append(Constructor.DeclaringType.FullName);
			sb.Append('(');
			string sep = "";
			ParameterInfo[] parameters = Constructor.GetParameters();
			IList<CustomAttributeTypedArgument> args = ConstructorArguments;
			for (int i = 0; i < parameters.Length; i++)
			{
				sb.Append(sep);
				sep = ", ";
				AppendValue(sb, parameters[i].ParameterType, args[i]);
			}
			foreach (CustomAttributeNamedArgument named in NamedArguments)
			{
				sb.Append(sep);
				sep = ", ";
				sb.Append(named.MemberInfo.Name);
				sb.Append(" = ");
				FieldInfo fi = named.MemberInfo as FieldInfo;
				Type type = fi != null ? fi.FieldType : ((PropertyInfo)named.MemberInfo).PropertyType;
				AppendValue(sb, type, named.TypedValue);
			}
			sb.Append(')');
			sb.Append(']');
			return sb.ToString();
		}

		private static void AppendValue(StringBuilder sb, Type type, CustomAttributeTypedArgument arg)
		{
			if (arg.ArgumentType == arg.ArgumentType.Module.universe.System_String)
			{
				sb.Append('"').Append(arg.Value).Append('"');
			}
			else if (arg.ArgumentType.IsArray)
			{
				Type elementType = arg.ArgumentType.GetElementType();
				string elementTypeName;
				if (elementType.IsPrimitive
					|| elementType == type.Module.universe.System_Object
					|| elementType == type.Module.universe.System_String
					|| elementType == type.Module.universe.System_Type)
				{
					elementTypeName = elementType.Name;
				}
				else
				{
					elementTypeName = elementType.FullName;
				}
				sb.Append("new ").Append(elementTypeName).Append("[").Append(((Array)arg.Value).Length).Append("] { ");
				string sep = "";
				foreach (CustomAttributeTypedArgument elem in (CustomAttributeTypedArgument[])arg.Value)
				{
					sb.Append(sep);
					sep = ", ";
					AppendValue(sb, elementType, elem);
				}
				sb.Append(" }");
			}
			else
			{
				if (arg.ArgumentType != type || (type.IsEnum && !arg.Value.Equals(0)))
				{
					sb.Append('(');
					sb.Append(arg.ArgumentType.FullName);
					sb.Append(')');
				}
				sb.Append(arg.Value);
			}
		}

		internal static void ReadDeclarativeSecurity(Module module, int index, List<CustomAttributeData> list)
		{
			Universe u = module.universe;
			Assembly asm = module.Assembly;
			int action = module.DeclSecurity.records[index].Action;
			ByteReader br = module.GetBlob(module.DeclSecurity.records[index].PermissionSet);
			if (br.PeekByte() == '.')
			{
				br.ReadByte();
				int count = br.ReadCompressedUInt();
				for (int j = 0; j < count; j++)
				{
					Type type = ReadType(asm, br);
					ConstructorInfo constructor = type.GetPseudoCustomAttributeConstructor(u.System_Security_Permissions_SecurityAction);
					// LAMESPEC there is an additional length here (probably of the named argument list)
					byte[] blob = br.ReadBytes(br.ReadCompressedUInt());
					list.Add(new CustomAttributeData(asm, constructor, action, blob, index));
				}
			}
			else
			{
				// .NET 1.x format (xml)
				char[] buf = new char[br.Length / 2];
				for (int i = 0; i < buf.Length; i++)
				{
					buf[i] = br.ReadChar();
				}
				string xml = new String(buf);
				ConstructorInfo constructor = u.System_Security_Permissions_PermissionSetAttribute.GetPseudoCustomAttributeConstructor(u.System_Security_Permissions_SecurityAction);
				List<CustomAttributeNamedArgument> args = new List<CustomAttributeNamedArgument>();
				args.Add(new CustomAttributeNamedArgument(GetProperty(u.System_Security_Permissions_PermissionSetAttribute, "XML", u.System_String),
					new CustomAttributeTypedArgument(u.System_String, xml)));
				list.Add(new CustomAttributeData(asm.ManifestModule, constructor, new object[] { action }, args));
			}
		}

		// 5) Unresolved declarative security
		internal CustomAttributeData(Assembly asm, ConstructorInfo constructor, int securityAction, byte[] blob, int index)
		{
			this.module = asm.ManifestModule;
			this.customAttributeIndex = -1;
			this.declSecurityIndex = index;
			Universe u = constructor.Module.universe;
			this.lazyConstructor = constructor;
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>();
			list.Add(new CustomAttributeTypedArgument(u.System_Security_Permissions_SecurityAction, securityAction));
			this.lazyConstructorArguments =  list.AsReadOnly();
			this.declSecurityBlob = blob;
		}

		private static Type ReadFieldOrPropType(Assembly asm, ByteReader br)
		{
			Universe u = asm.universe;
			switch (br.ReadByte())
			{
				case Signature.ELEMENT_TYPE_BOOLEAN:
					return u.System_Boolean;
				case Signature.ELEMENT_TYPE_CHAR:
					return u.System_Char;
				case Signature.ELEMENT_TYPE_I1:
					return u.System_SByte;
				case Signature.ELEMENT_TYPE_U1:
					return u.System_Byte;
				case Signature.ELEMENT_TYPE_I2:
					return u.System_Int16;
				case Signature.ELEMENT_TYPE_U2:
					return u.System_UInt16;
				case Signature.ELEMENT_TYPE_I4:
					return u.System_Int32;
				case Signature.ELEMENT_TYPE_U4:
					return u.System_UInt32;
				case Signature.ELEMENT_TYPE_I8:
					return u.System_Int64;
				case Signature.ELEMENT_TYPE_U8:
					return u.System_UInt64;
				case Signature.ELEMENT_TYPE_R4:
					return u.System_Single;
				case Signature.ELEMENT_TYPE_R8:
					return u.System_Double;
				case Signature.ELEMENT_TYPE_STRING:
					return u.System_String;
				case Signature.ELEMENT_TYPE_SZARRAY:
					return ReadFieldOrPropType(asm, br).MakeArrayType();
				case 0x55:
					return ReadType(asm, br);
				case 0x50:
					return u.System_Type;
				case 0x51:
					return u.System_Object;
				default:
					throw new BadImageFormatException();
			}
		}

		private static CustomAttributeTypedArgument ReadFixedArg(Assembly asm, ByteReader br, Type type)
		{
			Universe u = asm.universe;
			if (type == u.System_String)
			{
				return new CustomAttributeTypedArgument(type, br.ReadString());
			}
			else if (type == u.System_Boolean)
			{
				return new CustomAttributeTypedArgument(type, br.ReadByte() != 0);
			}
			else if (type == u.System_Char)
			{
				return new CustomAttributeTypedArgument(type, br.ReadChar());
			}
			else if (type == u.System_Single)
			{
				return new CustomAttributeTypedArgument(type, br.ReadSingle());
			}
			else if (type == u.System_Double)
			{
				return new CustomAttributeTypedArgument(type, br.ReadDouble());
			}
			else if (type == u.System_SByte)
			{
				return new CustomAttributeTypedArgument(type, br.ReadSByte());
			}
			else if (type == u.System_Int16)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt16());
			}
			else if (type == u.System_Int32)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt32());
			}
			else if (type == u.System_Int64)
			{
				return new CustomAttributeTypedArgument(type, br.ReadInt64());
			}
			else if (type == u.System_Byte)
			{
				return new CustomAttributeTypedArgument(type, br.ReadByte());
			}
			else if (type == u.System_UInt16)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt16());
			}
			else if (type == u.System_UInt32)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt32());
			}
			else if (type == u.System_UInt64)
			{
				return new CustomAttributeTypedArgument(type, br.ReadUInt64());
			}
			else if (type == u.System_Type)
			{
				return new CustomAttributeTypedArgument(type, ReadType(asm, br));
			}
			else if (type == u.System_Object)
			{
				return ReadFixedArg(asm, br, ReadFieldOrPropType(asm, br));
			}
			else if (type.IsArray)
			{
				int length = br.ReadInt32();
				if (length == -1)
				{
					return new CustomAttributeTypedArgument(type, null);
				}
				Type elementType = type.GetElementType();
				CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[length];
				for (int i = 0; i < length; i++)
				{
					array[i] = ReadFixedArg(asm, br, elementType);
				}
				return new CustomAttributeTypedArgument(type, array);
			}
			else if (type.IsEnum)
			{
				return new CustomAttributeTypedArgument(type, ReadFixedArg(asm, br, type.GetEnumUnderlyingTypeImpl()).Value);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		private static Type ReadType(Assembly asm, ByteReader br)
		{
			string typeName = br.ReadString();
			if (typeName == null)
			{
				return null;
			}
			if (typeName.Length > 0 && typeName[typeName.Length - 1] == 0)
			{
				// there are broken compilers that emit an extra NUL character after the type name
				typeName = typeName.Substring(0, typeName.Length - 1);
			}
			return TypeNameParser.Parse(typeName, true).GetType(asm.universe, asm, true, typeName, true, false);
		}

		private static IList<CustomAttributeTypedArgument> ReadConstructorArguments(Assembly asm, ByteReader br, ConstructorInfo constructor)
		{
			MethodSignature sig = constructor.MethodSignature;
			int count = sig.GetParameterCount();
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadFixedArg(asm, br, sig.GetParameterType(i)));
			}
			return list.AsReadOnly();
		}

		private static IList<CustomAttributeNamedArgument> ReadNamedArguments(Assembly asm, ByteReader br, int named, Type type)
		{
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>(named);
			for (int i = 0; i < named; i++)
			{
				byte fieldOrProperty = br.ReadByte();
				Type fieldOrPropertyType = ReadFieldOrPropType(asm, br);
				string name = br.ReadString();
				CustomAttributeTypedArgument value = ReadFixedArg(asm, br, fieldOrPropertyType);
				MemberInfo member;
				switch (fieldOrProperty)
				{
					case 0x53:
						member = GetField(type, name, fieldOrPropertyType);
						break;
					case 0x54:
						member = GetProperty(type, name, fieldOrPropertyType);
						break;
					default:
						throw new BadImageFormatException();
				}
				list.Add(new CustomAttributeNamedArgument(member, value));
			}
			return list.AsReadOnly();
		}

		private static FieldInfo GetField(Type type, string name, Type fieldType)
		{
			Type org = type;
			for (; type != null && !type.__IsMissing; type = type.BaseType)
			{
				foreach (FieldInfo field in type.__GetDeclaredFields())
				{
					if (field.IsPublic && !field.IsStatic && field.Name == name)
					{
						return field;
					}
				}
			}
			// if the field is missing, we stick the missing field on the first missing base type
			if (type == null)
			{
				type = org;
			}
			FieldSignature sig = FieldSignature.Create(fieldType, new CustomModifiers());
			return type.FindField(name, sig)
				?? type.Module.universe.GetMissingFieldOrThrow(type, name, sig);
		}

		private static PropertyInfo GetProperty(Type type, string name, Type propertyType)
		{
			Type org = type;
			for (; type != null && !type.__IsMissing; type = type.BaseType)
			{
				foreach (PropertyInfo property in type.__GetDeclaredProperties())
				{
					if (property.IsPublic && !property.IsStatic && property.Name == name)
					{
						return property;
					}
				}
			}
			// if the property is missing, we stick the missing property on the first missing base type
			if (type == null)
			{
				type = org;
			}
			return type.Module.universe.GetMissingPropertyOrThrow(type, name, PropertySignature.Create(CallingConventions.Standard | CallingConventions.HasThis, propertyType, null, new PackedCustomModifiers()));
		}

		[Obsolete("Use AttributeType property instead.")]
		internal bool __TryReadTypeName(out string ns, out string name)
		{
			if (Constructor.DeclaringType.IsNested)
			{
				ns = null;
				name = null;
				return false;
			}
			ns = Constructor.DeclaringType.__Namespace;
			name = Constructor.DeclaringType.__Name;
			return true;
		}

		public byte[] __GetBlob()
		{
			if (declSecurityBlob != null)
			{
				return (byte[])declSecurityBlob.Clone();
			}
			else if (customAttributeIndex == -1)
			{
				return __ToBuilder().GetBlob(module.Assembly);
			}
			else
			{
				return ((ModuleReader)module).GetBlobCopy(module.CustomAttribute.records[customAttributeIndex].Value);
			}
		}

		public int __Parent
		{
			get
			{
				return customAttributeIndex >= 0
					? module.CustomAttribute.records[customAttributeIndex].Parent
					: declSecurityIndex >= 0
						? module.DeclSecurity.records[declSecurityIndex].Parent
						: 0;
			}
		}

		// .NET 4.5 API
		public Type AttributeType
		{
			get { return Constructor.DeclaringType; }
		}

		public ConstructorInfo Constructor
		{
			get
			{
				if (lazyConstructor == null)
				{
					lazyConstructor = (ConstructorInfo)module.ResolveMethod(module.CustomAttribute.records[customAttributeIndex].Type);
				}
				return lazyConstructor;
			}
		}

		public IList<CustomAttributeTypedArgument> ConstructorArguments
		{
			get
			{
				if (lazyConstructorArguments == null)
				{
					LazyParseArguments();
				}
				return lazyConstructorArguments;
			}
		}

		public IList<CustomAttributeNamedArgument> NamedArguments
		{
			get
			{
				if (lazyNamedArguments == null)
				{
					if (customAttributeIndex >= 0)
					{
						// 1) Unresolved Custom Attribute
						LazyParseArguments();
					}
					else
					{
						// 5) Unresolved declarative security
						ByteReader br = new ByteReader(declSecurityBlob, 0, declSecurityBlob.Length);
						// LAMESPEC the count of named arguments is a compressed integer (instead of UInt16 as NumNamed in custom attributes)
						lazyNamedArguments = ReadNamedArguments(module.Assembly, br, br.ReadCompressedUInt(), Constructor.DeclaringType);
					}
				}
				return lazyNamedArguments;
			}
		}

		private void LazyParseArguments()
		{
			ByteReader br = module.GetBlob(module.CustomAttribute.records[customAttributeIndex].Value);
			if (br.Length == 0)
			{
				// it's legal to have an empty blob
				lazyConstructorArguments = Empty<CustomAttributeTypedArgument>.Array;
				lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
			}
			else
			{
				if (br.ReadUInt16() != 1)
				{
					throw new BadImageFormatException();
				}
				lazyConstructorArguments = ReadConstructorArguments(module.Assembly, br, Constructor);
				lazyNamedArguments = ReadNamedArguments(module.Assembly, br, br.ReadUInt16(), Constructor.DeclaringType);
			}
		}

		public CustomAttributeBuilder __ToBuilder()
		{
			ParameterInfo[] parameters = Constructor.GetParameters();
			object[] args = new object[ConstructorArguments.Count];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = RewrapArray(parameters[i].ParameterType, ConstructorArguments[i]);
			}
			List<PropertyInfo> namedProperties = new List<PropertyInfo>();
			List<object> propertyValues = new List<object>();
			List<FieldInfo> namedFields = new List<FieldInfo>();
			List<object> fieldValues = new List<object>();
			foreach (CustomAttributeNamedArgument named in NamedArguments)
			{
				PropertyInfo pi = named.MemberInfo as PropertyInfo;
				if (pi != null)
				{
					namedProperties.Add(pi);
					propertyValues.Add(RewrapArray(pi.PropertyType, named.TypedValue));
				}
				else
				{
					FieldInfo fi = (FieldInfo)named.MemberInfo;
					namedFields.Add(fi);
					fieldValues.Add(RewrapArray(fi.FieldType, named.TypedValue));
				}
			}
			return new CustomAttributeBuilder(Constructor, args, namedProperties.ToArray(), propertyValues.ToArray(), namedFields.ToArray(), fieldValues.ToArray());
		}

		private static object RewrapArray(Type type, CustomAttributeTypedArgument arg)
		{
			IList<CustomAttributeTypedArgument> list = arg.Value as IList<CustomAttributeTypedArgument>;
			if (list != null)
			{
				Type elementType = arg.ArgumentType.GetElementType();
				object[] arr = new object[list.Count];
				for (int i = 0; i < arr.Length; i++)
				{
					arr[i] = RewrapArray(elementType, list[i]);
				}
				if (type == type.Module.universe.System_Object)
				{
					return CustomAttributeBuilder.__MakeTypedArgument(arg.ArgumentType, arr);
				}
				return arr;
			}
			else
			{
				return arg.Value;
			}
		}

		public static IList<CustomAttributeData> GetCustomAttributes(MemberInfo member)
		{
			return __GetCustomAttributes(member, null, false);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Assembly assembly)
		{
			return assembly.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Module module)
		{
			return __GetCustomAttributes(module, null, false);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(ParameterInfo parameter)
		{
			return __GetCustomAttributes(parameter, null, false);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Assembly assembly, Type attributeType, bool inherit)
		{
			return assembly.GetCustomAttributesData(attributeType);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Module module, Type attributeType, bool inherit)
		{
			if (module.__IsMissing)
			{
				throw new MissingModuleException((MissingModule)module);
			}
			return GetCustomAttributesImpl(null, module, 0x00000001, attributeType) ?? EmptyList;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(ParameterInfo parameter, Type attributeType, bool inherit)
		{
			Module module = parameter.Module;
			List<CustomAttributeData> list = null;
			if (module.universe.ReturnPseudoCustomAttributes)
			{
				if (attributeType == null || attributeType.IsAssignableFrom(parameter.Module.universe.System_Runtime_InteropServices_MarshalAsAttribute))
				{
					FieldMarshal spec;
					if (parameter.__TryGetFieldMarshal(out spec))
					{
						if (list == null)
						{
							list = new List<CustomAttributeData>();
						}
						list.Add(CustomAttributeData.CreateMarshalAsPseudoCustomAttribute(parameter.Module, spec));
					}
				}
			}
			ModuleBuilder mb = module as ModuleBuilder;
			int token = parameter.MetadataToken;
			if (mb != null && mb.IsSaved && ModuleBuilder.IsPseudoToken(token))
			{
				token = mb.ResolvePseudoToken(token);
			}
			return GetCustomAttributesImpl(list, module, token, attributeType) ?? EmptyList;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
		{
			if (!member.IsBaked)
			{
				// like .NET we we don't return custom attributes for unbaked members
				throw new NotImplementedException();
			}
			if (!inherit || !IsInheritableAttribute(attributeType))
			{
				return GetCustomAttributesImpl(null, member, attributeType) ?? EmptyList;
			}
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			for (; ; )
			{
				GetCustomAttributesImpl(list, member, attributeType);
				Type type = member as Type;
				if (type != null)
				{
					type = type.BaseType;
					if (type == null)
					{
						return list;
					}
					member = type;
					continue;
				}
				MethodInfo method = member as MethodInfo;
				if (method != null)
				{
					MemberInfo prev = member;
					method = method.GetBaseDefinition();
					if (method == null || method == prev)
					{
						return list;
					}
					member = method;
					continue;
				}
				return list;
			}
		}

		private static List<CustomAttributeData> GetCustomAttributesImpl(List<CustomAttributeData> list, MemberInfo member, Type attributeType)
		{
			if (member.Module.universe.ReturnPseudoCustomAttributes)
			{
				List<CustomAttributeData> pseudo = member.GetPseudoCustomAttributes(attributeType);
				if (list == null)
				{
					list = pseudo;
				}
				else if (pseudo != null)
				{
					list.AddRange(pseudo);
				}
			}
			return GetCustomAttributesImpl(list, member.Module, member.GetCurrentToken(), attributeType);
		}

		internal static List<CustomAttributeData> GetCustomAttributesImpl(List<CustomAttributeData> list, Module module, int token, Type attributeType)
		{
			foreach (int i in module.CustomAttribute.Filter(token))
			{
				if (attributeType == null)
				{
					if (list == null)
					{
						list = new List<CustomAttributeData>();
					}
					list.Add(new CustomAttributeData(module, i));
				}
				else
				{
					if (attributeType.IsAssignableFrom(module.ResolveMethod(module.CustomAttribute.records[i].Type).DeclaringType))
					{
						if (list == null)
						{
							list = new List<CustomAttributeData>();
						}
						list.Add(new CustomAttributeData(module, i));
					}
				}
			}
			return list;
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Type type, Type interfaceType, Type attributeType, bool inherit)
		{
			Module module = type.Module;
			foreach (int i in module.InterfaceImpl.Filter(type.MetadataToken))
			{
				if (module.ResolveType(module.InterfaceImpl.records[i].Interface, type) == interfaceType)
				{
					return GetCustomAttributesImpl(null, module, (InterfaceImplTable.Index << 24) | (i + 1), attributeType) ?? EmptyList;
				}
			}
			return EmptyList;
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(Assembly assembly)
		{
			if (assembly.__IsMissing)
			{
				throw new MissingAssemblyException((MissingAssembly)assembly);
			}
			return assembly.ManifestModule.GetDeclarativeSecurity(0x20000001);
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(Type type)
		{
			if ((type.Attributes & TypeAttributes.HasSecurity) != 0)
			{
				return type.Module.GetDeclarativeSecurity(type.MetadataToken);
			}
			else
			{
				return EmptyList;
			}
		}

		public static IList<CustomAttributeData> __GetDeclarativeSecurity(MethodBase method)
		{
			if ((method.Attributes & MethodAttributes.HasSecurity) != 0)
			{
				return method.Module.GetDeclarativeSecurity(method.MetadataToken);
			}
			else
			{
				return EmptyList;
			}
		}

		private static bool IsInheritableAttribute(Type attribute)
		{
			Type attributeUsageAttribute = attribute.Module.universe.System_AttributeUsageAttribute;
			IList<CustomAttributeData> attr = __GetCustomAttributes(attribute, attributeUsageAttribute, false);
			if (attr.Count != 0)
			{
				foreach (CustomAttributeNamedArgument named in attr[0].NamedArguments)
				{
					if (named.MemberInfo.Name == "Inherited")
					{
						return (bool)named.TypedValue.Value;
					}
				}
			}
			return true;
		}

		internal static CustomAttributeData CreateDllImportPseudoCustomAttribute(Module module, ImplMapFlags flags, string entryPoint, string dllName, MethodImplAttributes attr)
		{
			Type type = module.universe.System_Runtime_InteropServices_DllImportAttribute;
			ConstructorInfo constructor = type.GetPseudoCustomAttributeConstructor(module.universe.System_String);
			List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
			System.Runtime.InteropServices.CharSet charSet;
			switch (flags & ImplMapFlags.CharSetMask)
			{
				case ImplMapFlags.CharSetAnsi:
					charSet = System.Runtime.InteropServices.CharSet.Ansi;
					break;
				case ImplMapFlags.CharSetUnicode:
					charSet = System.Runtime.InteropServices.CharSet.Unicode;
					break;
				case ImplMapFlags.CharSetAuto:
					charSet = System.Runtime.InteropServices.CharSet.Auto;
					break;
				case ImplMapFlags.CharSetNotSpec:
				default:
					charSet = System.Runtime.InteropServices.CharSet.None;
					break;
			}
			System.Runtime.InteropServices.CallingConvention callingConvention;
			switch (flags & ImplMapFlags.CallConvMask)
			{
				case ImplMapFlags.CallConvCdecl:
					callingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
					break;
				case ImplMapFlags.CallConvFastcall:
					callingConvention = System.Runtime.InteropServices.CallingConvention.FastCall;
					break;
				case ImplMapFlags.CallConvStdcall:
					callingConvention = System.Runtime.InteropServices.CallingConvention.StdCall;
					break;
				case ImplMapFlags.CallConvThiscall:
					callingConvention = System.Runtime.InteropServices.CallingConvention.ThisCall;
					break;
				case ImplMapFlags.CallConvWinapi:
					callingConvention = System.Runtime.InteropServices.CallingConvention.Winapi;
					break;
				default:
					callingConvention = 0;
					break;
			}
			AddNamedArgument(list, type, "EntryPoint", entryPoint);
			AddNamedArgument(list, type, "CharSet", module.universe.System_Runtime_InteropServices_CharSet, (int)charSet);
			AddNamedArgument(list, type, "ExactSpelling", (int)flags, (int)ImplMapFlags.NoMangle);
			AddNamedArgument(list, type, "SetLastError", (int)flags, (int)ImplMapFlags.SupportsLastError);
			AddNamedArgument(list, type, "PreserveSig", (int)attr, (int)MethodImplAttributes.PreserveSig);
			AddNamedArgument(list, type, "CallingConvention", module.universe.System_Runtime_InteropServices_CallingConvention, (int)callingConvention);
			AddNamedArgument(list, type, "BestFitMapping", (int)flags, (int)ImplMapFlags.BestFitOn);
			AddNamedArgument(list, type, "ThrowOnUnmappableChar", (int)flags, (int)ImplMapFlags.CharMapErrorOn);
			return new CustomAttributeData(module, constructor, new object[] { dllName }, list);
		}

		internal static CustomAttributeData CreateMarshalAsPseudoCustomAttribute(Module module, FieldMarshal fm)
		{
			Type typeofMarshalAs = module.universe.System_Runtime_InteropServices_MarshalAsAttribute;
			Type typeofUnmanagedType = module.universe.System_Runtime_InteropServices_UnmanagedType;
			Type typeofVarEnum = module.universe.System_Runtime_InteropServices_VarEnum;
			Type typeofType = module.universe.System_Type;
			List<CustomAttributeNamedArgument> named = new List<CustomAttributeNamedArgument>();
			AddNamedArgument(named, typeofMarshalAs, "ArraySubType", typeofUnmanagedType, (int)(fm.ArraySubType ?? 0));
			AddNamedArgument(named, typeofMarshalAs, "SizeParamIndex", module.universe.System_Int16, fm.SizeParamIndex ?? 0);
			AddNamedArgument(named, typeofMarshalAs, "SizeConst", module.universe.System_Int32, fm.SizeConst ?? 0);
			AddNamedArgument(named, typeofMarshalAs, "IidParameterIndex", module.universe.System_Int32, fm.IidParameterIndex ?? 0);
			AddNamedArgument(named, typeofMarshalAs, "SafeArraySubType", typeofVarEnum, (int)(fm.SafeArraySubType ?? 0));
			if (fm.SafeArrayUserDefinedSubType != null)
			{
				AddNamedArgument(named, typeofMarshalAs, "SafeArrayUserDefinedSubType", typeofType, fm.SafeArrayUserDefinedSubType);
			}
			if (fm.MarshalType != null)
			{
				AddNamedArgument(named, typeofMarshalAs, "MarshalType", module.universe.System_String, fm.MarshalType);
			}
			if (fm.MarshalTypeRef != null)
			{
				AddNamedArgument(named, typeofMarshalAs, "MarshalTypeRef", module.universe.System_Type, fm.MarshalTypeRef);
			}
			if (fm.MarshalCookie != null)
			{
				AddNamedArgument(named, typeofMarshalAs, "MarshalCookie", module.universe.System_String, fm.MarshalCookie);
			}
			ConstructorInfo constructor = typeofMarshalAs.GetPseudoCustomAttributeConstructor(typeofUnmanagedType);
			return new CustomAttributeData(module, constructor, new object[] { (int)fm.UnmanagedType }, named);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, string value)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_String, value);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type type, string fieldName, int flags, int flagMask)
		{
			AddNamedArgument(list, type, fieldName, type.Module.universe.System_Boolean, (flags & flagMask) != 0);
		}

		private static void AddNamedArgument(List<CustomAttributeNamedArgument> list, Type attributeType, string fieldName, Type valueType, object value)
		{
			// some fields are not available on the .NET Compact Framework version of DllImportAttribute/MarshalAsAttribute
			FieldInfo field = attributeType.FindField(fieldName, FieldSignature.Create(valueType, new CustomModifiers()));
			if (field != null)
			{
				list.Add(new CustomAttributeNamedArgument(field, new CustomAttributeTypedArgument(valueType, value)));
			}
		}

		internal static CustomAttributeData CreateFieldOffsetPseudoCustomAttribute(Module module, int offset)
		{
			Type type = module.universe.System_Runtime_InteropServices_FieldOffsetAttribute;
			ConstructorInfo constructor = type.GetPseudoCustomAttributeConstructor(module.universe.System_Int32);
			return new CustomAttributeData(module, constructor, new object[] { offset }, null);
		}

		internal static CustomAttributeData CreatePreserveSigPseudoCustomAttribute(Module module)
		{
			Type type = module.universe.System_Runtime_InteropServices_PreserveSigAttribute;
			ConstructorInfo constructor = type.GetPseudoCustomAttributeConstructor();
			return new CustomAttributeData(module, constructor, Empty<object>.Array, null);
		}
	}
}
