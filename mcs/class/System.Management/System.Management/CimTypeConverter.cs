//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Management
{
	internal static class CimTypeConverter
	{
		private static Dictionary<Type, CimType> _dotNetTypeToScalarCimType;

		
		static CimTypeConverter()
		{
			CimTypeConverter.InitializeDotNetTypeToCimTypeDictionaries();
		}
		
		public static CimType GetCimType(Type dotNetType)
		{
			CimType cimType = CimType.None;
			if (dotNetType != null)
			{
				if (!CimTypeConverter._dotNetTypeToScalarCimType.TryGetValue(dotNetType, out cimType))
				{
					return CimType.None;
				}
				else
				{
					return cimType;
				}
			}
			else
			{
				throw new ArgumentNullException("dotNetType");
			}
		}
		
		private static CimType GetCimTypeFromDotNetValue(object dotNetValue)
		{
			if (dotNetValue != null)
			{
				CimType cimType = CimTypeConverter.GetCimType(dotNetValue.GetType());
				if (cimType != CimType.None)
				{
					return cimType;
				}
			}

			return CimType.None;
		}
		
		internal static CimType GetCimTypeFromDotNetValueOrThrowAnException(object dotNetValue)
		{
			CimType cimTypeFromDotNetValue = CimTypeConverter.GetCimTypeFromDotNetValue(dotNetValue);
			if (cimTypeFromDotNetValue != CimType.None)
			{
				return cimTypeFromDotNetValue;
			}
			else
			{
				throw new ArgumentException("DotNetValueToCimTypeConversionNotPossible");
			}
		}
		
		public static Type GetDotNetType (CimType cimType)
		{
			CimType cimType1 = cimType;
			switch (cimType1) {
			case CimType.None:
				{
					return null;
				}
			case CimType.Boolean:
				{
					return typeof(bool);
				}
			case CimType.UInt8:
				{
					return typeof(byte);
				}
			case CimType.SInt8:
				{
					return typeof(sbyte);
				}
			case CimType.UInt16:
				{
					return typeof(ushort);
				}
			case CimType.SInt16:
				{
					return typeof(short);
				}
			case CimType.UInt32:
				{
					return typeof(int);
				}
			case CimType.SInt32:
				{
					return typeof(int);
				}
			case CimType.UInt64:
				{
					return typeof(ulong);
				}
			case CimType.SInt64:
				{
					return typeof(long);
				}
			case CimType.Real32:
				{
					return typeof(float);
				}
			case CimType.Real64:
				{
					return typeof(double);
				}
			case CimType.Char16:
				{
					return typeof(char);
				}
			case CimType.DateTime:
				{
					return null;
				}
			case CimType.String:
				{
					return typeof(string);
				}
			case CimType.Reference:
				{
					return typeof(object);
				}
			}
			return null;
		}

		public static int GetMiType(Type type)
		{
			if (type.IsArray) {
				return 0x2000;
			}
			return (int)CimTypeConverter.GetCimType(type);
		}
		
		private static void InitializeDotNetTypeToCimTypeDictionaries()
		{
			Dictionary<Type, CimType> types = new Dictionary<Type, CimType>();
			types.Add(typeof(sbyte), CimType.SInt8);
			types.Add(typeof(byte), CimType.UInt8);
			types.Add(typeof(short), CimType.SInt16);
			types.Add(typeof(ushort), CimType.UInt16);
			types.Add(typeof(int), CimType.SInt32);
			types.Add(typeof(uint), CimType.UInt32);
			types.Add(typeof(long), CimType.SInt64);
			types.Add(typeof(ulong), CimType.UInt64);
			types.Add(typeof(float), CimType.Real32);
			types.Add(typeof(double), CimType.Real64);
			types.Add(typeof(bool), CimType.Boolean);
			types.Add(typeof(string), CimType.String);
			types.Add(typeof(DateTime), CimType.DateTime);
			types.Add(typeof(TimeSpan), CimType.DateTime);
			types.Add(typeof(char), CimType.Char16);
			CimTypeConverter._dotNetTypeToScalarCimType = types;
		}
	}
}