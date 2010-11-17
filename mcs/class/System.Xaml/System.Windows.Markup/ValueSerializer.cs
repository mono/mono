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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xaml;

namespace System.Windows.Markup
{
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyWindowsBase)]
	public abstract class ValueSerializer
	{
		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor)
		{
			return GetSerializerFor (descriptor, null);
		}

		public static ValueSerializer GetSerializerFor (Type type)
		{
			return GetSerializerFor (type, null);
		}

		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public static ValueSerializer GetSerializerFor (Type type, IValueSerializerContext context)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			// weird, but .NET also throws NIE(!)
			if (context != null)
				throw new NotImplementedException ();

			// Standard MarkupExtensions are serialized without ValueSerializer.
			if (typeof (MarkupExtension).IsAssignableFrom (type) && XamlLanguage.AllTypes.Any (x => x.UnderlyingType == type))
				return null;

			var tc = TypeDescriptor.GetConverter (type);
			if (tc != null && tc.GetType () != typeof (TypeConverter))
				return new TypeConverterValueSerializer (type);
			return null;
		}

		// instance members

		public virtual bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return false;
		}

		public virtual bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return false;
		}

		public virtual object ConvertFromString (string value, IValueSerializerContext context)
		{
			throw new NotSupportedException (String.Format ("Conversion from string '{0}' is not supported", value));
		}

		public virtual string ConvertToString (object value,     IValueSerializerContext context)
		{
			throw new NotSupportedException (String.Format ("Conversion from '{0}' to string is not supported", value != null ? value.GetType ().Name : "(null)"));
		}

		protected Exception GetConvertFromException (object value)
		{
			throw new NotImplementedException ();
		}

		protected Exception GetConvertToException (object value, Type destinationType)
		{
			throw new NotImplementedException ();
		}

		public virtual IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
	}

	internal class StringValueSerializer : ValueSerializer
	{
		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return true;
		}

		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return true;
		}

		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ConvertToString (object value,     IValueSerializerContext context)
		{
			return (string) value;
		}

		public override IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
	}

	#region Internal implementations.

	internal class TypeConverterValueSerializer<T> : TypeConverterValueSerializer
	{
		public TypeConverterValueSerializer ()
			: base (typeof (T))
		{
		}
	}

	internal class TypeConverterValueSerializer : ValueSerializer
	{
		public TypeConverterValueSerializer (Type type)
		{
			c = TypeDescriptor.GetConverter (type);
		}

		TypeConverter c;

		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return c.CanConvertFrom (context, typeof (string));
		}

		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return c.CanConvertTo (context, typeof (string));
		}

		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			return c.ConvertFromInvariantString (context, value);
		}

		public override string ConvertToString (object value,     IValueSerializerContext context)
		{
			return value == null ? String.Empty : c.ConvertToInvariantString (context, value);
		}

		public override IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	#endregion
}
