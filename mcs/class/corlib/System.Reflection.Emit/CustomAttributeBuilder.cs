
//
// System.Reflection.Emit/CustomAttributeBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public class CustomAttributeBuilder {
		ConstructorInfo ctor;
		byte[] data;

		internal ConstructorInfo Ctor {
			get {return ctor;}
		}

		internal byte[] Data {
			get {return data;}
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern byte[] GetBlob(ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues);
		
		internal CustomAttributeBuilder( ConstructorInfo con, byte[] cdata) {
			ctor = con;
			data = (byte[])cdata.Clone ();
			/* should we check that the user supplied data is correct? */
		}
		
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs)
			: this (con, constructorArgs, null, null, null, null) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
			: this (con, constructorArgs, null, null, namedFields, fieldValues) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
			: this (con, constructorArgs, namedProperties, propertyValues, null, null) {
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues) {
			ctor = con;
			data = GetBlob (con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
		}

		/* helper methods */
		internal static int decode_len (byte[] data, int pos, out int rpos) {
			int len = 0;
			if ((data [pos] & 0x80) == 0) {
				len = (int)(data [pos++] & 0x7f);
			} else if ((data [pos] & 0x40) == 0) {
				len = ((data [pos] & 0x3f) << 8) + data [pos + 1];
				pos += 2;
			} else {
				len = ((data [pos] & 0x1f) << 24) + (data [pos + 1] << 16) + (data [pos + 2] << 8) + data [pos + 3];
				pos += 4;
			}
			rpos = pos;
			return len;
		}

		internal static string string_from_bytes (byte[] data, int pos, int len) 
		{
			return System.Text.Encoding.UTF8.GetString(data, pos, len);
		}

		internal static UnmanagedMarshal get_umarshal (CustomAttributeBuilder customBuilder, bool is_field) {
			byte[] data = customBuilder.Data;
			UnmanagedType subtype = UnmanagedType.I4;
			int sizeConst = 0;
			int value;
			int utype; /* the (stupid) ctor takes a short or an enum ... */
			utype = (int)data [2];
			utype |= ((int)data [3]) << 8;

			string first_type_name = customBuilder.Ctor.GetParameters()[0].ParameterType.FullName;
			int pos = 6;
			if (first_type_name == "System.Int16")
				pos = 4;
			int nnamed = (int)data [pos++];
			nnamed |= ((int)data [pos++]) << 8;
			
			for (int i = 0; i < nnamed; ++i) {
				int paramType; // What is this ?
				paramType = (int)data [pos++];
				paramType |= ((int)data [pos++]) << 8;
				int len = decode_len (data, pos, out pos);
				string named_name = string_from_bytes (data, pos, len);
				pos += len;

				switch (named_name) {
				case "ArraySubType":
					value = (int)data [pos++];
					value |= ((int)data [pos++]) << 8;
					value |= ((int)data [pos++]) << 16;
					value |= ((int)data [pos++]) << 24;
					subtype = (UnmanagedType)value;
					break;
				case "SizeConst":
					value = (int)data [pos++];
					value |= ((int)data [pos++]) << 8;
					value |= ((int)data [pos++]) << 16;
					value |= ((int)data [pos++]) << 24;
					sizeConst = value;
					break;
				default:
					break;
				}
			}

			switch ((UnmanagedType)utype) {
			case UnmanagedType.LPArray:
				return UnmanagedMarshal.DefineLPArray (subtype);
			case UnmanagedType.SafeArray:
				return UnmanagedMarshal.DefineSafeArray (subtype);
			case UnmanagedType.ByValArray:
				return UnmanagedMarshal.DefineByValArray (sizeConst);
			case UnmanagedType.ByValTStr:
				return UnmanagedMarshal.DefineByValTStr (sizeConst);
			default:
				return UnmanagedMarshal.DefineUnmanagedMarshal ((UnmanagedType)utype);
			}
		}

	}
}
