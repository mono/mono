//
// System.Nullable
//
// Martin Baulig (martin@ximian.com)
//
// (C) 2004 Novell, Inc.
//

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

using System.Reflection;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Runtime.CompilerServices;

#if NET_2_0
namespace System
{

	[CLSCompliant(false)]
	public static class Nullable
	{
		public static int Compare<T> (Nullable<T> left, Nullable<T> right)
		{
			IComparable icomparable = left.value as IComparable;
			if (icomparable == null)
				throw new ArgumentException ("At least one object must implement IComparable.");
			if (left.has_value == false && right.has_value == false)
				return 0;
			if (!left.has_value)
				return -1;
			if (!right.has_value)
				return 1;

			return icomparable.CompareTo (right.value);
		}

		public static bool Equals<T> (Nullable <T> value1, Nullable<T> value2)
		{
			return value1.Equals (value2);
		}

		public static Nullable<T> FromObject<T> (object value)
		{
			if (!(value is T))
				throw new ArgumentException ("Object type can not be converted to target type.");

			return new Nullable<T> ((T) value);
		}

		public static T GetValueOrDefault<T>(Nullable<T> value)
		{
			return GetValueOrDefault<T> (value, default (T));
		}

		public static T GetValueOrDefault<T> (Nullable<T> value, T defaultValue)
		{
			if (!value.has_value)
				return defaultValue;

			return value.value;
		}

		public static bool HasValue<T> (Nullable <T> value)
		{
			return value.has_value;
		}

		public static object ToObject<T> (Nullable<T> value)
		{
			if (!value.has_value)
				return null;

			return (object)value.value;
		}
	}
	
	[CLSCompliant(false)]
	public struct Nullable<T> : IComparable, INullableValue
	{
		internal T value;
		internal bool has_value;

		public Nullable (T value)
		{
			if (value == null)
				this.has_value = false;
			else
				this.has_value = true;
			
			this.value = value;
		}

		public bool HasValue {
			get { return has_value; }
		}

		public T Value {
			get { 
				if (!has_value)
					throw new InvalidOperationException ("Nullable object must have a value.");
				
				return value; 
			}
		}

		object INullableValue.Value {
			get {
				return (object)Value;
			}
		}
		
		[Obsolete]
		public int CompareTo (Nullable<T> other)
		{
			return Nullable.Compare<T> (this, other);
		}

		[Obsolete]
		public int CompareTo (object other)
		{
			if (!(other is Nullable<T>))
				throw new ArgumentException ("Object type can not be converted to target type.");

			return Nullable.Compare<T> (this, (Nullable<T>) other);
		}

		public override bool Equals (object other)
		{
			if (!(other is Nullable<T>))
				return false;

			return Equals ((Nullable <T>) other);
		}

		public bool Equals (Nullable<T> other)
		{
			if (other.has_value != has_value)
				return false;

			if (has_value == false)
				return true;

			return other.value.Equals (value);
		}

		[Obsolete]
		public static Nullable<T> FromObject (object value)
		{
			return Nullable.FromObject<T> (value);
		}

		public override int GetHashCode ()
		{
			if (!has_value)
				return 0;

			return value.GetHashCode ();
		}

		[Obsolete]
		public T GetValueOrDefault ()
		{
			return Nullable.GetValueOrDefault<T> (this, default (T));
		}

		[Obsolete]
		public T GetValueOrDefault (T def_value)
		{
			return Nullable.GetValueOrDefault<T> (this, def_value);
		}

		public object ToObject ()
		{
			if (!has_value)
				return null;

			return (object)value;
		}

		public override string ToString ()
		{
			return value.ToString ();
		}

		public static implicit operator Nullable<T> (T value)
		{
			return new Nullable<T> (value);
		}

		public static explicit operator T (Nullable<T> value)
		{
			return value.Value;
		}

		public static bool operator == (Nullable<T> left, Nullable<T> right)
		{
			return left.Equals (right);
		}

		public static bool operator != (Nullable<T> left, Nullable<T> right)
		{
			return !left.Equals (right);
		}
	}
}
#endif
