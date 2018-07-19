//------------------------------------------------------------------------------
// <copyright file="DataControlFieldCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Represents the collection of fields to be displayed in 
    /// a data bound control that uses fields.</para>
    /// </devdoc>
    public sealed class DataControlFieldCollection : StateManagedCollection {

        private static readonly Type[] knownTypes = new Type[] {
                                                                typeof(BoundField),
                                                                typeof(ButtonField),
                                                                typeof(CheckBoxField),
                                                                typeof(CommandField),
                                                                typeof(HyperLinkField),
                                                                typeof(ImageField),
                                                                typeof(TemplateField)
                                                            };
        

        public event EventHandler FieldsChanged;



        /// <devdoc>
        /// <para>Gets a <see cref='System.Web.UI.WebControls.DataControlField'/> at the specified index in the 
        /// collection.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public DataControlField this[int index] {
            get {
                return ((IList)this)[index] as DataControlField;
            }
        }


        /// <devdoc>
        /// <para>Appends a <see cref='System.Web.UI.WebControls.DataControlField'/> to the collection.</para>
        /// </devdoc>
        public void Add(DataControlField field) {
            ((IList)this).Add(field);
        }

        /// <devdoc>
        /// <para>Provides a deep copy of the collection.  Used mainly by design time dialogs to implement "cancel" rollback behavior.</para>
        /// </devdoc>
        public DataControlFieldCollection CloneFields() {
            DataControlFieldCollection fields = new DataControlFieldCollection();
            foreach (DataControlField field in this) {
                fields.Add(field.CloneField());
            }
            return fields;
        }


        /// <devdoc>
        /// <para>Returns whether a DataControlField is a member of the collection.</para>
        /// </devdoc>
        public bool Contains(DataControlField field) {
            return ((IList)this).Contains(field);
        }


        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending at 
        /// the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(DataControlField[] array, int index) {
            ((IList)this).CopyTo(array, index);
            return;
        }


        /// <devdoc>
        /// <para>Creates a known type of DataControlField.</para>
        /// </devdoc>
        protected override object CreateKnownType(int index) {
             switch (index) {
                 case 0:
                     return new BoundField();
                 case 1:
                     return new ButtonField();
                 case 2:
                     return new CheckBoxField();
                 case 3:
                     return new CommandField();
                 case 4:
                     return new HyperLinkField();
                 case 5:
                     return new ImageField();
                 case 6:
                     return new TemplateField();
                 default:
                     throw new ArgumentOutOfRangeException(SR.GetString(SR.DataControlFieldCollection_InvalidTypeIndex));
            }        
        }


        /// <devdoc>
        /// <para>Returns an ArrayList of known DataControlField types.</para>
        /// </devdoc>
        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }


        /// <devdoc>
        /// <para>Returns the index of the first occurrence of a value in a <see cref='System.Web.UI.WebControls.DataControlField'/>.</para>
        /// </devdoc>
        public int IndexOf(DataControlField field) {
            return ((IList)this).IndexOf(field);
        }


        /// <devdoc>
        /// <para>Inserts a <see cref='System.Web.UI.WebControls.DataControlField'/> to the collection 
        /// at the specified index.</para>
        /// </devdoc>
        public void Insert(int index, DataControlField field) {
            ((IList)this).Insert(index, field);
        }


        /// <devdoc>
        /// Called when the Clear() method is complete.
        /// </devdoc>
        protected override void OnClearComplete() { 
            OnFieldsChanged();
        }
        
        /// <devdoc>
        /// </devdoc>
        void OnFieldChanged(object sender, EventArgs e) {
            OnFieldsChanged();
        }

        /// <devdoc>
        /// </devdoc>
        void OnFieldsChanged() {
            if (FieldsChanged != null) {
                FieldsChanged(this, EventArgs.Empty);
            }
        }


        /// <devdoc>
        /// Called when the Insert() method is complete.
        /// </devdoc>
        protected override void OnInsertComplete(int index, object value) { 
            DataControlField field = value as DataControlField;
            if (field != null) {
                field.FieldChanged += new EventHandler(OnFieldChanged);
            }
            OnFieldsChanged();
        }


        /// <devdoc>
        /// Called when the Remove() method is complete.
        /// </devdoc>
        protected override void OnRemoveComplete(int index, object value) { 
            DataControlField field = value as DataControlField;
            if (field != null) {
                field.FieldChanged -= new EventHandler(OnFieldChanged);
            }
            OnFieldsChanged();
        }


        /// <devdoc>
        /// <para>Validates that an object is a HotSpot.</para>
        /// </devdoc>
        protected override void OnValidate(object o) {
            base.OnValidate(o);
            if (!(o is DataControlField))
                throw new ArgumentException(SR.GetString(SR.DataControlFieldCollection_InvalidType));
        }


        /// <devdoc>
        /// <para>Removes a <see cref='System.Web.UI.WebControls.DataControlField'/> from the collection at the specified 
        /// index.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }


        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Web.UI.WebControls.DataControlField'/> from the collection.</para>
        /// </devdoc>
        public void Remove(DataControlField field) {
            ((IList)this).Remove(field);
        }


        /// <devdoc>
        /// <para>Marks a DataControlField as dirty so that it will record its entire state into view state.</para>
        /// </devdoc>
        protected override void SetDirtyObject(object o) {
            ((DataControlField)o).SetDirty();
        }
    }
}
