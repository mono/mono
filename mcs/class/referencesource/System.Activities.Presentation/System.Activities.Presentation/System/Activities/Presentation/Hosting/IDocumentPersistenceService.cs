//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Activities.Presentation.Model;


    public interface IDocumentPersistenceService
    {
        object Load(string fileName);
        void Flush(object documentRoot);
        void OnModelChanged(object documentRoot);
    }
}
