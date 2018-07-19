//------------------------------------------------------------------------------
// <copyright file="NonVisualControlAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NonVisualControlAttribute : Attribute {


        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly NonVisualControlAttribute NonVisual = new NonVisualControlAttribute(true);


        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly NonVisualControlAttribute Visual = new NonVisualControlAttribute(false);


        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly NonVisualControlAttribute Default = Visual;

        private bool _nonVisual;


        /// <devdoc>
        /// </devdoc>
        public NonVisualControlAttribute() : this(true) {
        }


        /// <devdoc>
        /// </devdoc>
        public NonVisualControlAttribute(bool nonVisual) {
            _nonVisual = nonVisual;
        }


        /// <devdoc>
        ///    <para>Indicates if the control is non-visual.</para>
        /// </devdoc>
        public bool IsNonVisual {
            get {
                return _nonVisual;
            }
        }


        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            NonVisualControlAttribute other = obj as NonVisualControlAttribute;
            return (other != null) && (other.IsNonVisual == IsNonVisual);
        }


        /// <internalonly/>
        public override int GetHashCode() {
            return _nonVisual.GetHashCode();
        }


        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}

