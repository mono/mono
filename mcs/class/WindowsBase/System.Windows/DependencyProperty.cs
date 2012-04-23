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
// (C) 2005 Iain McCoy
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Iain McCoy (iain@mccoy.id.au)
//	Chris Toshok (toshok@ximian.com)
//

using System.Collections.Generic;

namespace System.Windows {
	public sealed class DependencyProperty {
		private Dictionary<Type,PropertyMetadata> metadataByType = new Dictionary<Type,PropertyMetadata>();

		public static readonly object UnsetValue = new object ();

		private DependencyProperty (bool isAttached, string name, Type propertyType, Type ownerType,
					    PropertyMetadata defaultMetadata,
					    ValidateValueCallback validateValueCallback)
		{
			IsAttached = isAttached;
			DefaultMetadata = (defaultMetadata == null ? new PropertyMetadata() : defaultMetadata);
			Name = name;
			OwnerType = ownerType;
			PropertyType = propertyType;
			ValidateValueCallback = validateValueCallback;
		}

		internal bool IsAttached { get; set; }
		public bool ReadOnly { get; private set; }
		public PropertyMetadata DefaultMetadata { get; private set; }
		public string Name { get; private set; }
		public Type OwnerType { get; private set; }
		public Type PropertyType { get; private set; }
		public ValidateValueCallback ValidateValueCallback { get; private set; }

		public int GlobalIndex {			
			get { throw new NotImplementedException (); }
		}


		public DependencyProperty AddOwner(Type ownerType)
		{
			return AddOwner (ownerType, null);
		}

		public DependencyProperty AddOwner(Type ownerType, PropertyMetadata typeMetadata)
		{
			if (typeMetadata == null) typeMetadata = new PropertyMetadata ();
			OverrideMetadata (ownerType, typeMetadata);

			// MS seems to always return the same DependencyProperty
			return this;
		}
		
		public PropertyMetadata GetMetadata(Type forType)
		{
			if (metadataByType.ContainsKey (forType))
				return metadataByType[forType];
			return null;
		}

		public PropertyMetadata GetMetadata(DependencyObject dependencyObject)
		{
			if (metadataByType.ContainsKey (dependencyObject.GetType()))
				return metadataByType[dependencyObject.GetType()];
			return null;
		}

		public PropertyMetadata GetMetadata(DependencyObjectType dependencyObjectType)
		{
			if (metadataByType.ContainsKey (dependencyObjectType.SystemType))
				return metadataByType[dependencyObjectType.SystemType];
			return null;
		}


		public bool IsValidType(object value)
		{
			return PropertyType.IsInstanceOfType (value);
		}

		public bool IsValidValue(object value)
		{
			if (!IsValidType (value))
				return false;
			if (ValidateValueCallback == null)
				return true;
			return ValidateValueCallback (value);
		}
		
		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
		{
			if (forType == null)
				throw new ArgumentNullException ("forType");
			if (typeMetadata == null)
				throw new ArgumentNullException ("typeMetadata");

			if (ReadOnly)
				throw new InvalidOperationException (String.Format ("Cannot override metadata on readonly property '{0}' without using a DependencyPropertyKey", Name));

			typeMetadata.DoMerge (DefaultMetadata, this, forType);
			metadataByType.Add (forType, typeMetadata);
		}

		public void OverrideMetadata (Type forType, PropertyMetadata typeMetadata, DependencyPropertyKey key)
		{
			if (forType == null)
				throw new ArgumentNullException ("forType");
			if (typeMetadata == null)
				throw new ArgumentNullException ("typeMetadata");


			// further checking?  should we check
			// key.DependencyProperty == this?

			typeMetadata.DoMerge (DefaultMetadata, this, forType);
			metadataByType.Add (forType, typeMetadata);
		}

		public override string ToString ()
		{
			return Name;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode() ^ PropertyType.GetHashCode() ^ OwnerType.GetHashCode();
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
		{
			return Register(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType,
							  PropertyMetadata typeMetadata)
		{
			return Register(name, propertyType, ownerType, typeMetadata, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType,
							  PropertyMetadata typeMetadata,
							  ValidateValueCallback validateValueCallback)
		{
			if (typeMetadata == null)
				typeMetadata = new PropertyMetadata();

			DependencyProperty dp = new DependencyProperty(false, name, propertyType, ownerType,
								       typeMetadata, validateValueCallback);
			DependencyObject.register(ownerType, dp);

			dp.OverrideMetadata (ownerType, typeMetadata);

			return dp;
		}
		
		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
		{
			return RegisterAttached(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType,
								  PropertyMetadata defaultMetadata)
		{
			return RegisterAttached(name, propertyType, ownerType, defaultMetadata, null);
		}
		
		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType,
								  PropertyMetadata defaultMetadata,
								  ValidateValueCallback validateValueCallback)
		{
			DependencyProperty dp = new DependencyProperty(true, name, propertyType, ownerType,
								       defaultMetadata, validateValueCallback);
			DependencyObject.register(ownerType, dp);
			return dp;
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType,
									     PropertyMetadata defaultMetadata)
		{
			throw new NotImplementedException("RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)");
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType,
									     PropertyMetadata defaultMetadata,
									     ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException("RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)");
		}

		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType,
								     PropertyMetadata typeMetadata)
		{
			return RegisterReadOnly (name, propertyType, ownerType, typeMetadata, null);
		}


		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType,
								     PropertyMetadata typeMetadata,
								     ValidateValueCallback validateValueCallback)
		{
			DependencyProperty prop = Register (name, propertyType, ownerType, typeMetadata, validateValueCallback);
			prop.ReadOnly = true;
			return new DependencyPropertyKey (prop);
		}

	}
}
