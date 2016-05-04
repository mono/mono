//------------------------------------------------------------------------------
// <copyright file="InheritanceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {

    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Marks instances of objects that are inherited from their base class. This
    ///       class cannot be inherited.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
    public sealed class InheritanceAttribute : Attribute {
    
        private readonly InheritanceLevel inheritanceLevel;

        /// <devdoc>
        ///    <para>
        ///       Specifies that the component is inherited. This field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly InheritanceAttribute Inherited = new InheritanceAttribute(InheritanceLevel.Inherited);

        /// <devdoc>
        ///    <para>
        ///       Specifies that
        ///       the component is inherited and is read-only. This field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly InheritanceAttribute InheritedReadOnly = new InheritanceAttribute(InheritanceLevel.InheritedReadOnly);

        /// <devdoc>
        ///    <para>
        ///       Specifies that the component is not inherited. This field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly InheritanceAttribute NotInherited = new InheritanceAttribute(InheritanceLevel.NotInherited);

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for
        ///       the InheritanceAttribute as NotInherited.
        ///    </para>
        /// </devdoc>
        public static readonly InheritanceAttribute Default = NotInherited;

        /// <devdoc>
        /// <para>Initializes a new instance of the System.ComponentModel.Design.InheritanceAttribute 
        /// class.</para>
        /// </devdoc>
        public InheritanceAttribute() {
            inheritanceLevel = Default.inheritanceLevel;
        }
        
        /// <devdoc>
        /// <para>Initializes a new instance of the System.ComponentModel.Design.InheritanceAttribute class 
        ///    with the specified inheritance
        ///    level.</para>
        /// </devdoc>
        public InheritanceAttribute(InheritanceLevel inheritanceLevel) {
            this.inheritanceLevel = inheritanceLevel;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the current inheritance level stored in this attribute.
        ///    </para>
        /// </devdoc>
        public InheritanceLevel InheritanceLevel {
            get {
                return inheritanceLevel;
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Override to test for equality.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object value) {
            if (value == this) {
                return true;
            }
            
            if (!(value is InheritanceAttribute)) {
                return false;
            }
            
            InheritanceLevel valueLevel = ((InheritanceAttribute)value).InheritanceLevel;
            return (valueLevel == inheritanceLevel);
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the hashcode for this object.
        ///    </para>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <devdoc>
        ///    <para>
        ///       Gets whether this attribute is the default.
        ///    </para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
        
        /// <devdoc>
        ///    <para>
        ///       Converts this attribute to a string.
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            return TypeDescriptor.GetConverter(typeof(InheritanceLevel)).ConvertToString(InheritanceLevel);
        }
    }
}

