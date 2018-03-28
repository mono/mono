//------------------------------------------------------------------------------
// <copyright file="HtmlControlPersistable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.ComponentModel;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class HtmlControlPersistableAttribute : Attribute {

        internal static readonly HtmlControlPersistableAttribute Yes = new HtmlControlPersistableAttribute(true);
        internal static readonly HtmlControlPersistableAttribute No =  new HtmlControlPersistableAttribute(false);
        internal static readonly HtmlControlPersistableAttribute Default = Yes;
        private bool persistable = true;

        internal HtmlControlPersistableAttribute(bool persistable) {
            this.persistable = persistable;
        }

        internal bool HtmlControlPersistable {
            get {
                return persistable;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            HtmlControlPersistableAttribute other = obj as HtmlControlPersistableAttribute;
            return (other != null) && other.HtmlControlPersistable == persistable;
        }

        public override int GetHashCode() {
            return persistable.GetHashCode();
        }

        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
    }
}
