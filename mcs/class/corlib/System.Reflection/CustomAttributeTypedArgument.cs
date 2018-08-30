//
// System.Reflection/CustomAttributeTypedArgument.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//   Carlos Alberto Cortez (calberto.cortez@gmail.com)
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


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection {

	[ComVisible (true)]
	[Serializable]
	public struct CustomAttributeTypedArgument {
		Type argumentType;
		object value;

		public
		CustomAttributeTypedArgument (Type argumentType, object value)
		{
			if (argumentType == null)
				throw new ArgumentNullException ("argumentType");

			this.argumentType = argumentType;
			this.value = (value == null) ? null : CanonicalizeValue (value);

			// MS seems to convert arrays into a ReadOnlyCollection
			if (value is Array) {
				Array a = (Array)value;

				Type etype = a.GetType ().GetElementType ();
				CustomAttributeTypedArgument[] new_value = new CustomAttributeTypedArgument [a.GetLength (0)];
				for (int i = 0; i < new_value.Length; ++i)
					new_value [i] = new CustomAttributeTypedArgument (etype, a.GetValue (i));
				this.value = new ReadOnlyCollection <CustomAttributeTypedArgument> (new_value);
			}
		}
		
		public CustomAttributeTypedArgument (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			this.argumentType = value.GetType ();
			this.value = CanonicalizeValue (value);
		}

		public Type ArgumentType {
			get {
				return argumentType;
			}
		}

		public object Value {
			get {
				return value;
			}
		}

		public override string ToString () => ToString (typed: false);

		internal string ToString (bool typed)
		{
			if (ArgumentType == null)
				return base.ToString (); // Someone called ToString() on "default(CustomAttributeTypedArgument)"

			try	{
				if (ArgumentType.IsEnum)
					return string.Format (CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", Value, ArgumentType.FullName);

				else if (Value == null)
					return string.Format (CultureInfo.CurrentCulture, typed ? "null" : "({0})null", ArgumentType.Name);

				else if (ArgumentType == typeof (string))
					return string.Format (CultureInfo.CurrentCulture, "\"{0}\"", Value);

				else if (ArgumentType == typeof (char))
					return string.Format (CultureInfo.CurrentCulture, "'{0}'", Value);

				else if (ArgumentType == typeof (Type))
					return string.Format (CultureInfo.CurrentCulture, "typeof({0})", ((Type)Value).FullName);

				else if (ArgumentType.IsArray) {
					StringBuilder result = new StringBuilder ();
					IList<CustomAttributeTypedArgument> array = Value as IList<CustomAttributeTypedArgument>;

					Type elementType = ArgumentType.GetElementType ();
					result.AppendFormat (CultureInfo.CurrentCulture, @"new {0}[{1}] {{ ", elementType.IsEnum ? elementType.FullName : elementType.Name, array.Count);

					for (int i = 0; i < array.Count; i++)
						result.AppendFormat (CultureInfo.CurrentCulture, i == 0 ? "{0}" : ", {0}", array [i].ToString (elementType != typeof (object)));

					return result.Append (" }").ToString ();
				}

				return string.Format (CultureInfo.CurrentCulture, typed ? "{0}" : "({1}){0}", Value, ArgumentType.Name);
			} catch (MissingMetadataException) {
				return base.ToString (); // Failsafe. Code inside "try" should still strive to avoid trigging a MissingMetadataException as caught exceptions are annoying when debugging.
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is CustomAttributeTypedArgument))
				return false;
			CustomAttributeTypedArgument other = (CustomAttributeTypedArgument) obj;
			return  other.argumentType == argumentType &&
				value != null ? value.Equals (other.value) : (object) other.value == null;
		}

		public override int GetHashCode ()
		{
			return (argumentType.GetHashCode () << 16) + (value != null ? value.GetHashCode () : 0);
		}

		public static bool operator == (CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
		{
			return left.Equals (right);
		}

		public static bool operator != (CustomAttributeTypedArgument left, CustomAttributeTypedArgument right)
		{
			return !left.Equals (right);
		}

		private static object CanonicalizeValue (object value)
		{
			if (value is Enum e) 
				return e.GetValue ();
			return value;
		}		
	}
}
