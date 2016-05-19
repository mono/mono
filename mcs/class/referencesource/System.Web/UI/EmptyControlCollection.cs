//------------------------------------------------------------------------------
// <copyright file="EmptyControlCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       Represents a ControlCollection that is always empty.
    ///    </para>
    /// </devdoc>
    public class EmptyControlCollection : ControlCollection {


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EmptyControlCollection(Control owner) : base(owner) {
        }

        private void ThrowNotSupportedException() {
            throw new HttpException(SR.GetString(SR.Control_does_not_allow_children,
                                                                     Owner.GetType().ToString()));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Add(Control child) {
            ThrowNotSupportedException();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void AddAt(int index, Control child) {
            ThrowNotSupportedException();
        }
    }
}
