//
// System.ComponentModel.Design.CollectionEditor
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//      Ivan N. Zlatev (contact@i-nz.net)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2007 Andreas Nahr
// (C) 2007 Ivan N. Zlatev
// (C) 2008 Novell, Inc
//

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
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace System.ComponentModel.Design
{
	public class CollectionEditor : UITypeEditor
	{
		protected abstract class CollectionForm : Form
		{
			private CollectionEditor editor;
			private object editValue;

			public CollectionForm (CollectionEditor editor)
			{
				this.editor = editor;
			}

			protected Type CollectionItemType
			{
				get { return editor.CollectionItemType; }
			}

			protected Type CollectionType
			{
				get { return editor.CollectionType; }
			}

			protected ITypeDescriptorContext Context
			{
				get { return editor.Context; }
			}

			public object EditValue
			{
				get { return editValue; }
				set
				{
					editValue = value;
					OnEditValueChanged ();
				}
			}

			protected object[] Items
			{
				get { return editor.GetItems (editValue); }
				set {
					if (editValue == null) {
						object newEmptyCollection = null;
						try {
							if (typeof (Array).IsAssignableFrom (CollectionType))
								newEmptyCollection = Array.CreateInstance (CollectionItemType, 0);
							else
								newEmptyCollection = Activator.CreateInstance (CollectionType);
						} catch {}

						object val = editor.SetItems (newEmptyCollection, value);
						if (val != newEmptyCollection)
							EditValue = val;
					} else {
						object val = editor.SetItems (editValue, value);
						if (val != editValue)
							EditValue = val;
					}
				}
			}

			protected Type[] NewItemTypes
			{
				get { return editor.NewItemTypes; }
			}

			protected bool CanRemoveInstance (object value)
			{
				return editor.CanRemoveInstance (value);
			}

			protected virtual bool CanSelectMultipleInstances ()
			{
				return editor.CanSelectMultipleInstances ();
			}

			protected object CreateInstance (Type itemType)
			{
				return editor.CreateInstance (itemType);
			}

			protected void DestroyInstance (object instance)
			{
				editor.DestroyInstance (instance);
			}

			protected virtual void DisplayError (Exception e)
			{
				MessageBox.Show (e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			protected override object GetService (Type serviceType)
			{
				return editor.GetService (serviceType);
			}

			protected abstract void OnEditValueChanged ();

			protected internal virtual DialogResult ShowEditorDialog (IWindowsFormsEditorService edSvc)
			{
				return edSvc.ShowDialog (this);
			}
		}

		private class ConcreteCollectionForm : CollectionForm
		{
			internal class ObjectContainerConverter : TypeConverter
			{
				private class ObjectContainerPropertyDescriptor : TypeConverter.SimplePropertyDescriptor
				{
					private AttributeCollection attributes;

					public ObjectContainerPropertyDescriptor (Type componentType, Type propertyType)
						: base (componentType, "Value", propertyType)
					{
						CategoryAttribute cat = new CategoryAttribute (propertyType.Name);
						attributes = new AttributeCollection (new Attribute[] { cat });
					}

					public override object GetValue (object component)
					{
						ObjectContainer container = (ObjectContainer)component;
						return container.Object;
					}

					public override void SetValue (object component, object value)
					{
						ObjectContainer container = (ObjectContainer)component;
						container.Object = value;
					}

					public override AttributeCollection Attributes
					{
						get { return attributes; }
					}
				}

				public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context, object value, Attribute[] attributes)
				{
					ObjectContainer container = (ObjectContainer)value;
					ObjectContainerPropertyDescriptor desc = new ObjectContainerPropertyDescriptor (value.GetType (), container.editor.CollectionItemType);
					PropertyDescriptor[] properties = new PropertyDescriptor[] { desc };
					PropertyDescriptorCollection pc = new PropertyDescriptorCollection (properties);
					return pc;
				}

				public override bool GetPropertiesSupported (ITypeDescriptorContext context)
				{
					return true;
				}
			}

			[TypeConverter (typeof (ObjectContainerConverter))]
			private class ObjectContainer
			{
				internal object Object;
				internal CollectionEditor editor;

				public ObjectContainer (object obj, CollectionEditor editor)
				{
					this.Object = obj;
					this.editor = editor;
				}

				internal string Name {
					get { return editor.GetDisplayText (Object); }
				}

				public override string ToString ()
				{
					return Name;
				}
			}

			private class UpdateableListbox : ListBox
			{
				public void DoRefreshItem (int index)
				{
					base.RefreshItem (index);
				}
			}

			private CollectionEditor editor;

			private System.Windows.Forms.Label labelMember;
			private System.Windows.Forms.Label labelProperty;
			private UpdateableListbox itemsList;
			private System.Windows.Forms.PropertyGrid itemDisplay;
			private System.Windows.Forms.Button doClose;
			private System.Windows.Forms.Button moveUp;
			private System.Windows.Forms.Button moveDown;
			private System.Windows.Forms.Button doAdd;
			private System.Windows.Forms.Button doRemove;
			private System.Windows.Forms.Button doCancel;
			private System.Windows.Forms.ComboBox addType;

			public ConcreteCollectionForm (CollectionEditor editor)
				: base (editor)
			{
				this.editor = editor;

				this.labelMember = new System.Windows.Forms.Label ();
				this.labelProperty = new System.Windows.Forms.Label ();
				this.itemsList = new UpdateableListbox ();
				this.itemDisplay = new System.Windows.Forms.PropertyGrid ();
				this.doClose = new System.Windows.Forms.Button ();
				this.moveUp = new System.Windows.Forms.Button ();
				this.moveDown = new System.Windows.Forms.Button ();
				this.doAdd = new System.Windows.Forms.Button ();
				this.doRemove = new System.Windows.Forms.Button ();
				this.doCancel = new System.Windows.Forms.Button ();
				this.addType = new System.Windows.Forms.ComboBox ();
				this.SuspendLayout ();
				// 
				// labelMember
				// 
				this.labelMember.Location = new System.Drawing.Point (12, 9);
				this.labelMember.Size = new System.Drawing.Size (55, 13);
				this.labelMember.Text = "Members:";
				// 
				// labelProperty
				// 
				this.labelProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
				this.labelProperty.Location = new System.Drawing.Point (172, 9);
				this.labelProperty.Size = new System.Drawing.Size (347, 13);
				this.labelProperty.Text = "Properties:";
				// 
				// itemsList
				// 
				this.itemsList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)));
				this.itemsList.HorizontalScrollbar = true;
				this.itemsList.Location = new System.Drawing.Point (12, 25);
				this.itemsList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
				this.itemsList.Size = new System.Drawing.Size (120, 290);
				this.itemsList.TabIndex = 0;
				this.itemsList.SelectedIndexChanged += new System.EventHandler (this.itemsList_SelectedIndexChanged);
				// 
				// itemDisplay
				// 
				this.itemDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
				this.itemDisplay.HelpVisible = false;
				this.itemDisplay.Location = new System.Drawing.Point (175, 25);
				this.itemDisplay.Size = new System.Drawing.Size (344, 314);
				this.itemDisplay.TabIndex = 6;
				this.itemDisplay.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler (this.itemDisplay_PropertyValueChanged);
				// 
				// doClose
				// 
				this.doClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.doClose.Location = new System.Drawing.Point (341, 345);
				this.doClose.Size = new System.Drawing.Size (86, 26);
				this.doClose.TabIndex = 7;
				this.doClose.Text = "OK";
				this.doClose.Click += new System.EventHandler (this.doClose_Click);
				// 
				// moveUp
				// 
				this.moveUp.Location = new System.Drawing.Point (138, 25);
				this.moveUp.Size = new System.Drawing.Size (31, 28);
				this.moveUp.TabIndex = 4;
				this.moveUp.Enabled = false;
				this.moveUp.Text = "Up";
				this.moveUp.Click += new System.EventHandler (this.moveUp_Click);
				// 
				// moveDown
				// 
				this.moveDown.Location = new System.Drawing.Point (138, 59);
				this.moveDown.Size = new System.Drawing.Size (31, 28);
				this.moveDown.TabIndex = 5;
				this.moveDown.Enabled = false;
				this.moveDown.Text = "Dn";
				this.moveDown.Click += new System.EventHandler (this.moveDown_Click);
				// 
				// doAdd
				// 
				this.doAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.doAdd.Location = new System.Drawing.Point (12, 346);
				this.doAdd.Size = new System.Drawing.Size (59, 25);
				this.doAdd.TabIndex = 1;
				this.doAdd.Text = "Add";
				this.doAdd.Click += new System.EventHandler (this.doAdd_Click);
				// 
				// doRemove
				// 
				this.doRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.doRemove.Location = new System.Drawing.Point (77, 346);
				this.doRemove.Size = new System.Drawing.Size (55, 25);
				this.doRemove.TabIndex = 2;
				this.doRemove.Text = "Remove";
				this.doRemove.Click += new System.EventHandler (this.doRemove_Click);
				// 
				// doCancel
				// 
				this.doCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
				this.doCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.doCancel.Location = new System.Drawing.Point (433, 345);
				this.doCancel.Size = new System.Drawing.Size (86, 26);
				this.doCancel.TabIndex = 8;
				this.doCancel.Text = "Cancel";
				this.doCancel.Click += new System.EventHandler (this.doCancel_Click);
				// 
				// addType
				// 
				this.addType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
				this.addType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
				this.addType.Location = new System.Drawing.Point (12, 319);
				this.addType.Size = new System.Drawing.Size (120, 21);
				this.addType.TabIndex = 3;
				// 
				// DesignerForm
				// 
				this.AcceptButton = this.doClose;
				this.CancelButton = this.doCancel;
				this.ClientSize = new System.Drawing.Size (531, 381);
				this.ControlBox = false;
				this.Controls.Add (this.addType);
				this.Controls.Add (this.doCancel);
				this.Controls.Add (this.doRemove);
				this.Controls.Add (this.doAdd);
				this.Controls.Add (this.moveDown);
				this.Controls.Add (this.moveUp);
				this.Controls.Add (this.doClose);
				this.Controls.Add (this.itemDisplay);
				this.Controls.Add (this.itemsList);
				this.Controls.Add (this.labelProperty);
				this.Controls.Add (this.labelMember);
				this.HelpButton = true;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.MinimumSize = new System.Drawing.Size (400, 300);
				this.ShowInTaskbar = false;
				this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
				this.ResumeLayout (false);

#if NET_2_0
				if (editor.CollectionType.IsGenericType)
					this.Text = editor.CollectionItemType.Name + " Collection Editor";
				else
					this.Text = editor.CollectionType.Name + " Collection Editor";
#else
				this.Text = editor.CollectionType.Name + " Collection Editor";
#endif
				foreach (Type type in editor.NewItemTypes)
					addType.Items.Add (type.Name);
				if (addType.Items.Count > 0)
					addType.SelectedIndex = 0;
			}

			private void UpdateItems ()
			{
				object[] items = editor.GetItems (EditValue);
				if (items != null) {
					itemsList.BeginUpdate ();
					itemsList.Items.Clear ();
					foreach (object o in items)
						this.itemsList.Items.Add (new ObjectContainer (o, editor));
					if (itemsList.Items.Count > 0)
						itemsList.SelectedIndex = 0;
					itemsList.EndUpdate ();
				}
			}

			private void doClose_Click (object sender, EventArgs e)
			{
				SetEditValue ();
				this.Close ();
			}

			private void SetEditValue ()
			{
				object[] items = new object[itemsList.Items.Count];
				for (int i = 0; i < itemsList.Items.Count; i++)
					items[i] = ((ObjectContainer)itemsList.Items[i]).Object;
				this.Items = items;
			}

			private void doCancel_Click (object sender, EventArgs e)
			{
				editor.CancelChanges ();
				this.Close ();
			}

			private void itemsList_SelectedIndexChanged (object sender, EventArgs e)
			{
				if (itemsList.SelectedIndex == -1) {
					itemDisplay.SelectedObject = null;
					return;
				}

				if (itemsList.SelectedIndex <= 0 || itemsList.SelectedItems.Count > 1)
					moveUp.Enabled = false;
				else
					moveUp.Enabled = true;
				if (itemsList.SelectedIndex > itemsList.Items.Count - 2 || itemsList.SelectedItems.Count > 1)
					moveDown.Enabled = false;
				else
					moveDown.Enabled = true;

				if (itemsList.SelectedItems.Count == 1)
				{
					ObjectContainer o = (ObjectContainer)itemsList.SelectedItem;
					if (Type.GetTypeCode (o.Object.GetType ()) != TypeCode.Object)
						itemDisplay.SelectedObject = o;
					else
						itemDisplay.SelectedObject = o.Object;
				}
				else
				{
					object[] items = new object[itemsList.SelectedItems.Count];
					for (int i = 0; i < itemsList.SelectedItems.Count; i++)
					{
						ObjectContainer o = (ObjectContainer)itemsList.SelectedItem;
						if (Type.GetTypeCode (o.Object.GetType ()) != TypeCode.Object)
							items[i] = ((ObjectContainer)itemsList.SelectedItems[i]);
						else
							items[i] = ((ObjectContainer)itemsList.SelectedItems[i]).Object;
					}
					itemDisplay.SelectedObjects = items;
				}
				labelProperty.Text = ((ObjectContainer)itemsList.SelectedItem).Name + " properties:";
			}

			private void itemDisplay_PropertyValueChanged (object sender, EventArgs e)
			{
				int[] selected = new int[itemsList.SelectedItems.Count];
				for (int i = 0; i < itemsList.SelectedItems.Count; i++)
					selected[i] = itemsList.Items.IndexOf (itemsList.SelectedItems[i]);

				// The list might be repopulated if a new instance of the collection edited
				// is created during the update. This happen for example for Arrays.
				SetEditValue ();

				// Restore current selection in case the list gets repopulated.
				// Refresh the item after that to reflect possible value change.
				// 
				itemsList.BeginUpdate ();
				itemsList.ClearSelected ();
				foreach (int index in selected) {
					itemsList.DoRefreshItem (index);
					itemsList.SetSelected (index, true);
				}
				itemsList.SelectedIndex = selected[0];
				itemsList.EndUpdate ();
			}

			private void moveUp_Click (object sender, EventArgs e)
			{
				if (itemsList.SelectedIndex <= 0)
					return;

				object selected = itemsList.SelectedItem;
				int index = itemsList.SelectedIndex;
				itemsList.Items.RemoveAt (index);
				itemsList.Items.Insert (index - 1, selected);
				itemsList.SelectedIndex = index - 1;
			}

			private void moveDown_Click (object sender, EventArgs e)
			{
				if (itemsList.SelectedIndex > itemsList.Items.Count - 2)
					return;

				object selected = itemsList.SelectedItem;
				int index = itemsList.SelectedIndex;
				itemsList.Items.RemoveAt (index);
				itemsList.Items.Insert (index + 1, selected);
				itemsList.SelectedIndex = index + 1;
			}

			private void doAdd_Click (object sender, EventArgs e)
			{
				object o;
				try {
					o = editor.CreateInstance (editor.NewItemTypes[addType.SelectedIndex]);
				} catch (Exception ex) {
					DisplayError (ex);
					return;
				}
				itemsList.Items.Add (new ObjectContainer (o, editor));
				itemsList.SelectedIndex = -1;
				itemsList.SelectedIndex = itemsList.Items.Count - 1;
			}

			private void doRemove_Click (object sender, EventArgs e)
			{
				if (itemsList.SelectedIndex != -1) {
					int[] selected = new int[itemsList.SelectedItems.Count];
					for (int i=0; i < itemsList.SelectedItems.Count; i++)
						selected[i] = itemsList.Items.IndexOf (itemsList.SelectedItems[i]);

					for (int i = selected.Length - 1; i >= 0; i--)
						itemsList.Items.RemoveAt (selected[i]);

					itemsList.SelectedIndex = Math.Min (selected[0], itemsList.Items.Count-1);
				}
			}

			// OnEditValueChanged is called only if the  EditValue has changed,
			// which is only in the case when a new instance of the collection is 
			// required, e.g for arrays.
			// 
			protected override void OnEditValueChanged ()
			{
				UpdateItems ();
			}
		}

		private Type type;
		private Type collectionItemType;
		private Type[] newItemTypes;
		private ITypeDescriptorContext context;
		private IServiceProvider provider;
		private IWindowsFormsEditorService editorService;

		public CollectionEditor (Type type)
		{
			this.type = type;
			this.collectionItemType = CreateCollectionItemType ();
			this.newItemTypes = CreateNewItemTypes ();
		}

		protected Type CollectionItemType
		{
			get { return collectionItemType; }
		}

		protected Type CollectionType
		{
			get { return type; }
		}

		protected ITypeDescriptorContext Context
		{
			get { return context; }
		}

		protected virtual string HelpTopic
		{
			get { return "CollectionEditor"; }
		}

		protected Type[] NewItemTypes
		{
			get { return newItemTypes; }
		}

		protected virtual void CancelChanges ()
		{
		}

		protected virtual bool CanRemoveInstance (object value)
		{
			return true;
		}

		protected virtual bool CanSelectMultipleInstances ()
		{
			return true;
		}

		protected virtual CollectionEditor.CollectionForm CreateCollectionForm ()
		{
			return new ConcreteCollectionForm (this);
		}

		protected virtual Type CreateCollectionItemType ()
		{
			PropertyInfo[] properties = type.GetProperties ();
			foreach (PropertyInfo property in properties)
				if (property.Name == "Item")
					return property.PropertyType;
			return typeof (object);
		}
		
		protected virtual object CreateInstance (Type itemType)
		{
			object instance = null;
			if (typeof (IComponent).IsAssignableFrom (itemType)) {
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (host != null)
					instance = host.CreateComponent (itemType);
			}

			if (instance == null) {
#if NET_2_0
				instance = TypeDescriptor.CreateInstance (provider, itemType, null, null);
#else
				instance =  Activator.CreateInstance (itemType);
#endif
			}
			return instance;
		}
		
		protected virtual Type[] CreateNewItemTypes ()
		{
			return new Type[] { collectionItemType };
		}

		protected virtual void DestroyInstance (object instance)
		{
			IComponent component = instance as IComponent;
			if (component != null) {
				IDesignerHost host = GetService (typeof (IDesignerHost)) as IDesignerHost;
				if (host != null)
					host.DestroyComponent (component);
			}
		}

		public override object EditValue (ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			this.context = context;
			this.provider = provider;

			if (context != null && provider != null)
			{
				editorService = (IWindowsFormsEditorService)provider.GetService (typeof (IWindowsFormsEditorService));
				if (editorService != null)
				{
					CollectionForm editorForm = CreateCollectionForm ();
					editorForm.EditValue = value;
					editorForm.ShowEditorDialog (editorService);
					return editorForm.EditValue;
				}
			}
			return base.EditValue (context, provider, value);
		}

		protected virtual string GetDisplayText (object value)
		{
			if (value == null)
				return string.Empty;

			PropertyInfo nameProperty = value.GetType ().GetProperty ("Name");
			if (nameProperty != null)
			{
				string data = (nameProperty.GetValue (value, null)) as string;
				if (data != null)
					if (data.Length != 0)
						return data;
			}

			if (Type.GetTypeCode (value.GetType ()) == TypeCode.Object)
				return value.GetType ().Name;
			else
				return value.ToString ();
		}

		public override UITypeEditorEditStyle GetEditStyle (ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		protected virtual object[] GetItems (object editValue)
		{
			if (editValue == null)
				return new object[0];
			ICollection collection = editValue as ICollection;
			if (collection == null)
				return new object[0];

			object[] result = new object[collection.Count];
			collection.CopyTo (result, 0);
			return result;
		}

		protected virtual IList GetObjectsFromInstance (object instance)
		{
			ArrayList list = new ArrayList ();
			list.Add (instance);
			return list;
		}

		protected object GetService (Type serviceType)
		{
			return context.GetService (serviceType);
		}

		protected virtual object SetItems (object editValue, object[] value)
		{
			IList list = (IList) editValue;
			if (list == null)
				return null;

			list.Clear ();
			foreach (object o in value)
				list.Add (o);

			return list;
		}

		protected virtual void ShowHelp ()
		{
			//TODO: Fixme Add parent and URL
			Help.ShowHelp (null, "", HelpTopic);
		}
	}
}
