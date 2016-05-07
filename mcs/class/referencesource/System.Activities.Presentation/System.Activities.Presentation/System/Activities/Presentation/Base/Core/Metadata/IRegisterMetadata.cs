//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Metadata 
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    // <summary>
    // Specifies a class that wishes to add extra attributes
    // to the metadata store.
    // </summary>
    public interface IRegisterMetadata 
    {
        // <summary>
        // Classes implementing register should use the
        // MetadataStore.AddAttributeTable to add additional
        // metadata to the store.
        //
        // Register will be called upon the
        // initialization of the designer.
        // </summary>
        void Register();

    }
}
