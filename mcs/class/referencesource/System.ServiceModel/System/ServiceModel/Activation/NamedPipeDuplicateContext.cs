//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    [DataContract]
    class NamedPipeDuplicateContext : DuplicateContext
    {
        [DataMember]
        IntPtr handle;

        public NamedPipeDuplicateContext(IntPtr handle, Uri via, byte[] readData)
            : base(via, readData)
        {
            this.handle = handle;
        }

        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
        }
    }
}
