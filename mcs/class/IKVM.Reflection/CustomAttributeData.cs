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
using System.IO;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection
{
	public sealed class CustomAttributeData
	{
		internal static readonly IList<CustomAttributeData> EmptyList = new List<CustomAttributeData>(0).AsReadOnly();
		private Module module;
		private int index;
		private ConstructorInfo lazyConstructor;
		private IList<CustomAttributeTypedArgument> lazyConstructorArguments;
		private IList<CustomAttributeNamedArgument> lazyNamedArguments;

		internal CustomAttributeData(Module module, int index)
		{
			this.module = module;
			this.index = index;
		}

		internal CustomAttributeData(ConstructorInfo constructor, object[] args, List<CustomAttributeNamedArgument> namedArguments)
		{
			this.lazyConstructor = constructor;
			MethodSignature sig = constructor.MethodSignature;
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>();
			for (int i = 0; i < args.Length; i++)
			{
				list.Add(new CustomAttributeTypedArgument(sig.GetParameterType(i), args[i]));
			}
			lazyConstructorArguments = list.AsReadOnly();
			if (namedArguments == null)
			{
				this.lazyNamedArguments = Empty<CustomAttributeNamedArgument>.Array;
			}
			else
			{
				this.lazyNamedArguments = namedArguments.AsReadOnly();
			}
		}

		internal CustomAttributeData(Assembly asm, ConstructorInfo constructor, ByteReader br)
		{
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
			foreach (CustomAttributeTypedArgument arg in ConstructorArguments)
			{
				sb.Append(sep);
				sep = ", ";
				AppendValue(sb, arg);
			}
			foreach (CustomAttributeNamedArgument named in NamedArguments)
			{
				sb.Append(sep);
				sep = ", ";
				sb.Append(named.MemberInfo.Name);
				sb.Append(" = ");
				AppendValue(sb, named.TypedValue);
			}
			sb.Append(')');
			sb.Append(']');
			return sb.ToString();
		}

		private static void AppendValue(StringBuilder sb, CustomAttributeTypedArgument arg)
		{
			if (arg.ArgumentType == arg.ArgumentType.Module.universe.System_String)
			{
				sb.Append('"').Append(arg.Value).Append('"');
			}
			else
			{
				if (arg.ArgumentType.IsEnum)
				{
					sb.Append('(');
					sb.Append(arg.ArgumentType.FullName);
					sb.Append(')');
				}
				sb.Append(arg.Value);
			}
		}

		internal static void ReadDeclarativeSecurity(Assembly asm, List<CustomAttributeData> list, int action, ByteReader br)
		{
			Universe u = asm.universe;
			if (br.PeekByte() == '.')
			{
				br.ReadByte();
				int count = br.ReadCompressedInt();
				for (int j = 0; j < count; j++)
				{
					Type type = ReadType(asm, br);
					ConstructorInfo constructor;
					if (type == u.System_Security_Permissions_HostProtectionAttribute && action == (int)System.Security.Permissions.SecurityAction.LinkDemand)
					{
						constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
					}
					else
					{
						constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { u.System_Security_Permissions_SecurityAction }, null);
					}
					// LAMESPEC there is an additional length here (probably of the named argument list)
					ByteReader slice = br.Slice(br.ReadCompressedInt());
					// LAMESPEC the count of named arguments is a compressed integer (instead of UInt16 as NumNamed in custom attributes)
					list.Add(new CustomAttributeData(constructor, action, ReadNamedArguments(asm, slice, slice.ReadCompressedInt(), type)));
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
				ConstructorInfo constructor = u.System_Security_Permissions_PermissionSetAttribute.GetConstructor(new Type[] { u.System_Security_Permissions_SecurityAction });
				List<CustomAttributeNamedArgument> args = new List<CustomAttributeNamedArgument>();
				args.Add(new CustomAttributeNamedArgument(u.System_Security_Permissions_PermissionSetAttribute.GetProperty("XML"),
					new CustomAttributeTypedArgument(u.System_String, xml)));
				list.Add(new CustomAttributeData(constructor, action, args));
			}
		}

		private CustomAttributeData(ConstructorInfo constructor, int securityAction, IList<CustomAttributeNamedArgument> namedArguments)
		{
			Universe u = constructor.Module.universe;
			this.lazyConstructor = constructor;
			List<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>();
			list.Add(new CustomAttributeTypedArgument(u.System_Security_Permissions_SecurityAction, securityAction));
			this.lazyConstructorArguments =  list.AsReadOnly();
			this.lazyNamedArguments = namedArguments;
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
					throw new InvalidOperationException();
			}
		}

		private static CustomAttributeTypedArgument ReadFixedArg(Assembly asm, ByteReader br, Type type)
		{
			Universe u = asm.universe;
			if (type == u.System_String)
			{
				return new CustomAttributeTypedArgument(type, br.ReadString());
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
				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Boolean:
						return new CustomAttributeTypedArgument(type, br.ReadByte() != 0);
					case TypeCode.Char:
						return new CustomAttributeTypedArgument(type, br.ReadChar());
					case TypeCode.Single:
						return new CustomAttributeTypedArgument(type, br.ReadSingle());
					case TypeCode.Double:
						return new CustomAttributeTypedArgument(type, br.ReadDouble());
					case TypeCode.SByte:
						return new CustomAttributeTypedArgument(type, br.ReadSByte());
					case TypeCode.Int16:
						return new CustomAttributeTypedArgument(type, br.ReadInt16());
					case TypeCode.Int32:
						return new CustomAttributeTypedArgument(type, br.ReadInt32());
					case TypeCode.Int64:
						return new CustomAttributeTypedArgument(type, br.ReadInt64());
					case TypeCode.Byte:
						return new CustomAttributeTypedArgument(type, br.ReadByte());
					case TypeCode.UInt16:
						return new CustomAttributeTypedArgument(type, br.ReadUInt16());
					case TypeCode.UInt32:
						return new CustomAttributeTypedArgument(type, br.ReadUInt32());
					case TypeCode.UInt64:
						return new CustomAttributeTypedArgument(type, br.ReadUInt64());
					default:
						throw new InvalidOperationException();
				}
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
			return asm.universe.GetType(asm, typeName, true);
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
						member = GetField(type, name);
						break;
					case 0x54:
						member = GetProperty(type, name);
						break;
					default:
						throw new BadImageFormatException();
				}
				if (member == null)
				{
					throw new BadImageFormatException();
				}
				list.Add(new CustomAttributeNamedArgument(member, value));
			}
			return list.AsReadOnly();
		}

		private static FieldInfo GetField(Type type, string name)
		{
			for (; type != null; type = type.BaseType)
			{
				foreach (FieldInfo field in type.__GetDeclaredFields())
				{
					if (field.IsPublic && !field.IsStatic && field.Name == name)
					{
						return field;
					}
				}
			}
			return null;
		}

		private static PropertyInfo GetProperty(Type type, string name)
		{
			for (; type != null; type = type.BaseType)
			{
				foreach (PropertyInfo property in type.__GetDeclaredProperties())
				{
					if (property.IsPublic && !property.IsStatic && property.Name == name)
					{
						return property;
					}
				}
			}
			return null;
		}

		[Obsolete("Use Constructor.DeclaringType instead.")]
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
			return ((ModuleReader)module).GetBlobCopy(module.CustomAttribute.records[index].Value);
		}

		public ConstructorInfo Constructor
		{
			get
			{
				if (lazyConstructor == null)
				{
					lazyConstructor = (ConstructorInfo)module.ResolveMethod(module.CustomAttribute.records[index].Type);
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
					LazyParseArguments();
				}
				return lazyNamedArguments;
			}
		}

		private void LazyParseArguments()
		{
			ByteReader br = module.GetBlob(module.CustomAttribute.records[index].Value);
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
			object[] args = new object[ConstructorArguments.Count];
			for (int i = 0; i < args.Length; i++)
			{
				args[i] = RewrapArray(ConstructorArguments[i]);
			}
			List<PropertyInfo> namedProperties = new List<PropertyInfo>();
			List<object> propertyValues = new List<object>();
			List<FieldInfo> namedFields = new List<FieldInfo>();
			List<object> fieldValues = new List<object>();
			foreach (CustomAttributeNamedArgument named in NamedArguments)
			{
				if (named.MemberInfo is PropertyInfo)
				{
					namedProperties.Add((PropertyInfo)named.MemberInfo);
					propertyValues.Add(RewrapArray(named.TypedValue));
				}
				else
				{
					namedFields.Add((FieldInfo)named.MemberInfo);
					fieldValues.Add(RewrapArray(named.TypedValue));
				}
			}
			return new CustomAttributeBuilder(Constructor, args, namedProperties.ToArray(), propertyValues.ToArray(), namedFields.ToArray(), fieldValues.ToArray());
		}

		private static object RewrapArray(CustomAttributeTypedArgument arg)
		{
			IList<CustomAttributeTypedArgument> list = arg.Value as IList<CustomAttributeTypedArgument>;
			if (list != null)
			{
				object[] arr = new object[list.Count];
				for (int i = 0; i < arr.Length; i++)
				{
					// note that CLI spec only allows one dimensional arrays, so we don't need to rewrap the elements
					arr[i] = list[i].Value;
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
			return member.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Assembly assembly)
		{
			return assembly.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(Module module)
		{
			return module.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> GetCustomAttributes(ParameterInfo parameter)
		{
			return parameter.GetCustomAttributesData(null);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Assembly assembly, Type attributeType, bool inherit)
		{
			return assembly.GetCustomAttributesData(attributeType);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(Module module, Type attributeType, bool inherit)
		{
			return module.GetCustomAttributesData(attributeType);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(ParameterInfo parameter, Type attributeType, bool inherit)
		{
			return parameter.GetCustomAttributesData(attributeType);
		}

		public static IList<CustomAttributeData> __GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
		{
			if (!inherit || !IsInheritableAttribute(attributeType))
			{
				return member.GetCustomAttributesData(attributeType);
			}
			List<CustomAttributeData> list = new List<CustomAttributeData>();
			for (; ; )
			{
				list.AddRange(member.GetCustomAttributesData(attributeType));
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
			IList<CustomAttributeData> attr = attribute.GetCustomAttributesData(attributeUsageAttribute);
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
	}
}
