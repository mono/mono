//
// DefaultBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

namespace System
{
	partial class DefaultBinder
	{
		static bool CanConvertPrimitive (RuntimeType source, RuntimeType target)
		{
			if (source.IsEnum)
				return false;

			var from = Type.GetTypeCode (source);
			switch (Type.GetTypeCode (target)) {
			case TypeCode.Char:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.UInt16:
					return true;
				}
				return false;
			case TypeCode.Int16:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.SByte:
					return true;
				}
				return false;
			case TypeCode.UInt16:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.Char:
					return true;
				}
				return false;
			case TypeCode.Int32:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					return true;
				}
				return false;
			case TypeCode.UInt32:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.UInt16:
					return true;
				}
				return false;
			case TypeCode.Int64:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Char:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					return true;
				}
				return false;
			case TypeCode.UInt64:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
					return true;
				}
				return false;
			case TypeCode.Single:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Char:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
					return true;
				}
				return false;
			case TypeCode.Double:
				switch (from) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
					return true;
				}
				return false;
			}

			if (target == typeof (IntPtr))
				return source == target;

			if (target == typeof (UIntPtr))
				return source == target;

			return false;
		}

		static bool CanConvertPrimitiveObjectToType (Object source, RuntimeType type)
		{
			if (source == null)
				return true;

			var st = source.GetType ();
			return st == type || CanConvertPrimitive ((RuntimeType) st, type);
		}
	}
}