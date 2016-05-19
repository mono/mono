//------------------------------------------------------------------------------
// <copyright file="BindableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies whether a property is appropriate to bind data
    ///       to.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class BindableAttribute : Attribute {
        /// <devdoc>
        ///    <para>
        ///       Specifies that a property is appropriate to bind data to. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </devdoc>
        public static readonly BindableAttribute Yes = new BindableAttribute(true);

        /// <devdoc>
        ///    <para>
        ///       Specifies that a property is not appropriate to bind
        ///       data to. This <see langword='static '/>field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly BindableAttribute No = new BindableAttribute(false);

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.BindableAttribute'/>,
        ///       which is <see cref='System.ComponentModel.BindableAttribute.No'/>. This <see langword='static '/>field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly BindableAttribute Default = No;

        private bool bindable   = false;
        private bool isDefault  = false;
        private BindingDirection direction;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.BindableAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public BindableAttribute(bool bindable) : this(bindable, BindingDirection.OneWay) {
        }

        /// <devdoc>
        /// <para>
        /// Initializes a new instance of the <see cref='System.ComponentModel.BindableAttribute'/> class.
        /// </para>
        /// </devdoc>
        public BindableAttribute(bool bindable, BindingDirection direction) {
            this.bindable = bindable;
            this.direction = direction;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.BindableAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public BindableAttribute(BindableSupport flags) : this(flags, BindingDirection.OneWay) {
        }

        /// <devdoc>
        /// <para>
        /// Initializes a new instance of the <see cref='System.ComponentModel.BindableAttribute'/> class.
        /// </para>
        /// </devdoc>
        public BindableAttribute(BindableSupport flags, BindingDirection direction) {
            this.bindable = (flags != BindableSupport.No);
            this.isDefault = (flags == BindableSupport.Default);
            this.direction = direction;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating
        ///       whether a property is appropriate to bind data to.
        ///    </para>
        /// </devdoc>
        public bool Bindable {
            get {
                return bindable;
            }
        }

        /// <devdoc>
        /// <para>
        /// Gets a value indicating
        /// the direction(s) this property be bound to data.
        /// </para>
        /// </devdoc>
        public BindingDirection Direction {
            get {
                return direction;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            if (obj != null && obj is BindableAttribute) {
                return (((BindableAttribute)obj).Bindable == bindable);
            }

            return false;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return bindable.GetHashCode();
        }

#if !SILVERLIGHT
        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default) || isDefault);
        }
#endif
    }
}
