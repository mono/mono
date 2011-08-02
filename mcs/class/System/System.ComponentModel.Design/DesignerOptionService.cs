//
// System.ComponentModel.Design.DesignerOptionService
//
// Authors: 
//  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.Globalization;
using System.ComponentModel;

namespace System.ComponentModel.Design
{

	public abstract class DesignerOptionService : IDesignerOptionService
	{
		[MonoTODO ("implement own TypeConverter")]
		[TypeConverter (typeof (TypeConverter))]
		[Editor ("", "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public sealed class DesignerOptionCollection : IList, ICollection, IEnumerable
		{
			// PropertyDescriptor objects are taken directly from the value passed to the CreateOptionCollection method
			// and are wrapped in an additional property descriptor that hides the value object from the user. This means
			// that any value may be passed into the component parameter of the various PropertyDescriptor methods. The
			// value is not recognized and is replaced with the correct value internally.
			//
			public sealed class WrappedPropertyDescriptor : PropertyDescriptor
			{
				private PropertyDescriptor _property;
				private object _component;
				
				public WrappedPropertyDescriptor (PropertyDescriptor property, object component) : base (property.Name, new Attribute[0])
				{
					_property = property;
					_component = component;
				}

				public override object GetValue (object ignored)
				{
					return _property.GetValue (_component);
				}

				public override void SetValue (object ignored, object value) 
				{
					_property.SetValue (_component, value);
				}

				public override bool CanResetValue (object ignored)
				{
					return _property.CanResetValue (_component);
				}

				public override void ResetValue (object ignored)
				{
					_property.ResetValue (_component);
				}

				public override bool ShouldSerializeValue (object ignored)
				{
					return _property.ShouldSerializeValue (_component);
				}

				public override AttributeCollection Attributes {
					get { return _property.Attributes; }
				}

				public override bool IsReadOnly {
					get { return _property.IsReadOnly; }
				}

				public override Type ComponentType {
					get { return _property.ComponentType; }
				}

				public override Type PropertyType {
					get { return _property.PropertyType; }
				}
			} // WrappedPropertyDescriptor
			
			private string _name;
			private object _propertiesProvider;
			private DesignerOptionCollection _parent;
			private ArrayList _children;
			private DesignerOptionService _optionService;
			
			internal DesignerOptionCollection (DesignerOptionCollection parent, string name, object propertiesProvider, DesignerOptionService service)
			{
				_name = name;
				_propertiesProvider = propertiesProvider;
				_parent = parent;

				if (parent != null) {
					if (parent._children == null)
						parent._children = new ArrayList ();
					parent._children.Add (this);
				}
				
				_children = new ArrayList ();
				_optionService = service;
				service.PopulateOptionCollection (this);
			}

			public bool ShowDialog ()
			{
				return _optionService.ShowDialog (this, _propertiesProvider);
			}

			public DesignerOptionCollection this[int index] {
				get { return (DesignerOptionCollection) _children[index]; }
			}
			
			public DesignerOptionCollection this[string index] {
				get {
					foreach (DesignerOptionCollection dc in _children) {
						if (String.Compare (dc.Name, index, true, CultureInfo.InvariantCulture) == 0)
								return dc;
					}
					return null;
				}
			}
			
			public string Name {
				get { return _name; }
			}

			public int Count {
				get {
					if (_children != null)					 
						return _children.Count;
					return 0;
				}
			}

			public DesignerOptionCollection Parent {
				get { return _parent; }
			}

			public PropertyDescriptorCollection Properties {
				get {
					// TypeDescriptor.GetProperties gets only the public properties.
					//
					PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (_propertiesProvider);				   
					ArrayList wrappedProperties = new ArrayList (properties.Count);
					
					foreach (PropertyDescriptor pd in properties)
						wrappedProperties.Add (new WrappedPropertyDescriptor (pd, _propertiesProvider));

					PropertyDescriptor[] propertyArray = (PropertyDescriptor[]) wrappedProperties.ToArray (typeof (PropertyDescriptor));
					return new PropertyDescriptorCollection (propertyArray);
				}
			}
			
			public IEnumerator GetEnumerator ()
			{
				return _children.GetEnumerator ();
			}

			public int IndexOf (DesignerOptionCollection item)
			{
				return _children.IndexOf (item);
			}

			public void CopyTo (Array array, int index)
			{
				_children.CopyTo (array, index);
			}
			
			bool IList.IsFixedSize {
				get { return true; }
			}

			bool IList.IsReadOnly {
				get { return true; }
			}

			object IList.this[int index] {
				get { return this[index]; }
				set { throw new NotSupportedException (); }
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.Contains (object item)
			{
				return _children.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return _children.IndexOf (item);
			}
			
			int IList.Add (object item)
			{
				throw new NotSupportedException ();
			}

			void IList.Remove (object item)
			{
				throw new NotSupportedException ();
			}
			
			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ();
			}
			
			void IList.Insert (int index, object item)
			{
				throw new NotSupportedException ();
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ();
			}
			
		} // DesignerOptionCollection


		private DesignerOptionCollection _options;
		
		protected internal DesignerOptionService ()
		{
		}

		protected DesignerOptionCollection CreateOptionCollection (DesignerOptionCollection parent, string name, Object value)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (parent == null)
				throw new ArgumentNullException ("parent");
			if (name == String.Empty)
				throw new ArgumentException ("name.Length == 0");
				
			return new DesignerOptionCollection (parent, name, value, this);			
		}

		protected virtual bool ShowDialog (DesignerOptionCollection options, object optionObject)
		{
			return false;
		}

		protected virtual void PopulateOptionCollection (DesignerOptionCollection options)
		{
		}
		
		public DesignerOptionCollection Options {
			get {
				if (_options == null)
					_options = new DesignerOptionCollection (null, String.Empty, null, this);

				return _options;
			}
		}

		
		object IDesignerOptionService.GetOptionValue (string pageName, string valueName)
		{
			if (pageName == null)
				throw new ArgumentNullException ("pageName");
			if (valueName == null)
				throw new ArgumentNullException ("valueName");

			PropertyDescriptor property = GetOptionProperty (pageName, valueName);
			if (property != null)
				return property.GetValue (null);
			
			return null;
		}

		void IDesignerOptionService.SetOptionValue (string pageName, string valueName, object value)
		{
			if (pageName == null)
				throw new ArgumentNullException ("pageName");
			if (valueName == null)
				throw new ArgumentNullException ("valueName");

			PropertyDescriptor property = GetOptionProperty (pageName, valueName);
			if (property != null)
				property.SetValue (null, value);
		}

		// Go to the page and get the property associated with the option name.
		//
		private PropertyDescriptor GetOptionProperty (string pageName, string valueName)
		{
			string[] pages = pageName.Split (new char[] { '\\' });

			DesignerOptionCollection options = this.Options;
			foreach (string page in pages) {
				options = options[page];
				if (options == null)
					return null;
			}

			return options.Properties[valueName];
		}
		
	}
}
