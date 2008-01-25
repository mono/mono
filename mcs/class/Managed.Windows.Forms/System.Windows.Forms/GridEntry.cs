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
// Copyright (c) 2004-2008 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//      Ivan N. Zlatev	  (contact@i-nz.net)
//

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms.PropertyGridInternal
{
	internal class GridEntry : GridItem, ITypeDescriptorContext
	{
		#region Local Variables
		private PropertyGrid property_grid;
		private bool expanded;
		private GridItemCollection grid_items;
		private GridItem parent;
		private PropertyDescriptor property_descriptor;
		private object[] property_owners;
		private int top;
		private Rectangle plus_minus_bounds;
		#endregion	// Local Variables

		#region  Contructors
		protected GridEntry (PropertyGrid propertyGrid)
		{
			if (propertyGrid == null)
				throw new ArgumentNullException ("propertyGrid");
			property_grid = propertyGrid;
			plus_minus_bounds = new Rectangle (0,0,0,0);
			top = -1;
			grid_items = new GridItemCollection ();
			expanded = false;
		}

		public GridEntry (PropertyGrid propertyGrid, object[] propertyOwners, PropertyDescriptor prop_desc) : this (propertyGrid) 
		{
			if (propertyOwners == null)
				throw new ArgumentNullException ("propertyOwners");
			if (prop_desc == null)
				throw new ArgumentNullException ("prop_desc");
			property_owners = propertyOwners;
			property_descriptor = prop_desc;
		}
		#endregion	// Constructors

		#region Public Instance Properties

		public override bool Expandable {
			get { return grid_items.Count > 0; }
		}

		public override bool Expanded {
			get { return expanded; }
			set {
				if (expanded != value) {
					if (value)
						property_grid.OnExpandItem (this);
					else
						property_grid.OnCollapseItem (this);
					expanded = value;
				}
			}
		}

		public override GridItemCollection GridItems {
			get { return grid_items; }
		}

		public override GridItemType GridItemType {
			get { return GridItemType.Property; }
		}

		public override string Label {
			get { return property_descriptor.Name; }
		}

		public override GridItem Parent {
			get { return parent; }
		}

		public override PropertyDescriptor PropertyDescriptor {
			get { return property_descriptor; }
		}

		public override object Value {
			get {
				if (property_owners == null || property_owners.Length == 0 || property_descriptor == null)
					return null;

				object v = property_descriptor.GetValue (property_owners[0]);
				for (int i = 1; i < property_owners.Length; i ++) {
					if (!Object.Equals (v, property_descriptor.GetValue (property_owners[i])))
						return null;
				}

				return v;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override bool Select ()
		{
			property_grid.SelectedGridItem = this;
			return true;
		}
		#endregion	// Public Instance Methods

		#region ITypeDescriptorContext
		void ITypeDescriptorContext.OnComponentChanged () {
		}

		bool ITypeDescriptorContext.OnComponentChanging () {
			return false;
		}

		IContainer ITypeDescriptorContext.Container {
			get {
				if (property_owners != null && property_owners.Length > 0 && property_descriptor == null)
					return null;

				IComponent component = property_owners[0] as IComponent;
				if (component != null && component.Site != null)
					return component.Site.Container;
				return null;
			}
		}

		object ITypeDescriptorContext.Instance {
			get { return property_owners[0]; }
		}

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor {
			get { return property_descriptor; }
		}
		#endregion

		#region IServiceProvider Members

		object IServiceProvider.GetService (Type serviceType) {
			IComponent selectedComponent = property_grid.SelectedObject as IComponent;
			if (selectedComponent != null && selectedComponent.Site != null)
				return selectedComponent.Site.GetService (serviceType);
			return null;
		}

		#endregion

		public object[] PropertyOwners {
			get { return property_owners; }
			set { property_owners = value; }
		}

		internal int Top {
			get { return top; }
			set {
				if (top != value)
					top = value;
			}
		}

		internal Rectangle PlusMinusBounds {
			get { return plus_minus_bounds; }
			set { plus_minus_bounds = value; }
		}

		public void SetParent (GridItem parent)
		{
			this.parent = parent;
		}

		public string ValueString {
			get { return ConvertToString (this.Value); }
		}

		public ICollection AcceptedValues {
			get {
				if (property_descriptor != null && property_descriptor.Converter != null &&
				    property_descriptor.Converter.GetStandardValuesSupported ()) {
					ArrayList values = new ArrayList ();
					string stringVal = null;
					foreach (object value in property_descriptor.Converter.GetStandardValues ()) {
						stringVal = ConvertToString (value);
						if (stringVal != null)
							values.Add (stringVal);
					}
					return values.Count > 0 ? values : null;
				}
				return null;
			}
		}

		// TODO
		private string ConvertToString (object value)
		{
			if (value is string)
				return (string)value;

			if (property_descriptor != null && property_descriptor.Converter != null &&
			    property_descriptor.Converter.CanConvertTo (typeof (string))) {
				try {
					return property_descriptor.Converter.ConvertToString (value);
				} catch {
					// TODO
					return null;
				}
			}

			return null;
		}

		public bool HasCustomEditor {
			get { return GetEditor() != null; }
		}

		public UITypeEditorEditStyle EditorStyle {
			get {
				UITypeEditor editor = GetEditor ();
				if (editor != null)
					return editor.GetEditStyle ((ITypeDescriptorContext)this);
				return UITypeEditorEditStyle.None;
			}
		}

		public bool EditValue (IWindowsFormsEditorService service)
		{
			if (service == null)
				throw new ArgumentNullException ("service");

			IServiceContainer parent = ((ITypeDescriptorContext)this).GetService (typeof (IServiceContainer)) as IServiceContainer;
			ServiceContainer container = null;

			if (parent != null)
				container = new ServiceContainer (parent);
			else
				container = new ServiceContainer ();

			container.AddService (typeof (IWindowsFormsEditorService), service);

			UITypeEditor editor = GetEditor ();
			if (editor != null) {
				try {
					object value = editor.EditValue ((ITypeDescriptorContext)this,
									 container,
									 this.Value);
					return SetValue (value);
				} catch {
					// TODO
				}
			}
			return false;
		}

		private UITypeEditor GetEditor ()
		{
			if (property_descriptor != null) {
				try { // can happen, because we are missing some editors
					return property_descriptor.GetEditor (typeof (UITypeEditor)) as UITypeEditor;
				} catch {
					// TODO
				}
			}
			return null;
		}

		public bool ToggleValue ()
		{
			if (IsReadOnly)
				return false;

			bool success = false;
			if (property_descriptor.PropertyType == typeof(bool))
				success = SetValue (!(bool)this.Value);
			else if (property_descriptor.Converter != null && 
				 property_descriptor.Converter.GetStandardValuesSupported ()) {
				TypeConverter.StandardValuesCollection values = 
					(TypeConverter.StandardValuesCollection) property_descriptor.Converter.GetStandardValues();
				for (int i = 0; i < values.Count; i++) {
					if (this.Value.Equals (values[i])){
						if (i < values.Count-1)
							success = SetValue (values[i+1]);
						else
							success = SetValue (values[0]);
						break;
					}
				}
			}
			return success;
		}

		public bool SetValue (object value)
		{
			if (property_descriptor == null)
				return false;

			// if the new value is not of the same type try to convert it
			if (value != null && this.Value != null && value.GetType () != this.Value.GetType ()) {
				if (property_descriptor.Converter != null &&
				    property_descriptor.Converter.CanConvertFrom (value.GetType ())) {
					try {
						value = property_descriptor.Converter.ConvertFrom (value);
					} catch {
						// TODO
					}
				}
			}

			bool changed = false;
			foreach (object propertyOwner in property_owners) {
				object currentVal = property_descriptor.GetValue (propertyOwner);
				if (!Object.Equals (currentVal, value)) {
					try {
						property_descriptor.SetValue (propertyOwner, value);
					} catch {
						// TODO
					}
					if (IsValueType (this.Parent))
						((GridEntry) this.Parent).SetValue (propertyOwner);
					changed = true;
				}
			}

			if (changed)
				property_grid.OnPropertyValueChangedInternal (this, this.Value);
			return changed;
		}

		private bool IsValueType (GridItem item)
		{
			if (item != null && item.PropertyDescriptor != null && 
			    (item.PropertyDescriptor.PropertyType.IsValueType ||
			     item.PropertyDescriptor.PropertyType.IsArray))
				return true;
			return false;
		}

		public bool ResetValue ()
		{
			if (IsResetable) {
				property_descriptor.ResetValue (property_owners[0]);
				property_grid.OnPropertyValueChangedInternal (this, this.Value);
				return true;
			}
			return false;
		}

		public virtual bool IsResetable {
			get { return (!IsReadOnly && property_descriptor.CanResetValue (property_owners[0])); }
		}

		// If false the entry can be modified only by the means of a predefined values
		// and not such inputed by the user.
		//
		public virtual bool IsEditable {
			get {
				if (property_descriptor == null || property_descriptor.IsReadOnly)
					return false;
				else if (property_descriptor.Converter == null)
					return false;
				else if (property_descriptor.Converter.GetStandardValuesSupported () &&
					 property_descriptor.Converter.GetStandardValuesExclusive ())
					return false;
				else
					return true;
			}
		}

		// If true the the entry cannot be modified at all
		//
		public virtual bool IsReadOnly {
			get {
				if (property_descriptor == null || property_descriptor.IsReadOnly)
					return true;
				else if (GetEditor() == null && property_descriptor.Converter == null)
					return true;
				else if (!property_descriptor.Converter.GetStandardValuesSupported () &&
					 !property_descriptor.Converter.CanConvertFrom ((ITypeDescriptorContext)this, 
											typeof (string)) &&
					 this.EditorStyle == UITypeEditorEditStyle.None) {
					return true;
				} else
					return false;
			}
		}
		public virtual bool PaintValueSupported {
			get {

				UITypeEditor editor = GetEditor ();
				if (editor != null)
					return editor.GetPaintValueSupported ();
				return false;
			}
		}

		public virtual void PaintValue (Graphics gfx, Rectangle rect)
		{
			UITypeEditor editor = GetEditor ();
			if (editor != null) {
				try {
					editor.PaintValue (this.Value, gfx, rect);
				} catch {
					// TODO
				}
			}
		}

	}
}
