//------------------------------------------------------------------------------
// <copyright file="WebSysDisplayNameAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web {


    using System;
    using System.ComponentModel;   


    /// <devdoc>
    ///     DisplayNameAttribute marks a property, event, or extender with a
    ///     DisplayName. Visual designers can display this DisplayName when referencing
    ///     the member.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Class)]
    internal sealed class WebSysDisplayNameAttribute : DisplayNameAttribute {

        private bool replaced;


        /// <devdoc>
        ///    <para>Constructs a new sys DisplayName.</para>
        /// </devdoc>
        internal WebSysDisplayNameAttribute(string DisplayName) : base(DisplayName) {
        }


        /// <devdoc>
        ///    <para>Retrieves the DisplayName text.</para>
        /// </devdoc>
        public override string DisplayName {
            get {
                if (!replaced) {
                    replaced = true;
                    DisplayNameValue = SR.GetString(base.DisplayName);                
                }
                return base.DisplayName;
            }
        }

        public override object TypeId {
            get {
                return typeof(DisplayNameAttribute);
            }
        }
    }
}
