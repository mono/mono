//
// EnumDataTypeAttribute.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell Inc. (http://novell.com)
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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class EnumDataTypeAttribute : DataTypeAttribute
	{
		public Type EnumType { get; private set; }
		
		public EnumDataTypeAttribute (Type enumType)
			: base (DataType.Custom)
		{
			this.EnumType = enumType;
		}

		public override bool IsValid (object value)
		{
			Type type = EnumType;

			if (!type.IsEnum)
				throw new InvalidOperationException (
					String.Format ("The type '{0}' needs to represent an enumeration type.", type.FullName)
				);

			if (value == null)
				return true;

			Type valueType = value.GetType ();
			if (valueType.IsEnum && valueType != type)
				return false;

			string s = value as string;
			if (s != null && s.Length == 0)
				return true;
			
			if (s != null && (valueType == typeof (bool) || valueType == typeof (char) || valueType == typeof (float)))
				return false;

			object o;

			if (s != null) {
				try {
					o = Enum.Parse (type, s);
				} catch {
					return false;
				}
			} else if (valueType.IsEnum)
				o = value;
			else {
				try {
					o = Enum.ToObject (type, value);
				} catch {
					return false;
				}
			}

			object[] attrs = type.GetCustomAttributes (typeof (FlagsAttribute), true);
			if (attrs != null && attrs.Length > 0) {
				string sval = Convert.ChangeType (o, Enum.GetUnderlyingType (type), CultureInfo.InvariantCulture).ToString ();

				// This looks weird, but what happens here is that if we have a
				// mismatch, the above type change will make sval equal o.ToString
				// () and if we have a match, then sval will be string
				// representation of the enum member's value. So, if we have an
				// enum:
				//
				// [Flags]
				// enum Test
				// {
				//     One = 1,
				//     Two = 2
				// }
				//
				// And the passed value was 3, then o.ToString () == "One, Two" and
				// sval == "3". If the passed value was 33, though, o.ToString () ==
				// "33" and sval == "33" - thus we DON'T have a match.
				return !sval.Equals (o.ToString ());
			}
			
			return Enum.IsDefined (type, o);
		}
	}
}
#endif