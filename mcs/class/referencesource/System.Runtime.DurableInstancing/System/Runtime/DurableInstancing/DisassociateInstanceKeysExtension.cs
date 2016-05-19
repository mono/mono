// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.DurableInstancing
{
    using System;

    internal class DisassociateInstanceKeysExtension
    {
        private bool automaticDisassociationEnabled;

        internal DisassociateInstanceKeysExtension()
        {
            this.automaticDisassociationEnabled = false;
        }

        internal bool AutomaticDisassociationEnabled
        {
            get
            {
                return this.automaticDisassociationEnabled;
            }

            set
            {
                this.automaticDisassociationEnabled = value;
            }
        }
    }
}
