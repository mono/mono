//------------------------------------------------------------------------------
// <copyright file="SubMenuStyleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    public class SubMenuStyleCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(SubMenuStyle) };

        internal SubMenuStyleCollection() {
        }

        protected override void OnInsert(int index, object value) {
            base.OnInsert(index, value);
            if (value is SubMenuStyle) {
                SubMenuStyle style = (SubMenuStyle)value;
                style.Font.Underline = style.Font.Underline;
            }
            else {
                throw new ArgumentException(SR.GetString(SR.SubMenuStyleCollection_InvalidArgument), "value");
            }
        }
        
        public SubMenuStyle this[int i] {
            get {
                return (SubMenuStyle)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }

        public int Add(SubMenuStyle style) {
            return ((IList)this).Add(style);
        }

        public bool Contains(SubMenuStyle style) {
            return ((IList)this).Contains(style);
        }

        public void CopyTo(SubMenuStyle[] styleArray, int index) {
            base.CopyTo(styleArray, index);
        }

        public int IndexOf(SubMenuStyle style) {
            return ((IList)this).IndexOf(style);
        }

        public void Insert(int index, SubMenuStyle style) {
            ((IList)this).Insert(index, style);
        }

        protected override object CreateKnownType(int index) {
            return new SubMenuStyle();
        }

        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        public void Remove(SubMenuStyle style) {
            ((IList)this).Remove(style);
        }

        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            if (o is SubMenuStyle) {
                ((SubMenuStyle)o).SetDirty();
            }
        }
    }
}

