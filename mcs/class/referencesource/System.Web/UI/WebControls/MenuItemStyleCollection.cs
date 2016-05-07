//------------------------------------------------------------------------------
// <copyright file="MenuItemStyleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;

    public sealed class MenuItemStyleCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(MenuItemStyle) };

        internal MenuItemStyleCollection() {
        }

        protected override void OnInsert(int index, object value) {
            base.OnInsert(index, value);
            if (value is MenuItemStyle) {
                MenuItemStyle style = (MenuItemStyle)value;
                style.Font.Underline = style.Font.Underline;
            }
            else {
                throw new ArgumentException(SR.GetString(SR.MenuItemStyleCollection_InvalidArgument), "value");
            }
        }

        public MenuItemStyle this[int i] {
            get {
                return (MenuItemStyle)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }

        public int Add(MenuItemStyle style) {
            return ((IList)this).Add(style);
        }

        public bool Contains(MenuItemStyle style) {
            return ((IList)this).Contains(style);
        }

        public void CopyTo(MenuItemStyle[] styleArray, int index) {
            base.CopyTo(styleArray, index);
        }

        public int IndexOf(MenuItemStyle style) {
            return ((IList)this).IndexOf(style);
        }

        public void Insert(int index, MenuItemStyle style) {
            ((IList)this).Insert(index, style);
        }

        protected override object CreateKnownType(int index) {
            return new MenuItemStyle();
        }

        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        public void Remove(MenuItemStyle style) {
            ((IList)this).Remove(style);
        }

        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            if (o is MenuItemStyle) {
                ((MenuItemStyle)o).SetDirty();
            }
        }
    }
}

