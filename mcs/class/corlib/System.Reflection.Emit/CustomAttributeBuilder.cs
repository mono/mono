
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
		static extern byte[] GetBlob(Assembly asmb, ConstructorInfo con, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues);
		
		internal CustomAttributeBuilder( ConstructorInfo con, byte[] cdata) {
			ctor = con;
			data = (byte[])cdata.Clone ();
			/* should we check that the user supplied data is correct? */
		}
		
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs)
		{
			Initialize (con, constructorArgs, new PropertyInfo [0], new object [0],
					new FieldInfo [0], new object [0]);
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs,
				FieldInfo[] namedFields, object[] fieldValues) 
		{
			Initialize (con, constructorArgs, new PropertyInfo [0], new object [0],
					namedFields, fieldValues);
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs,
				PropertyInfo[] namedProperties, object[] propertyValues)
		{
			Initialize (con, constructorArgs, namedProperties, propertyValues, new FieldInfo [0],
					new object [0]);
		}
		public CustomAttributeBuilder( ConstructorInfo con, object[] constructorArgs,
				PropertyInfo[] namedProperties, object[] propertyValues,
				FieldInfo[] namedFields, object[] fieldValues)
		{
			Initialize (con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
		}

		private bool IsValidType (Type t)
		{
			/* FIXME: Add more checks */
			if (t.IsArray && t.GetArrayRank () > 1)
				return false;
			return true;
		}

		private void Initialize (ConstructorInfo con, object [] constructorArgs,
				PropertyInfo [] namedProperties, object [] propertyValues,
				FieldInfo [] namedFields, object [] fieldValues)
		{
			ctor = con;
			if (con == null)
				throw new ArgumentNullException ("con");
			if (constructorArgs == null)
				throw new ArgumentNullException ("constructorArgs");
			if (namedProperties == null)
				throw new ArgumentNullException ("namedProperties");
			if (propertyValues == null)
				throw new ArgumentNullException ("propertyValues");
			if (namedFields == null)
				throw new ArgumentNullException ("namedFields");
			if (fieldValues == null)
				throw new ArgumentNullException ("fieldValues");
			if (con.GetParameterCount () != constructorArgs.Length)
				throw new ArgumentException ("Parameter count does not match " +
						"passed in argument value count.");
			if (namedProperties.Length != propertyValues.Length)
				throw new ArgumentException ("Array lengths must be the same.",
						"namedProperties, propertyValues");
			if (namedFields.Length != fieldValues.Length)
				throw new ArgumentException ("Array lengths must be the same.",
						"namedFields, fieldValues");
			if ((con.Attributes & MethodAttributes.Static) == MethodAttributes.Static ||
					(con.Attributes & MethodAttributes.Private) == MethodAttributes.Private)
				throw new ArgumentException ("Cannot have private or static constructor.");

			Type atype = ctor.DeclaringType;
			int i;
			i = 0;
			foreach (FieldInfo fi in namedFields) {
				Type t = fi.DeclaringType;
				if (!IsValidType (t))
					throw new ArgumentException ("Field '" + fi.Name + "' does not have a valid type.");
				if ((atype != t) && (!t.IsSubclassOf (atype)) && (!atype.IsSubclassOf (t)))
					throw new ArgumentException ("Field '" + fi.Name + "' does not belong to the same class as the constructor");
				// FIXME: Check enums and TypeBuilders as well
				if (fieldValues [i] != null)
					// IsEnum does not seem to work on TypeBuilders
					if (!(fi.FieldType is TypeBuilder) && !fi.FieldType.IsEnum && !fi.FieldType.IsAssignableFrom (fieldValues [i].GetType ())) {
						//
						// mcs allways uses object[] for array types and
						// MS.NET allows this
						//
						if (!fi.FieldType.IsArray)
							throw new ArgumentException ("Value of field '" + fi.Name + "' does not match field type: " + fi.FieldType);
						}
				i ++;
			}

			i = 0;
			foreach (PropertyInfo pi in namedProperties) {
				if (!pi.CanWrite)
					throw new ArgumentException ("Property '" + pi.Name + "' does not have a setter.");
				Type t = pi.DeclaringType;
				if (!IsValidType (t))
					throw new ArgumentException ("Property '" + pi.Name + "' does not have a valid type.");
				if ((atype != t) && (!t.IsSubclassOf (atype)) && (!atype.IsSubclassOf (t)))
					throw new ArgumentException ("Property '" + pi.Name + "' does not belong to the same class as the constructor");
				if (propertyValues [i] != null) {
					if (!(pi.PropertyType is TypeBuilder) && !pi.PropertyType.IsEnum && !pi.PropertyType.IsAssignableFrom (propertyValues [i].GetType ()))
						if (!pi.PropertyType.IsArray)
							throw new ArgumentException ("Value of property '" + pi.Name + "' does not match property type: " + pi.PropertyType + " -> " + propertyValues [i]);
				}
				i ++;
			}

			i = 0;
			foreach (ParameterInfo pi in con.GetParameters ()) {
				if (pi != null) {
					Type paramType = pi.ParameterType;
					if (!IsValidType (paramType))
						throw new ArgumentException ("Argument " + i + " does not have a valid type.");
					if (constructorArgs [i] != null)
						if (!(paramType is TypeBuilder) && !paramType.IsEnum && !paramType.IsAssignableFrom (constructorArgs [i].GetType ()))
							if (!paramType.IsArray)
								throw new ArgumentException ("Value of argument " + i + " does not match parameter type: " + paramType + " -> " + constructorArgs [i]);
				}
				i ++;
			}
				
			data = GetBlob (atype.Assembly, con, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
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

		internal string string_arg ()
		{
			int pos = 2;
			int len = decode_len (data, pos, out pos);
			return string_from_bytes (data, pos, len);
		}			

		internal static UnmanagedMarshal get_umarshal (CustomAttributeBuilder customBuilder, bool is_field) {
			byte[] data = customBuilder.Data;
			UnmanagedType subtype = UnmanagedType.I4;
			int sizeConst = 0;
			int value;
			int utype; /* the (stupid) ctor takes a short or an enum ... */
			Type marshalTypeRef = null;
			string marshalCookie = String.Empty;
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
				case "MarshalTypeRef":
				case "MarshalType":
					len = decode_len (data, pos, out pos);
					marshalTypeRef = Type.GetType (string_from_bytes (data, pos, len));
					pos += len;
					break;
				case "MarshalCookie":
					len = decode_len (data, pos, out pos);
					marshalCookie = string_from_bytes (data, pos, len);
					pos += len;
					break;
				default:
					len = decode_len(data, pos, out pos);
					string_from_bytes (data, pos, len);
					pos += len;
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
			case UnmanagedType.CustomMarshaler:
				return UnmanagedMarshal.DefineCustom ( marshalTypeRef, marshalCookie, marshalTypeRef.ToString (), Guid.Empty);
			default:
				return UnmanagedMarshal.DefineUnmanagedMarshal ((UnmanagedType)utype);
			}
		}

	}
}
