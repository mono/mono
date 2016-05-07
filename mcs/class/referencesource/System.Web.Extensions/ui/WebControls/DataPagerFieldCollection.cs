//------------------------------------------------------------------------------
// <copyright file="DataPagerFieldCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.Resources;
using System.Web.Security;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    /// <summary>
    /// Summary description for DataPagerFieldCollection
    /// </summary>
    public class DataPagerFieldCollection : StateManagedCollection {
        private DataPager _dataPager;
        private static readonly Type[] knownTypes = new Type[] {
                                                                typeof(NextPreviousPagerField),
                                                                typeof(NumericPagerField),
                                                                typeof(TemplatePagerField)
                                                            };

        public event EventHandler FieldsChanged;

        public DataPagerFieldCollection(DataPager dataPager) {
            _dataPager = dataPager;
        }

        /// <devdoc>
        /// <para>Gets a <see cref='System.Web.UI.WebControls.DataPagerField'/> at the specified index in the 
        /// collection.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public DataPagerField this[int index] {
            get {
                return ((IList)this)[index] as DataPagerField;
            }
        }


        /// <devdoc>
        /// <para>Appends a <see cref='System.Web.UI.WebControls.DataPagerField'/> to the collection.</para>
        /// </devdoc>
        public void Add(DataPagerField field) {
            ((IList)this).Add(field);
        }

        /// <devdoc>
        /// <para>Provides a deep copy of the collection.  Used mainly by design time dialogs to implement "cancel" rollback behavior.</para>
        /// </devdoc>
        public DataPagerFieldCollection CloneFields(DataPager pager) {

            DataPagerFieldCollection fields = new DataPagerFieldCollection(pager);
            foreach (DataPagerField field in this) {
                fields.Add(field.CloneField());
            }
            return fields;
        }


        /// <devdoc>
        /// <para>Returns whether a DataPagerField is a member of the collection.</para>
        /// </devdoc>
        public bool Contains(DataPagerField field) {
            return ((IList)this).Contains(field);
        }


        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending at 
        /// the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(DataPagerField[] array, int index) {
            ((IList)this).CopyTo(array, index);
            return;
        }


        /// <devdoc>
        /// <para>Creates a known type of DataPagerField.</para>
        /// </devdoc>
        protected override object CreateKnownType(int index) {
            switch (index) {
                case 0:
                    return new NextPreviousPagerField();
                case 1:
                    return new NumericPagerField();
                case 2:
                    return new TemplatePagerField();
                default:
                    throw new ArgumentOutOfRangeException(AtlasWeb.PagerFieldCollection_InvalidTypeIndex);
            }
        }


        /// <devdoc>
        /// <para>Returns an ArrayList of known DataPagerField types.</para>
        /// </devdoc>
        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }


        /// <devdoc>
        /// <para>Returns the index of the first occurrence of a value in a <see cref='System.Web.UI.WebControls.DataPagerField'/>.</para>
        /// </devdoc>
        public int IndexOf(DataPagerField field) {
            return ((IList)this).IndexOf(field);
        }


        /// <devdoc>
        /// <para>Inserts a <see cref='System.Web.UI.WebControls.DataPagerField'/> to the collection 
        /// at the specified index.</para>
        /// </devdoc>
        public void Insert(int index, DataPagerField field) {
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
            DataPagerField field = value as DataPagerField;
            if (field != null) {
                field.FieldChanged += new EventHandler(OnFieldChanged);
            }
            field.SetDataPager(_dataPager);
            OnFieldsChanged();
        }


        /// <devdoc>
        /// Called when the Remove() method is complete.
        /// </devdoc>
        protected override void OnRemoveComplete(int index, object value) {
            DataPagerField field = value as DataPagerField;
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
            if (!(o is DataPagerField))
                throw new ArgumentException(AtlasWeb.PagerFieldCollection_InvalidType);
        }


        /// <devdoc>
        /// <para>Removes a <see cref='System.Web.UI.WebControls.DataPagerField'/> from the collection at the specified 
        /// index.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }


        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Web.UI.WebControls.DataPagerField'/> from the collection.</para>
        /// </devdoc>
        public void Remove(DataPagerField field) {
            ((IList)this).Remove(field);
        }


        /// <devdoc>
        /// <para>Marks a DataPagerField as dirty so that it will record its entire state into view state.</para>
        /// </devdoc>
        protected override void SetDirtyObject(object o) {
            ((DataPagerField)o).SetDirty();
        }
    }
}
