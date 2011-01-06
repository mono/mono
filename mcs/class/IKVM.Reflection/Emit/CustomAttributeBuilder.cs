/*
  Copyright (C) 2008 Jeroen Frijters

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

		private sealed class BlobWriter
		{
			private readonly ModuleBuilder moduleBuilder;
			private readonly CustomAttributeBuilder cab;
			private readonly ByteBuffer bb;

			internal BlobWriter(ModuleBuilder moduleBuilder, CustomAttributeBuilder cab, ByteBuffer bb)
			{
				this.moduleBuilder = moduleBuilder;
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
				Universe u = moduleBuilder.universe;
				if (type == u.System_String)
				{
					WriteString((string)value);
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
					switch (Type.GetTypeCode(type))
					{
						case TypeCode.Boolean:
							WriteByte((bool)value ? (byte)1 : (byte)0);
							break;
						case TypeCode.Char:
							WriteUInt16((char)value);
							break;
						case TypeCode.SByte:
							WriteByte((byte)(sbyte)value);
							break;
						case TypeCode.Byte:
							WriteByte((byte)value);
							break;
						case TypeCode.Int16:
							WriteUInt16((ushort)(short)value);
							break;
						case TypeCode.UInt16:
							WriteUInt16((ushort)value);
							break;
						case TypeCode.Int32:
							WriteInt32((int)value);
							break;
						case TypeCode.UInt32:
							WriteInt32((int)(uint)value);
							break;
						case TypeCode.Int64:
							WriteInt64((long)value);
							break;
						case TypeCode.UInt64:
							WriteInt64((long)(ulong)value);
							break;
						case TypeCode.Single:
							WriteSingle((float)value);
							break;
						case TypeCode.Double:
							WriteDouble((double)value);
							break;
						default:
							throw new ArgumentException();
					}
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
					if (type.Assembly == moduleBuilder.Assembly)
					{
						name = type.FullName;
					}
					else
					{
						name = type.AssemblyQualifiedName;
					}
				}
				WriteString(name);
			}

			private void WriteString(string val)
			{
				bb.Write(val);
			}

			private void WritePackedLen(int len)
			{
				bb.WriteCompressedInt(len);
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
					switch (Type.GetTypeCode(type))
					{
						case TypeCode.Boolean:
							WriteByte(0x02);
							break;
						case TypeCode.Char:
							WriteByte(0x03);
							break;
						case TypeCode.SByte:
							WriteByte(0x04);
							break;
						case TypeCode.Byte:
							WriteByte(0x05);
							break;
						case TypeCode.Int16:
							WriteByte(0x06);
							break;
						case TypeCode.UInt16:
							WriteByte(0x07);
							break;
						case TypeCode.Int32:
							WriteByte(0x08);
							break;
						case TypeCode.UInt32:
							WriteByte(0x09);
							break;
						case TypeCode.Int64:
							WriteByte(0x0A);
							break;
						case TypeCode.UInt64:
							WriteByte(0x0B);
							break;
						case TypeCode.Single:
							WriteByte(0x0C);
							break;
						case TypeCode.Double:
							WriteByte(0x0D);
							break;
						case TypeCode.String:
							WriteByte(0x0E);
							break;
						default:
							throw new ArgumentException();
					}
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
			ByteBuffer bb = new ByteBuffer(100);
			if (blob != null)
			{
				bb.Write(blob);
			}
			else
			{
				BlobWriter bw = new BlobWriter(moduleBuilder, this, bb);
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

		internal void WriteNamedArgumentsForDeclSecurity(ModuleBuilder moduleBuilder, ByteBuffer bb)
		{
			BlobWriter bw = new BlobWriter(moduleBuilder, this, bb);
			bw.WriteNamedArguments(true);
		}

		internal CustomAttributeData ToData(Assembly asm)
		{
			if (blob != null)
			{
				return new CustomAttributeData(asm, con, new IKVM.Reflection.Reader.ByteReader(blob, 0, blob.Length));
			}
			else
			{
				List<CustomAttributeNamedArgument> namedArgs = new List<CustomAttributeNamedArgument>();
				if (namedProperties != null)
				{
					for (int i = 0; i < namedProperties.Length; i++)
					{
						namedArgs.Add(new CustomAttributeNamedArgument(namedProperties[i], new CustomAttributeTypedArgument(namedProperties[i].PropertyType, propertyValues[i])));
					}
				}
				if (namedFields != null)
				{
					for (int i = 0; i < namedFields.Length; i++)
					{
						namedArgs.Add(new CustomAttributeNamedArgument(namedFields[i], new CustomAttributeTypedArgument(namedFields[i].FieldType, fieldValues[i])));
					}
				}
				return new CustomAttributeData(con, constructorArgs, namedArgs);
			}
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
	}
}
