//------------------------------------------------------------------------------
// <copyright file="TreeNodeStyleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    public sealed class TreeNodeStyleCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(TreeNodeStyle) };

        internal TreeNodeStyleCollection() {
        }

        protected override void OnInsert(int index, object value) {
            base.OnInsert(index, value);
            if (value is TreeNodeStyle) {
                TreeNodeStyle style = (TreeNodeStyle)value;
                style.Font.Underline = style.Font.Underline;
            }
            else {
                throw new ArgumentException(SR.GetString(SR.TreeNodeStyleCollection_InvalidArgument), "value");
            }
        }

        public TreeNodeStyle this[int i] {
            get {
                return (TreeNodeStyle)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }

        public int Add(TreeNodeStyle style) {
            return ((IList)this).Add(style);
        }

        public bool Contains(TreeNodeStyle style) {
            return ((IList)this).Contains(style);
        }

        public void CopyTo(TreeNodeStyle[] styleArray, int index) {
            base.CopyTo(styleArray, index);
        }

        public int IndexOf(TreeNodeStyle style) {
            return ((IList)this).IndexOf(style);
        }

        public void Insert(int index, TreeNodeStyle style) {
            ((IList)this).Insert(index, style);
        }

        protected override object CreateKnownType(int index) {
            return new TreeNodeStyle();
        }

        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        public void Remove(TreeNodeStyle style) {
            ((IList)this).Remove(style);
        }

        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            if (o is TreeNodeStyle) {
                ((TreeNodeStyle)o).SetDirty();
            }
        }
    }
}

