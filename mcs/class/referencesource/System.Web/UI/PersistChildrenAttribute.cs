//------------------------------------------------------------------------------
// <copyright file="PersistChildrenAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para> 
    ///       Indicates whether
    ///       the contents within a tag representing a custom
    ///       or Web control should be treated as literal text. Web controls supporting complex properties, like
    ///       templates, and
    ///       so on, typically mark themselves as "literals", thereby letting the designer
    ///       infra-structure deal with the persistence of those attributes.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PersistChildrenAttribute : Attribute {


        /// <devdoc>
        ///    <para>Indicates that the children of a control should be persisted at design-time.
        ///       </para>
        /// </devdoc>
        public static readonly PersistChildrenAttribute Yes = new PersistChildrenAttribute(true);


        /// <devdoc>
        ///    <para>Indicates that the children of a control should not be persisted at design-time.</para>
        /// </devdoc>
        public static readonly PersistChildrenAttribute No = new PersistChildrenAttribute(false);


        /// <devdoc>
        ///     This marks the default child persistence behavior for a control at design time. (equal to Yes.)
        /// </devdoc>
        public static readonly PersistChildrenAttribute Default = Yes;

        private bool _persist;
        private bool _usesCustomPersistence;


        /// <devdoc>
        /// </devdoc>
        public PersistChildrenAttribute(bool persist) {
            _persist = persist;
        }

        public PersistChildrenAttribute(bool persist, bool usesCustomPersistence) : this(persist) {
            _usesCustomPersistence = usesCustomPersistence;
        }


        /// <devdoc>
        ///    <para>Indicates whether the children of a control should be persisted at design-time.
        ///       This property is read-only.</para>
        /// </devdoc>
        public bool Persist {
            get {
                return _persist;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the control does custom persistence.
        ///       This property is read-only.</para>
        /// </devdoc>
        public bool UsesCustomPersistence {
            get {
                // if persist is true, we don't use custom persistence.
                return !_persist && _usesCustomPersistence;
            }
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override int GetHashCode() {
            return Persist.GetHashCode();
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            if ((obj != null) && (obj is PersistChildrenAttribute)) {
                return ((PersistChildrenAttribute)obj).Persist == _persist;
            }

            return false;
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
