//------------------------------------------------------------------------------
// <copyright file="TemplateInstanceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    /// Whether a template is instantiated single or multiple times.
    /// The code generator generates fields corresponding to controls in the template
    /// for single instance templates.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TemplateInstanceAttribute : Attribute {


        public static readonly TemplateInstanceAttribute Multiple = new TemplateInstanceAttribute(TemplateInstance.Multiple);


        public static readonly TemplateInstanceAttribute Single = new TemplateInstanceAttribute(TemplateInstance.Single);


        public static readonly TemplateInstanceAttribute Default = Multiple;

        private TemplateInstance _instances;


        /// <devdoc>
        /// </devdoc>
        public TemplateInstanceAttribute(TemplateInstance instances) {
            _instances = instances;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        public TemplateInstance Instances {
            get {
                return _instances;
            }
        }


        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            
            TemplateInstanceAttribute other = obj as TemplateInstanceAttribute;
            if (other != null) {
                return (other.Instances == Instances);
            }
            return false;
        }


        /// <internalonly/>
        public override int GetHashCode() {
            return _instances.GetHashCode();
        }


        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}
