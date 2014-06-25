//
// System.TypeUtil class
//
// Author:
//	Matthew Leibowitz <matthew.leibowitz@xamarin.com>
//
// Copyright (C) 2004-2005,2009 Novell, Inc (http://www.novell.com)
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

#if NETFX_CORE
using System.Collections.Generic;
using System.Linq;
using IConvertible = System.Object;
#endif
using System.Reflection;

namespace System {

	static class TypeUtil {
	
		private static readonly Type[] conversionTable = {
			// Valid ICovnertible Types
			null,		       //  0 empty
			typeof (object),   //  1 TypeCode.Object
			typeof (DBNull),   //  2 TypeCode.DBNull
			typeof (Boolean),  //  3 TypeCode.Boolean
			typeof (Char),	   //  4 TypeCode.Char
			typeof (SByte),	   //  5 TypeCode.SByte
			typeof (Byte),	   //  6 TypeCode.Byte
			typeof (Int16),	   //  7 TypeCode.Int16
			typeof (UInt16),   //  8 TypeCode.UInt16
			typeof (Int32),	   //  9 TypeCode.Int32
			typeof (UInt32),   // 10 TypeCode.UInt32
			typeof (Int64),	   // 11 TypeCode.Int64
			typeof (UInt64),   // 12 TypeCode.UInt64
			typeof (Single),   // 13 TypeCode.Single
			typeof (Double),   // 14 TypeCode.Double
			typeof (Decimal),  // 15 TypeCode.Decimal
			typeof (DateTime), // 16 TypeCode.DateTime
			null,			   // 17 null.
			typeof (String),   // 18 TypeCode.String
			typeof (Enum)
		};

		internal static object CreateInstance (Type type) {
#if !WINDOWS_PHONE && !NETFX_CORE
			return Activator.CreateInstance (type, true);
#else
			return Activator.CreateInstance (type);
#endif
		}

		internal static object ChangeType (object value, TypeCode typeCode, IFormatProvider provider)
		{
#if NETFX_CORE
			Type type = conversionTable [(int)typeCode];
			return Convert.ChangeType (value, type, provider);
#else
			return Convert.ChangeType (value, typeCode, provider);
#endif
		}
		
		internal static TypeCode GetTypeCode (Type type)
		{
#if NETFX_CORE
			if (type == null) return TypeCode.Empty;

			if (type == typeof (System.DBNull)) return TypeCode.DBNull;
			if (type == typeof (System.Boolean)) return TypeCode.Boolean;
			if (type == typeof (System.Char)) return TypeCode.Char;
			if (type == typeof (System.SByte)) return TypeCode.SByte;
			if (type == typeof (System.Byte)) return TypeCode.Byte;
			if (type == typeof (System.Int16)) return TypeCode.Int16;
			if (type == typeof (System.UInt16)) return TypeCode.UInt16;
			if (type == typeof (System.Int32)) return TypeCode.Int32;
			if (type == typeof (System.UInt32)) return TypeCode.UInt32;
			if (type == typeof (System.Int64)) return TypeCode.Int64;
			if (type == typeof (System.UInt64)) return TypeCode.UInt64;
			if (type == typeof (System.Single)) return TypeCode.Single;
			if (type == typeof (System.Double)) return TypeCode.Double;
			if (type == typeof (System.Decimal)) return TypeCode.Decimal;
			if (type == typeof (System.DateTime)) return TypeCode.DateTime;
			if (type == typeof (System.String)) return TypeCode.String;

			return TypeCode.Object;
#else
			return Type.GetTypeCode (type);
#endif
		}

		internal static Assembly GetAssembly (this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo ().Assembly;
#else
			return type.Assembly;
#endif
		}

		internal static bool GetIsValueType (this Type type)
		{
#if NETFX_CORE
			return type.GetTypeInfo ().IsValueType;
#else
			return type.IsValueType;
#endif
		}
		
		// Extension methods to provide WinRT with slightly more .NET like methods
#if NETFX_CORE
		internal static TypeCode GetTypeCode (this object obj)
		{
			if (obj == null) 
				return TypeCode.DBNull;
			return GetTypeCode (obj.GetType ());
		}
		
		internal static bool IsSubclassOf (this Type type, Type type2)
		{
			return type.GetTypeInfo ().IsSubclassOf (type2);
		}

		internal static bool IsAssignableFrom (this Type type, Type type2)
		{
			return type.GetTypeInfo ().IsAssignableFrom (type2.GetTypeInfo ());
		}

		internal static bool IsInstanceOfType (this Type type, object value)
		{
			return type.GetTypeInfo ().IsAssignableFrom (value.GetType ().GetTypeInfo ());
		}

		internal static MethodInfo GetMethod (this Type type, string methodName)
		{
			return type.GetTypeInfo ().GetDeclaredMethod (methodName);
		}

		internal static MethodInfo GetMethod (this Type type, string methodName, Type[] types)
		{
			IEnumerable<MethodInfo> methods = type.GetTypeInfo ().GetDeclaredMethods (methodName);
			return methods.First (m => {
				ParameterInfo[] parameters = m.GetParameters ();
				if (parameters.Length != types.Length)
					return false;
				Type[] parameterTypes = parameters.Select (p => p.ParameterType).ToArray ();
				for (int i = 0; i < parameters.Length; i++) {
					if (parameterTypes [i] != types [i])
						return false;
				}
				return true;
			});
		}
#else
		internal static MethodInfo GetMethodInfo (this Delegate d)
		{
			return d.Method;
		}
#endif
	}
}
