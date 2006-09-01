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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

namespace System.Workflow.ComponentModel
{
	[Serializable]
	public sealed class DependencyProperty : ISerializable
	{
		private static IDictionary <int, DependencyProperty> properties = new Dictionary <int, DependencyProperty> ();

		private PropertyMetadata def_metadata;
		private bool attached;
		private bool _event;
		private string name;
		private Type owner_type;
		private Type property_type;
		private Type validator_type;

		// Constructors
		private DependencyProperty ()
		{

		}

		private DependencyProperty (string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			this.name = name;
			property_type = propertyType;
			owner_type = ownerType;
			def_metadata = defaultMetadata;
		}

		// Properties
		public PropertyMetadata DefaultMetadata {
			get { return def_metadata; }
		}

		public bool IsAttached {
			get {return attached; }
		}

		public bool IsEvent {
			get {return _event; }
		}

		public string Name {
			get {return name; }
		}

		public Type OwnerType {
			get {return owner_type; }
		}

		public Type PropertyType {
			get {return property_type; }
		}

		public Type ValidatorType {
			get {return validator_type; }
		}

		internal bool IsEventSet {
			set { _event = value; }
		}

		// Methods
		public static DependencyProperty FromName (string propertyName, Type ownerType)
		{
			DependencyProperty rslt;

			rslt = properties [propertyName.GetHashCode() * ownerType.GetHashCode()];
			return rslt;
		}

		public static IList <DependencyProperty> FromType (Type ownerType)
		{
			List <DependencyProperty> rslt = new List <DependencyProperty> ();
			IEnumerator e = properties.GetEnumerator ();
			DependencyProperty property;

			for (e.Reset (); e.MoveNext ();) {

				if (e.Current as DependencyProperty == null)
					continue;

				property = (DependencyProperty) e.Current;

				if (property.OwnerType == ownerType) {
					rslt.Add (property);
				}
			}

			return rslt;
		}

		public override int GetHashCode ()
		{
			return name.GetHashCode() * owner_type.GetHashCode ();
		}

		public static DependencyProperty Register (string name, Type propertyType, Type ownerType)
		{
			return Register (name, propertyType, ownerType, new PropertyMetadata ());
		}

		public static DependencyProperty Register (string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			DependencyProperty property = new DependencyProperty (name, propertyType, ownerType, defaultMetadata);

			if (ownerType.GetField (name + "Event", BindingFlags.Public | BindingFlags.Static) != null) {
				property.IsEventSet = true;
			}

			if (properties.ContainsKey (property.GetHashCode ())) {
				throw new InvalidOperationException ("A property with the same name already exists");
			}

			properties.Add (property.GetHashCode (), property);
			return property;
		}

		public static DependencyProperty RegisterAttached (string name, Type propertyType, Type ownerType)
		{
			return RegisterAttached (name, propertyType, ownerType, new PropertyMetadata (), null);
		}

		public static DependencyProperty RegisterAttached (string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{

			return RegisterAttached (name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyProperty RegisterAttached (string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, Type validatorType)
		{
			DependencyProperty property = Register (name,  propertyType, ownerType, defaultMetadata);
			property.attached = true;
			property.validator_type = validatorType;
			return property;
		}

		[MonoTODO]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return name;
		}
	}
}

