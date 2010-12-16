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
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;
using IKVM.Reflection.Writer;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection
{
	static class MarshalSpec
	{
		private const UnmanagedType NATIVE_TYPE_MAX = (UnmanagedType)0x50;

		internal static CustomAttributeData GetMarshalAsAttribute(Module module, int token)
		{
			// TODO use binary search?
			for (int i = 0; i < module.FieldMarshal.records.Length; i++)
			{
				if (module.FieldMarshal.records[i].Parent == token)
				{
					ByteReader blob = module.GetBlob(module.FieldMarshal.records[i].NativeType);
					UnmanagedType unmanagedType = (UnmanagedType)blob.ReadCompressedInt();
					UnmanagedType? arraySubType = null;
					short? sizeParamIndex = null;
					int? sizeConst = null;
					VarEnum? safeArraySubType = null;
					Type safeArrayUserDefinedSubType = null;
					int? iidParameterIndex = null;
					string marshalType = null;
					string marshalCookie = null;
					Type marshalTypeRef = null;
					if (unmanagedType == UnmanagedType.LPArray)
					{
						arraySubType = (UnmanagedType)blob.ReadCompressedInt();
						if (arraySubType == NATIVE_TYPE_MAX)
						{
							arraySubType = null;
						}
						if (blob.Length != 0)
						{
							sizeParamIndex = (short)blob.ReadCompressedInt();
							if (blob.Length != 0)
							{
								sizeConst = blob.ReadCompressedInt();
								if (blob.Length != 0 && blob.ReadCompressedInt() == 0)
								{
									sizeParamIndex = null;
								}
							}
						}
					}
					else if (unmanagedType == UnmanagedType.SafeArray)
					{
						if (blob.Length != 0)
						{
							safeArraySubType = (VarEnum)blob.ReadCompressedInt();
							if (blob.Length != 0)
							{
								safeArrayUserDefinedSubType = ReadType(module, blob);
							}
						}
					}
					else if (unmanagedType == UnmanagedType.ByValArray)
					{
						sizeConst = blob.ReadCompressedInt();
						if (blob.Length != 0)
						{
							arraySubType = (UnmanagedType)blob.ReadCompressedInt();
						}
					}
					else if (unmanagedType == UnmanagedType.ByValTStr)
					{
						sizeConst = blob.ReadCompressedInt();
					}
					else if (unmanagedType == UnmanagedType.Interface
						|| unmanagedType == UnmanagedType.IDispatch
						|| unmanagedType == UnmanagedType.IUnknown)
					{
						if (blob.Length != 0)
						{
							iidParameterIndex = blob.ReadCompressedInt();
						}
					}
					else if (unmanagedType == UnmanagedType.CustomMarshaler)
					{
						blob.ReadCompressedInt();
						blob.ReadCompressedInt();
						marshalType = ReadString(blob);
						marshalCookie = ReadString(blob);
						marshalTypeRef = module.Assembly.GetType(marshalType) ?? module.universe.GetType(marshalType);
					}

					Type typeofMarshalAs = module.universe.System_Runtime_InteropServices_MarshalAsAttribute;
					Type typeofUnmanagedType = module.universe.System_Runtime_InteropServices_UnmanagedType;
					Type typeofVarEnum = module.universe.System_Runtime_InteropServices_VarEnum;
					Type typeofType = module.universe.System_Type;
					List<CustomAttributeNamedArgument> named = new List<CustomAttributeNamedArgument>();
					if (arraySubType != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("ArraySubType"), new CustomAttributeTypedArgument(typeofUnmanagedType, arraySubType.Value)));
					}
					if (sizeParamIndex != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("SizeParamIndex"), new CustomAttributeTypedArgument(module.universe.System_Int16, sizeParamIndex.Value)));
					}
					if (sizeConst != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("SizeConst"), new CustomAttributeTypedArgument(module.universe.System_Int32, sizeConst.Value)));
					}
					if (safeArraySubType != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("SafeArraySubType"), new CustomAttributeTypedArgument(typeofVarEnum, safeArraySubType.Value)));
					}
					if (safeArrayUserDefinedSubType != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("SafeArrayUserDefinedSubType"), new CustomAttributeTypedArgument(typeofType, safeArrayUserDefinedSubType)));
					}
					if (iidParameterIndex != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("IidParameterIndex"), new CustomAttributeTypedArgument(module.universe.System_Int32, iidParameterIndex.Value)));
					}
					if (marshalType != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("MarshalType"), new CustomAttributeTypedArgument(module.universe.System_String, marshalType)));
					}
					if (marshalTypeRef != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("MarshalTypeRef"), new CustomAttributeTypedArgument(module.universe.System_Type, marshalTypeRef)));
					}
					if (marshalCookie != null)
					{
						named.Add(new CustomAttributeNamedArgument(typeofMarshalAs.GetField("MarshalCookie"), new CustomAttributeTypedArgument(module.universe.System_String, marshalCookie)));
					}
					ConstructorInfo constructor = typeofMarshalAs.GetConstructor(new Type[] { typeofUnmanagedType });
					return new CustomAttributeData(constructor, new object[] { unmanagedType }, named);
				}
			}
			throw new BadImageFormatException();
		}

		internal static void SetMarshalAsAttribute(ModuleBuilder module, int token, CustomAttributeBuilder attribute)
		{
			attribute = attribute.DecodeBlob(module.Assembly);
			FieldMarshalTable.Record rec = new FieldMarshalTable.Record();
			rec.Parent = token;
			rec.NativeType = WriteMarshallingDescriptor(module, attribute);
			module.FieldMarshal.AddRecord(rec);
		}

		private static int WriteMarshallingDescriptor(ModuleBuilder module, CustomAttributeBuilder attribute)
		{
			UnmanagedType unmanagedType;
			object val = attribute.GetConstructorArgument(0);
			if (val is short)
			{
				unmanagedType = (UnmanagedType)(short)val;
			}
			else if (val is int)
			{
				unmanagedType = (UnmanagedType)(int)val;
			}
			else
			{
				unmanagedType = (UnmanagedType)val;
			}

			ByteBuffer bb = new ByteBuffer(5);
			bb.WriteCompressedInt((int)unmanagedType);

			if (unmanagedType == UnmanagedType.LPArray)
			{
				UnmanagedType arraySubType = attribute.GetFieldValue<UnmanagedType>("ArraySubType") ?? NATIVE_TYPE_MAX;
				bb.WriteCompressedInt((int)arraySubType);
				int? sizeParamIndex = attribute.GetFieldValue<short>("SizeParamIndex");
				int? sizeConst = attribute.GetFieldValue<int>("SizeConst");
				if (sizeParamIndex != null)
				{
					bb.WriteCompressedInt(sizeParamIndex.Value);
					if (sizeConst != null)
					{
						bb.WriteCompressedInt(sizeConst.Value);
						bb.WriteCompressedInt(1); // flag that says that SizeParamIndex was specified
					}
				}
				else if (sizeConst != null)
				{
					bb.WriteCompressedInt(0); // SizeParamIndex
					bb.WriteCompressedInt(sizeConst.Value);
					bb.WriteCompressedInt(0); // flag that says that SizeParamIndex was not specified
				}
			}
			else if (unmanagedType == UnmanagedType.SafeArray)
			{
				VarEnum? safeArraySubType = attribute.GetFieldValue<VarEnum>("SafeArraySubType");
				if (safeArraySubType != null)
				{
					bb.WriteCompressedInt((int)safeArraySubType);
					Type safeArrayUserDefinedSubType = (Type)attribute.GetFieldValue("SafeArrayUserDefinedSubType");
					if (safeArrayUserDefinedSubType != null)
					{
						WriteType(module, bb, safeArrayUserDefinedSubType);
					}
				}
			}
			else if (unmanagedType == UnmanagedType.ByValArray)
			{
				bb.WriteCompressedInt(attribute.GetFieldValue<int>("SizeConst") ?? 1);
				UnmanagedType? arraySubType = attribute.GetFieldValue<UnmanagedType>("ArraySubType");
				if (arraySubType != null)
				{
					bb.WriteCompressedInt((int)arraySubType);
				}
			}
			else if (unmanagedType == UnmanagedType.ByValTStr)
			{
				bb.WriteCompressedInt(attribute.GetFieldValue<int>("SizeConst").Value);
			}
			else if (unmanagedType == UnmanagedType.Interface
				|| unmanagedType == UnmanagedType.IDispatch
				|| unmanagedType == UnmanagedType.IUnknown)
			{
				int? iidParameterIndex = attribute.GetFieldValue<int>("IidParameterIndex");
				if (iidParameterIndex != null)
				{
					bb.WriteCompressedInt(iidParameterIndex.Value);
				}
			}
			else if (unmanagedType == UnmanagedType.CustomMarshaler)
			{
				bb.WriteCompressedInt(0);
				bb.WriteCompressedInt(0);
				string marshalType = (string)attribute.GetFieldValue("MarshalType");
				if (marshalType != null)
				{
					WriteString(bb, marshalType);
				}
				else
				{
					WriteType(module, bb, (Type)attribute.GetFieldValue("MarshalTypeRef"));
				}
				WriteString(bb, (string)attribute.GetFieldValue("MarshalCookie") ?? "");
			}

			return module.Blobs.Add(bb);
		}

		private static Type ReadType(Module module, ByteReader br)
		{
			string str = ReadString(br);
			if (str == "")
			{
				return null;
			}
			return module.Assembly.GetType(str) ?? module.universe.GetType(str, true);
		}

		private static void WriteType(Module module, ByteBuffer bb, Type type)
		{
			WriteString(bb, type.Assembly == module.Assembly ? type.FullName : type.AssemblyQualifiedName);
		}

		private static string ReadString(ByteReader br)
		{
			return Encoding.UTF8.GetString(br.ReadBytes(br.ReadCompressedInt()));
		}

		private static void WriteString(ByteBuffer bb, string str)
		{
			byte[] buf = Encoding.UTF8.GetBytes(str);
			bb.WriteCompressedInt(buf.Length);
			bb.Write(buf);
		}
	}
}
