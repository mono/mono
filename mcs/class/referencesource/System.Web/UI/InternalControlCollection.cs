//------------------------------------------------------------------------------
// <copyright file="InternalControlCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {

    /// <devdoc>
    ///    <para>
    ///       Represents a ControlCollection that controls can only be added to internally.
    ///    </para>
    /// </devdoc>
    internal class InternalControlCollection : ControlCollection {

        internal InternalControlCollection(Control owner) : base(owner) {
        }

        private void ThrowNotSupportedException() {
            throw new HttpException(SR.GetString(SR.Control_does_not_allow_children,
                                                                     Owner.GetType().ToString()));
        }

        public override void Add(Control child) {
            ThrowNotSupportedException();
        }

        public override void AddAt(int index, Control child) {
            ThrowNotSupportedException();
        }
    }
}
