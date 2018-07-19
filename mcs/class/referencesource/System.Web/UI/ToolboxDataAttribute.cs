//------------------------------------------------------------------------------
// <copyright file="ToolboxDataAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.Util;
    

    /// <devdoc>
    ///     ToolboxDataAttribute 
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ToolboxDataAttribute : Attribute {


        /// <devdoc>
        ///     
        /// </devdoc>
        public static readonly ToolboxDataAttribute Default = new ToolboxDataAttribute(String.Empty);

        private string data = String.Empty;


        /// <devdoc>
        /// </devdoc>
        public string Data {
            get {
                return this.data;
            }
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public ToolboxDataAttribute(string data) {
            this.data = data;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return ((Data != null) ? Data.GetHashCode() : 0);
        }


        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if ((obj != null) && (obj is ToolboxDataAttribute)) {
                return(StringUtil.EqualsIgnoreCase(((ToolboxDataAttribute)obj).Data, data));
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
