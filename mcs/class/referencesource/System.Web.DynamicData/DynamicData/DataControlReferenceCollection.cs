namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;    
    using System.Security.Permissions;    

    /// <summary>
    /// Represents a collectin of DataControlReferences
    /// </summary>
    public class DataControlReferenceCollection : Collection<DataControlReference> {               
        public DataControlReferenceCollection(DynamicDataManager owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }

            Owner = owner;
        }

        public DynamicDataManager Owner {
            get;
            private set;
        }

        internal void Initialize() {
            foreach (DataControlReference reference in this) {
                reference.Owner = Owner;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void SetItem(int index, DataControlReference item) {
            item.Owner = Owner;
            base.SetItem(index, item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void InsertItem(int index, DataControlReference item) {
            item.Owner = Owner;
            base.InsertItem(index, item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void RemoveItem(int index) {
            this[index].Owner = null;
            base.RemoveItem(index);
        }
    }
}
