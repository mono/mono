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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class CustomAttributeBuilder
	{
		private readonly ConstructorInfo con;
		private readonly byte[] blob;
		private readonly object[] constructorArgs;
		private readonly PropertyInfo[] namedProperties;
		private readonly object[] propertyValues;
		private readonly FieldInfo[] namedFields;
		private readonly object[] fieldValues;

		internal CustomAttributeBuilder(ConstructorInfo con, byte[] blob)
		{
			this.con = con;
			this.blob = blob;
		}

		private CustomAttributeBuilder(ConstructorInfo con, int securityAction, byte[] blob)
		{
			this.con = con;
			this.blob = blob;
			this.constructorArgs = new object[] { securityAction };
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs)
			: this(con, constructorArgs, null, null, null,null)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
			: this(con, constructorArgs, null, null, namedFields, fieldValues)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
			: this(con, constructorArgs, namedProperties, propertyValues, null, null)
		{
		}

		public CustomAttributeBuilder(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
		{
			this.con = con;
			this.constructorArgs = constructorArgs;
			this.namedProperties = namedProperties;
			this.propertyValues = propertyValues;
			this.namedFields = namedFields;
			this.fieldValues = fieldValues;
		}

		public static CustomAttributeBuilder __FromBlob(ConstructorInfo con, byte[] blob)
		{
			return new CustomAttributeBuilder(con, blob);
		}

		public static CustomAttributeBuilder __FromBlob(ConstructorInfo con, int securityAction, byte[] blob)
		{
			return new CustomAttributeBuilder(con, securityAction, blob);
		}

		public static CustomAttributeTypedArgument __MakeTypedArgument(Type type, object value)
		{
			return new CustomAttributeTypedArgument(type, value);
		}

		private sealed class BlobWriter
		{
			private readonly Assembly assembly;
			private readonly CustomAttributeBuilder cab;
			private readonly ByteBuffer bb;

			internal BlobWriter(Assembly assembly, CustomAttributeBuilder cab, ByteBuffer bb)
			{
				this.assembly = assembly;
				this.cab = cab;
				this.bb = bb;
			}

			internal void WriteCustomAttributeBlob()
			{
				// prolog
				WriteUInt16(1);
				ParameterInfo[] pi = cab.con.GetParameters();
				for (int i = 0; i < pi.Length; i++)
				{
					WriteFixedArg(pi[i].ParameterType, cab.constructorArgs[i]);
				}
				WriteNamedArguments(false);
			}

			internal void WriteNamedArguments(bool forDeclSecurity)
			{
				// NumNamed
				int named = 0;
				if (cab.namedFields != null)
				{
					named += cab.namedFields.Length;
				}
				if (cab.namedProperties != null)
				{
					named += cab.namedProperties.Length;
				}
				if (forDeclSecurity)
				{
					WritePackedLen(named);
				}
				else
				{
					WriteUInt16((ushort)named);
				}
				if (cab.namedFields != null)
				{
					for (int i = 0; i < cab.namedFields.Length; i++)
					{
						WriteNamedArg(0x53, cab.namedFields[i].FieldType, cab.namedFields[i].Name, cab.fieldValues[i]);
					}
				}
				if (cab.namedProperties != null)
				{
					for (int i = 0; i < cab.namedProperties.Length; i++)
					{
						WriteNamedArg(0x54, cab.namedProperties[i].PropertyType, cab.namedProperties[i].Name, cab.propertyValues[i]);
					}
				}
			}

			private void WriteNamedArg(byte fieldOrProperty, Type type, string name, object value)
			{
				WriteByte(fieldOrProperty);
				WriteFieldOrPropType(type);
				WriteString(name);
				WriteFixedArg(type, value);
			}

			private void WriteByte(byte value)
			{
				bb.Write(value);
			}

			private void WriteUInt16(ushort value)
			{
				bb.Write(value);
			}

			private void WriteInt32(int value)
			{
				bb.Write(value);
			}

			private void WriteFixedArg(Type type, object value)
			{
				Universe u = assembly.universe;
				if (type == u.System_String)
				{
					WriteString((string)value);
				}
				else if (type == u.System_Boolean)
				{
					WriteByte((bool)value ? (byte)1 : (byte)0);
				}
				else if (type == u.System_Char)
				{
					WriteUInt16((char)value);
				}
				else if (type == u.System_SByte)
				{
					WriteByte((byte)(sbyte)value);
				}
				else if (type == u.System_Byte)
				{
					WriteByte((byte)value);
				}
				else if (type == u.System_Int16)
				{
					WriteUInt16((ushort)(short)value);
				}
				else if (type == u.System_UInt16)
				{
					WriteUInt16((ushort)value);
				}
				else if (type == u.System_Int32)
				{
					WriteInt32((int)value);
				}
				else if (type == u.System_UInt32)
				{
					WriteInt32((int)(uint)value);
				}
				else if (type == u.System_Int64)
				{
					WriteInt64((long)value);
				}
				else if (type == u.System_UInt64)
				{
					WriteInt64((long)(ulong)value);
				}
				else if (type == u.System_Single)
				{
					WriteSingle((float)value);
				}
				else if (type == u.System_Double)
				{
					WriteDouble((double)value);
				}
				else if (type == u.System_Type)
				{
					WriteTypeName((Type)value);
				}
				else if (type == u.System_Object)
				{
					if (value == null)
					{
						type = u.System_String;
					}
					else if (value is Type)
					{
						// value.GetType() would return a subclass of Type, but we don't want to deal with that
						type = u.System_Type;
					}
					else if (value is CustomAttributeTypedArgument)
					{
						CustomAttributeTypedArgument cta = (CustomAttributeTypedArgument)value;
						value = cta.Value;
						type = cta.ArgumentType;
					}
					else
					{
						type = u.Import(value.GetType());
					}
					WriteFieldOrPropType(type);
					WriteFixedArg(type, value);
				}
				else if (type.IsArray)
				{
					if (value == null)
					{
						WriteInt32(-1);
					}
					else
					{
						Array array = (Array)value;
						Type elemType = type.GetElementType();
						WriteInt32(array.Length);
						foreach (object val in array)
						{
							WriteFixedArg(elemType, val);
						}
					}
				}
				else if (type.IsEnum)
				{
					WriteFixedArg(type.GetEnumUnderlyingTypeImpl(), value);
				}
				else
				{
					throw new ArgumentException();
				}
			}

			private void WriteInt64(long value)
			{
				bb.Write(value);
			}

			private void WriteSingle(float value)
			{
				bb.Write(value);
			}

			private void WriteDouble(double value)
			{
				bb.Write(value);
			}

			private void WriteTypeName(Type type)
			{
				string name = null;
				if (type != null)
				{
					StringBuilder sb = new StringBuilder();
					GetTypeName(sb, type, false);
					name = sb.ToString();
				}
				WriteString(name);
			}

			private void GetTypeName(StringBuilder sb, Type type, bool isTypeParam)
			{
				bool v1 = !assembly.ManifestModule.__IsMissing && assembly.ManifestModule.MDStreamVersion < 0x20000;
				bool includeAssemblyName = type.Assembly != assembly && (!v1 || type.Assembly != type.Module.universe.Mscorlib);
				if (isTypeParam && includeAssemblyName)
				{
					sb.Append('[');
				}
				GetTypeNameImpl(sb, type);
				if (includeAssemblyName)
				{
					if (v1)
					{
						sb.Append(',');
					}
					else
					{
						sb.Append(", ");
					}
					if (isTypeParam)
					{
						sb.Append(type.Assembly.FullName.Replace("]", "\\]")).Append(']');
					}
					else
					{
						sb.Append(type.Assembly.FullName);
					}
				}
			}

			private void GetTypeNameImpl(StringBuilder sb, Type type)
			{
				if (type.HasElementType)
				{
					GetTypeNameImpl(sb, type.GetElementType());
					sb.Append(((ElementHolderType)type).GetSuffix());
				}
				else if (type.IsConstructedGenericType)
				{
					sb.Append(type.GetGenericTypeDefinition().FullName);
					sb.Append('[');
					string sep = "";
					foreach (Type typeParam in type.GetGenericArguments())
					{
						sb.Append(sep);
						GetTypeName(sb, typeParam, true);
						sep = ",";
					}
					sb.Append(']');
				}
				else
				{
					sb.Append(type.FullName);
				}
			}
	
			private void WriteString(string val)
			{
				bb.Write(val);
			}

			private void WritePackedLen(int len)
			{
				bb.WriteCompressedUInt(len);
			}

			private void WriteFieldOrPropType(Type type)
			{
				Universe u = type.Module.universe;
				if (type == u.System_Type)
				{
					WriteByte(0x50);
				}
				else if (type == u.System_Object)
				{
					WriteByte(0x51);
				}
				else if (type == u.System_Boolean)
				{
					WriteByte(0x02);
				}
				else if (type == u.System_Char)
				{
					WriteByte(0x03);
				}
				else if (type == u.System_SByte)
				{
					WriteByte(0x04);
				}
				else if (type == u.System_Byte)
				{
					WriteByte(0x05);
				}
				else if (type == u.System_Int16)
				{
					WriteByte(0x06);
				}
				else if (type == u.System_UInt16)
				{
					WriteByte(0x07);
				}
				else if (type == u.System_Int32)
				{
					WriteByte(0x08);
				}
				else if (type == u.System_UInt32)
				{
					WriteByte(0x09);
				}
				else if (type == u.System_Int64)
				{
					WriteByte(0x0A);
				}
				else if (type == u.System_UInt64)
				{
					WriteByte(0x0B);
				}
				else if (type == u.System_Single)
				{
					WriteByte(0x0C);
				}
				else if (type == u.System_Double)
				{
					WriteByte(0x0D);
				}
				else if (type == u.System_String)
				{
					WriteByte(0x0E);
				}
				else if (type.IsArray)
				{
					WriteByte(0x1D);
					WriteFieldOrPropType(type.GetElementType());
				}
				else if (type.IsEnum)
				{
					WriteByte(0x55);
					WriteTypeName(type);
				}
				else
				{
					throw new ArgumentException();
				}
			}
		}

		internal bool IsPseudoCustomAttribute
		{
			get { return con.DeclaringType.IsPseudoCustomAttribute; }
		}

		internal ConstructorInfo Constructor
		{
			get { return con; }
		}

		internal int WriteBlob(ModuleBuilder moduleBuilder)
		{
			ByteBuffer bb;
			if (blob != null)
			{
				bb = ByteBuffer.Wrap(blob);
			}
			else
			{
				bb = new ByteBuffer(100);
				BlobWriter bw = new BlobWriter(moduleBuilder.Assembly, this, bb);
				bw.WriteCustomAttributeBlob();
			}
			return moduleBuilder.Blobs.Add(bb);
		}

		internal object GetConstructorArgument(int pos)
		{
			return constructorArgs[pos];
		}

		internal int ConstructorArgumentCount
		{
			get { return constructorArgs == null ? 0 : constructorArgs.Length; }
		}

		internal T? GetFieldValue<T>(string name) where T : struct
		{
			object val = GetFieldValue(name);
			if (val is T)
			{
				return (T)val;
			}
			else if (val != null)
			{
				if (typeof(T).IsEnum)
				{
					Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == val.GetType());
					return (T)Enum.ToObject(typeof(T), val);
				}
				else
				{
					Debug.Assert(Enum.GetUnderlyingType(val.GetType()) == typeof(T));
					return (T)Convert.ChangeType(val, typeof(T));
				}
			}
			else
			{
				return null;
			}
		}

		internal object GetFieldValue(string name)
		{
			if (namedFields != null)
			{
				for (int i = 0; i < namedFields.Length; i++)
				{
					if (namedFields[i].Name == name)
					{
						return fieldValues[i];
					}
				}
			}
			return null;
		}

		internal string GetLegacyDeclSecurity()
		{
			if (con.DeclaringType == con.Module.universe.System_Security_Permissions_PermissionSetAttribute
				&& blob == null
				&& (namedFields == null || namedFields.Length == 0)
				&& namedProperties != null
				&& namedProperties.Length == 1
				&& namedProperties[0].Name == "XML")
			{
				return propertyValues[0] as string;
			}
			return null;
		}

		internal void WriteNamedArgumentsForDeclSecurity(ModuleBuilder moduleBuilder, ByteBuffer bb)
		{
			if (blob != null)
			{
				bb.Write(blob);
			}
			else
			{
				BlobWriter bw = new BlobWriter(moduleBuilder.Assembly, this, bb);
				bw.WriteNamedArguments(true);
			}
		}

		internal CustomAttributeData ToData(Assembly asm)
		{
			if (blob != null)
			{
				if (constructorArgs != null)
				{
					return new CustomAttributeData(asm, con, (int)constructorArgs[0], blob, -1);
				}
				return new CustomAttributeData(asm, con, new IKVM.Reflection.Reader.ByteReader(blob, 0, blob.Length));
			}
			else
			{
				List<CustomAttributeNamedArgument> namedArgs = new List<CustomAttributeNamedArgument>();
				if (namedProperties != null)
				{
					for (int i = 0; i < namedProperties.Length; i++)
					{
						namedArgs.Add(new CustomAttributeNamedArgument(namedProperties[i], RewrapValue(namedProperties[i].PropertyType, propertyValues[i])));
					}
				}
				if (namedFields != null)
				{
					for (int i = 0; i < namedFields.Length; i++)
					{
						namedArgs.Add(new CustomAttributeNamedArgument(namedFields[i], RewrapValue(namedFields[i].FieldType, fieldValues[i])));
					}
				}
				List<CustomAttributeTypedArgument> args = new List<CustomAttributeTypedArgument>(constructorArgs.Length);
				ParameterInfo[] parameters = this.Constructor.GetParameters();
				for (int i = 0; i < constructorArgs.Length; i++)
				{
					args.Add(RewrapValue(parameters[i].ParameterType, constructorArgs[i]));
				}
				return new CustomAttributeData(asm.ManifestModule, con, args, namedArgs);
			}
		}

		private static CustomAttributeTypedArgument RewrapValue(Type type, object value)
		{
			if (value is Array)
			{
				Array array = (Array)value;
				Type arrayType = type.Module.universe.Import(array.GetType());
				return RewrapArray(arrayType, array);
			}
			else if (value is CustomAttributeTypedArgument)
			{
				CustomAttributeTypedArgument arg = (CustomAttributeTypedArgument)value;
				if (arg.Value is Array)
				{
					return RewrapArray(arg.ArgumentType, (Array)arg.Value);
				}
				return arg;
			}
			else
			{
				return new CustomAttributeTypedArgument(type, value);
			}
		}

		private static CustomAttributeTypedArgument RewrapArray(Type arrayType, Array array)
		{
			Type elementType = arrayType.GetElementType();
			CustomAttributeTypedArgument[] newArray = new CustomAttributeTypedArgument[array.Length];
			for (int i = 0; i < newArray.Length; i++)
			{
				newArray[i] = RewrapValue(elementType, array.GetValue(i));
			}
			return new CustomAttributeTypedArgument(arrayType, newArray);
		}

		internal bool HasBlob
		{
			get { return blob != null; }
		}

		internal CustomAttributeBuilder DecodeBlob(Assembly asm)
		{
			if (blob == null)
			{
				return this;
			}
			else
			{
				return ToData(asm).__ToBuilder();
			}
		}

		internal byte[] GetBlob(Assembly asm)
		{
			ByteBuffer bb = new ByteBuffer(100);
			BlobWriter bw = new BlobWriter(asm, this, bb);
			bw.WriteCustomAttributeBlob();
			return bb.ToArray();
		}
	}
}
