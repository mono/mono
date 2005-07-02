//
// DependencyProperty.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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

namespace System.Windows {
	public class DependencyProperty {
		private PropertyMetadata defaultMetadata;
		private string name;
		private Type ownerType;
		private Type propertyType;
		private ValidateValueCallback validateValueCallback;

		private bool isAttached;
		internal bool IsAttached { get { return isAttached; } }

		private DependencyProperty (bool isAttached, string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			this.isAttached = isAttached;
			this.defaultMetadata = defaultMetadata;
			this.name = name;
			this.ownerType = ownerType;
			this.propertyType = propertyType;
			this.validateValueCallback = validateValueCallback;
		}

		public PropertyMetadata DefaultMetadata { 
			get { return defaultMetadata; }
		}
		public string Name {
			get { return name; }
		}
		public Type OwnerType {
			get { return ownerType; }
		}
		public Type PropertyType {
			get { return propertyType; }
		}
		public ValidateValueCallback ValidateValueCallback {
			get { return validateValueCallback; }
		}

		[MonoTODO()]		
		public DependencyProperty AddOwner(Type ownerType)
		{
			throw new NotImplementedException("AddOwner(Type ownerType)");
		}
		[MonoTODO()]		
		public DependencyProperty AddOwner(Type ownerType, PropertyMetadata typeMetadata)
		{
			throw new NotImplementedException("AddOwner(Type ownerType, PropertyMetadata typeMetadata)");
		}
		
		[MonoTODO()]		
		public static DependencyProperty FromName(String name, Type ownerType)
		{
			return DependencyObject.lookup(ownerType, name);
		}

		[MonoTODO()]		
		public PropertyMetadata GetMetadata(Type forType)
		{
			throw new NotImplementedException("GetMetadata(Type forType)");
		}
		[MonoTODO()]		
		public PropertyMetadata GetMetadata(DependencyObject d)
		{
			throw new NotImplementedException("GetMetadata(DependencyObject d)");
		}
		[MonoTODO()]		
		public PropertyMetadata GetMetadata(DependencyObjectType dependencyObjectType)
		{
			throw new NotImplementedException("GetMetadata(DependencyObjectType dependencyObjectType)");
		}

		[MonoTODO()]		
		public bool IsValidType(object value)
		{
			throw new NotImplementedException("IsValidType(object value)");
		}
		[MonoTODO()]		
		public bool IsValidValue(object value)
		{
			throw new NotImplementedException("IsValidValue(object value)");
		}
		
		[MonoTODO()]		
		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
		{
			throw new NotImplementedException("OverrideMetadata(Type forType, PropertyMetadata typeMetadata)");
		}
		[MonoTODO()]		
		public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata, DependencyPropertyKey key)
		{
			throw new NotImplementedException("OverrideMetadata(Type forType, PropertyMetadata typeMetadata, DependencyPropertyKey key)");
		}
		
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
		{
			return Register(name, propertyType, ownerType, null, null);
		}
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
		{
			return Register(name, propertyType, ownerType, typeMetadata, null);
		}
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
		{
			DependencyProperty dp = new DependencyProperty(false, name, propertyType, ownerType, typeMetadata, validateValueCallback);
			DependencyObject.register(ownerType, dp);
			return dp;
		}
		
		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
		{
			return RegisterAttached(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return RegisterAttached(name, propertyType, ownerType, defaultMetadata, null);
		}
		
		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			DependencyProperty dp = new DependencyProperty(true, name, propertyType, null, null, null);
			DependencyObject.register(ownerType, dp);
			return dp;
		}

		[MonoTODO()]		
		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			throw new NotImplementedException("RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)");
		}
		[MonoTODO()]		
		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException("RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)");
		}
		[MonoTODO()]		
		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
		{
			throw new NotImplementedException("RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)");
		}
		[MonoTODO()]		
		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException("RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata, ValidateValueCallback validateValueCallback)");
		}

	}
}
