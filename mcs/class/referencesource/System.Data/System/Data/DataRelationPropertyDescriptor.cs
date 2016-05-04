//------------------------------------------------------------------------------
// <copyright file="DataRelationPropertyDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>    
    internal sealed class DataRelationPropertyDescriptor : PropertyDescriptor {

        DataRelation relation;

        internal DataRelation Relation {
            get {
                return relation;
            }
        }

        internal DataRelationPropertyDescriptor(DataRelation dataRelation) : base(dataRelation.RelationName, null) {
            this.relation = dataRelation; 
        }

        public override Type ComponentType {
            get {
                return typeof(DataRowView);
            }
        }

        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        public override Type PropertyType {
            get {
                return typeof(IBindingList);
            }
        }

        public override bool Equals(object other) {
            if (other is DataRelationPropertyDescriptor) {
                DataRelationPropertyDescriptor descriptor = (DataRelationPropertyDescriptor) other;
                return(descriptor.Relation == Relation);
            }
            return false;
        }

        public override Int32 GetHashCode() {
            return Relation.GetHashCode();
        }

        public override bool CanResetValue(object component) {
            return false;
        }

        public override object GetValue(object component) {
            DataRowView dataRowView = (DataRowView) component;
            return dataRowView.CreateChildView(relation);
        }

        public override void ResetValue(object component) {
        }

        public override void SetValue(object component, object value) {
        }

        public override bool ShouldSerializeValue(object component) {
            return false;
        }
    }   
}

