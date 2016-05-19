//------------------------------------------------------------------------------
// <copyright file="ConstructorNeedsTagAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para> Allows a control to specify that it needs a
    ///       tag name in its constructor.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConstructorNeedsTagAttribute: Attribute {
        bool needsTag = false;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.ConstructorNeedsTagAttribute'/> class.</para>
        /// </devdoc>
        public ConstructorNeedsTagAttribute() {
        }


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.ConstructorNeedsTagAttribute'/> class.</para>
        /// </devdoc>
        public ConstructorNeedsTagAttribute(bool needsTag) {
            this.needsTag = needsTag;
        }


        /// <devdoc>
        ///    <para>Indicates whether a control needs a tag in its contstructor. This property is read-only.</para>
        /// </devdoc>
        public bool NeedsTag {
            get {
                return needsTag;
            }
        }
    }
}
