//------------------------------------------------------------------------------
// <copyright file="ParseChildrenAsPropertiesAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.Util;

    /// <devdoc>
    /// Define the metadata attribute that controls use to mark the fact
    /// that their children are in fact properties.
    /// Furthermore, if a string is passed in the constructor, it specifies
    /// the name of the defaultproperty.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ParseChildrenAttribute : Attribute {


        public static readonly ParseChildrenAttribute ParseAsChildren = new ParseChildrenAttribute(false, false);


        public static readonly ParseChildrenAttribute ParseAsProperties = new ParseChildrenAttribute(true, false);


        public static readonly ParseChildrenAttribute Default = ParseAsChildren;

        private bool _childrenAsProps;
        private string _defaultProperty;
        private Type _childControlType;

        private bool _allowChanges = true;


        /// <devdoc>
        /// Needed to use named parameters (ASURT 78869)
        /// </devdoc>
        public ParseChildrenAttribute() : this(false, null) {
        }


        /// <devdoc>
        /// </devdoc>
        public ParseChildrenAttribute(bool childrenAsProperties) : this(childrenAsProperties, null) {
        }

        public ParseChildrenAttribute(Type childControlType) : this(false, null) {
            if (childControlType == null) {
                throw new ArgumentNullException("childControlType");
            }

            _childControlType = childControlType;
        }

        /// <devdoc>
        /// Needed to create immutable static readonly instances of this attribute
        /// </devdoc>
        private ParseChildrenAttribute(bool childrenAsProperties, bool allowChanges) : this(childrenAsProperties, null) {
            _allowChanges  = allowChanges;
        }


        /// <devdoc>
        /// </devdoc>
        public ParseChildrenAttribute(bool childrenAsProperties, string defaultProperty) {
            _childrenAsProps = childrenAsProperties;
            if (_childrenAsProps == true) {
                _defaultProperty = defaultProperty;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the allowed child control type.
        ///       This property is read-only.</para>
        /// </devdoc>
        public Type ChildControlType {
            get {
                if (_childControlType == null) {
                    return typeof(System.Web.UI.Control);
                }

                return _childControlType;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool ChildrenAsProperties {
            get {
                return _childrenAsProps;
            }
            set {
                if (_allowChanges == false) {
                    throw new NotSupportedException();
                }
                _childrenAsProps = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public string DefaultProperty {
            get {
                if (_defaultProperty == null) {
                    return String.Empty;
                }
                return _defaultProperty;
            }
            set {
                if (_allowChanges == false) {
                    throw new NotSupportedException();
                }
                _defaultProperty = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override int GetHashCode() {
            if (_childrenAsProps == false) {
                return HashCodeCombiner.CombineHashCodes(_childrenAsProps.GetHashCode(), _childControlType.GetHashCode());
            }
            else {
                return HashCodeCombiner.CombineHashCodes(_childrenAsProps.GetHashCode(), DefaultProperty.GetHashCode());
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            ParseChildrenAttribute pca = obj as ParseChildrenAttribute;
            if (pca != null) {
                if (_childrenAsProps == false) {
                    return pca.ChildrenAsProperties == false && 
                        pca._childControlType == _childControlType;
                }
                else {
                    return pca.ChildrenAsProperties && (DefaultProperty.Equals(pca.DefaultProperty));
                }
            }
            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
