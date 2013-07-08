// Copyright 2013 Zynga Inc.
//	
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//		
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.

using System;

namespace _root
{

	//
	// Conversions (must be in C# to avoid conflicts).
	//

	public static class String_fn
	{
		public static string String (object o)
		{
			return o.ToString();
		}

		public static string String (string s)
		{
			return s;
		}

		public static string String (int i)
		{
			return i.ToString ();
		}

		public static string String (uint u)
		{
			return u.ToString ();
		}

		public static string String (double d)
		{
			return d.ToString ();
		}

		public static string String (bool b)
		{
			return b.ToString ();
		}

	}

	public static class Number_fn
	{  
		// Inlineable method
		public static double Number (string s)
		{
			double d;
			double.TryParse(s, out d);
			return d;
		}

	}

	public static class int_fn
	{  
		// Inlineable method
		public static int @int (string s)
		{
			int i;
			int.TryParse(s, out i);
			return i;
		}
		
	}

	public static class uint_fn
	{  

		// Inlineable method
		public static uint @uint (string s)
		{
			uint u;
			uint.TryParse(s, out u);
			return u;
		}

	}

	public static class Boolean_fn
	{  

		// Not inlinable.. but required to get correct results in flash.
		public static bool Boolean (object d)
		{
			if (d == null) return false;

			TypeCode tc = Type.GetTypeCode(d.GetType());
			switch (tc) {
			case TypeCode.Boolean:
				return (bool)d;
			case TypeCode.SByte:
				return (sbyte)d != 0;
			case TypeCode.Byte:
				return (byte)d != 0;
			case TypeCode.Int16:
				return (short)d != 0;
			case TypeCode.UInt16:
				return (ushort)d != 0;
			case TypeCode.Int32:
				return (int)d != 0;
			case TypeCode.UInt32:
				return (uint)d != 0;
			case TypeCode.Int64:
				return (long)d != 0;
			case TypeCode.UInt64:
				return (ulong)d != 0;
			case TypeCode.Single:
				return (float)d != 0.0f;
			case TypeCode.Double:
				return (double)d != 0.0;
			case TypeCode.Decimal:
				return (decimal)d != 0;
			case TypeCode.String:
				var s = (string)d;
				return !string.IsNullOrEmpty(s) && s != "0" && s != "false";
			case TypeCode.Empty:
				return false;
			case TypeCode.Object:
				return d != null;
			}
			return false;
		}

		// Inlineable method
		public static bool Boolean (string s)
		{
			throw new System.NotImplementedException();
		}

	}
}

