//
// Copyright (C) 2010 Novell Inc. http://novell.com
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

namespace System.Xaml.Schema
{
	public class XamlValueConverter<TConverterBase> : IEquatable<XamlValueConverter<TConverterBase>>
		where TConverterBase : class
	{
		public XamlValueConverter (Type converterType, XamlType targetType)
		{
			throw new NotImplementedException ();
		}
		public XamlValueConverter (Type converterType, XamlType targetType, string name)
		{
			throw new NotImplementedException ();
		}

		public TConverterBase ConverterInstance {
			get { throw new NotImplementedException (); }
		}
		public Type ConverterType { get; private set; }
		public string Name { get; private set; }
		public XamlType TargetType { get; private set; }

		
		public static bool operator == (XamlValueConverter<TConverterBase> left, XamlValueConverter<TConverterBase> right)
		{
			return IsNull (left) ? IsNull (right) : left.Equals (right);
		}

		static bool IsNull (XamlValueConverter<TConverterBase> a)
		{
			return Object.ReferenceEquals (a, null);
		}

		public static bool operator != (XamlValueConverter<TConverterBase> left, XamlValueConverter<TConverterBase> right)
		{
			return IsNull (left) ? !IsNull (right) : IsNull (right) || left.ConverterType != right.ConverterType || left.TargetType != right.TargetType || left.Name != right.Name;
		}
		
		public bool Equals (XamlValueConverter<TConverterBase> other)
		{
			return !IsNull (other) && ConverterType == other.ConverterType && TargetType == other.TargetType && Name == other.Name;
		}

		public override bool Equals (object obj)
		{
			var a = obj as XamlValueConverter<TConverterBase>;
			return Equals (a);
		}

		protected virtual TConverterBase CreateInstance ()
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
	}
}
