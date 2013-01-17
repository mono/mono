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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Windows;

namespace System.ComponentModel {

	public sealed class DependencyPropertyDescriptor : PropertyDescriptor {
		internal DependencyPropertyDescriptor () : base (null)
		{
		}

		public override AttributeCollection Attributes
		{
			get { throw new NotImplementedException (); }
		}
		public override string Category
		{
			get { throw new NotImplementedException (); }
		}
		public override Type ComponentType
		{
			get { throw new NotImplementedException (); }
		}
		public override TypeConverter Converter
		{
			get { throw new NotImplementedException (); }
		}
		public DependencyProperty DependencyProperty
		{
			get { throw new NotImplementedException (); }
		}
		public override string Description
		{
			get { throw new NotImplementedException (); }
		}
		public override bool DesignTimeOnly
		{
			get { throw new NotImplementedException (); }
		}
		public override string DisplayName
		{
			get { throw new NotImplementedException (); }
		}
		public bool IsAttached
		{
			get { throw new NotImplementedException (); }
		}
		public override bool IsBrowsable
		{
			get { throw new NotImplementedException (); }
		}
		public override bool IsLocalizable
		{
			get { throw new NotImplementedException (); }
		}
		public override bool IsReadOnly
		{
			get { throw new NotImplementedException (); }
		}
		public PropertyMetadata Metadata
		{
			get { throw new NotImplementedException (); }
		}
		public override Type PropertyType
		{
			get { throw new NotImplementedException (); }
		}
		public override bool SupportsChangeEvents
		{
			get { throw new NotImplementedException (); }
		}

		public override void AddValueChanged (object component, EventHandler handler)
		{
			throw new NotImplementedException ();
		}
		public override bool CanResetValue (object component)
		{
			throw new NotImplementedException ();
		}
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}
		public override PropertyDescriptorCollection GetChildProperties (object instance, Attribute[] filter)
		{
			throw new NotImplementedException ();
		}
		public override object GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}
		public override object GetValue (object component)
		{
			throw new NotImplementedException ();
		}
		public override void RemoveValueChanged (object component, EventHandler handler)
		{
			throw new NotImplementedException ();
		}
		public override void ResetValue (object component)
		{
			throw new NotImplementedException ();
		}
		public override void SetValue (object component, object value)
		{
			throw new NotImplementedException ();
		}
		public override bool ShouldSerializeValue (object component)
		{
			throw new NotImplementedException ();
		}
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public static DependencyPropertyDescriptor FromName (string name, Type ownerType, Type targetType)
		{
			throw new NotImplementedException ();
		}
		public static DependencyPropertyDescriptor FromProperty (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}
		public static DependencyPropertyDescriptor FromProperty (DependencyProperty dependencyProperty, Type targetType)
		{
			throw new NotImplementedException ();
		}

		public CoerceValueCallback DesignerCoerceValueCallback {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}

}