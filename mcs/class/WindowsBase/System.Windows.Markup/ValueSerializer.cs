#if !NET_4_0
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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Converters;

namespace System.Windows.Markup {

	// I don't like the idea of hardcoding the types in here, but
	// I'm not sure what else to do about it..
	class DefaultValueSerializerContext : IValueSerializerContext
	{
		public ValueSerializer GetValueSerializerFor (PropertyDescriptor descriptor)
		{
			Attribute attribute = (Attribute)descriptor.Attributes[typeof (ValueSerializerAttribute)];
			if (attribute != null)
				return (ValueSerializer)Activator.CreateInstance (((ValueSerializerAttribute)attribute).ValueSerializerType);

			return GetValueSerializerFor (descriptor.PropertyType);
		}

		public ValueSerializer GetValueSerializerFor (Type type)
		{
			Attribute attribute = (Attribute)TypeDescriptor.GetAttributes (type)[typeof (ValueSerializerAttribute)];
			if (attribute != null)
				return (ValueSerializer)Activator.CreateInstance (((ValueSerializerAttribute)attribute).ValueSerializerType);

			if (typeof (DateTime).IsAssignableFrom (type))
				return new DateTimeValueSerializer();
			else if (typeof (Int32Rect).IsAssignableFrom (type))
				return new Int32RectValueSerializer ();
			else if (typeof (Point).IsAssignableFrom (type))
				return new PointValueSerializer ();
			else if (typeof (Rect).IsAssignableFrom (type))
				return new RectValueSerializer ();
			else if (typeof (Size).IsAssignableFrom (type))
				return new SizeValueSerializer ();
			else if (typeof (Vector).IsAssignableFrom (type))
				return new VectorValueSerializer ();
			else
				return null;
		}

		public void OnComponentChanged ()
		{
		}

		public bool OnComponentChanging ()
		{
			return false;
		}

		public IContainer Container {
			get { return null; }
		}

		public object Instance {
			get { return null; }
		}

		public PropertyDescriptor PropertyDescriptor {
			get { return null; }
		}

		public object GetService (Type serviceType)
		{
			return null;
		}
	}

	public abstract class ValueSerializer
	{
		protected ValueSerializer ()
		{
		}

		public virtual bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public virtual bool CanConvertToString (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public virtual object ConvertFromString (string value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public virtual string ConvertToString (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
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

		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor)
		{
			DefaultValueSerializerContext defaultContext = new DefaultValueSerializerContext();
			return defaultContext.GetValueSerializerFor (descriptor);
		}

		public static ValueSerializer GetSerializerFor (Type type)
		{
			DefaultValueSerializerContext defaultContext = new DefaultValueSerializerContext();
			return defaultContext.GetValueSerializerFor (type);
		}

		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor, IValueSerializerContext context)
		{
			ValueSerializer s = context.GetValueSerializerFor (descriptor);
			if (s == null) {
				DefaultValueSerializerContext defaultContext = new DefaultValueSerializerContext();
				s = defaultContext.GetValueSerializerFor (descriptor);
			}
			return s;
		}

		public static ValueSerializer GetSerializerFor (Type type, IValueSerializerContext context)
		{
			ValueSerializer s = context.GetValueSerializerFor (type);
			if (s == null) {
				DefaultValueSerializerContext defaultContext = new DefaultValueSerializerContext();
				s = defaultContext.GetValueSerializerFor (type);
			}
			return s;
		}
	}

}
#endif
