//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    internal class BackPointer : LinkBase
    {
        public BackPointer(ModelItem sourceVertex, ModelItem destinationVertex)
            : base(sourceVertex, destinationVertex)
        {
        }

        public BackPointer(string propertyName, ModelItem sourceVertex, ModelItem destinationVertex)
            : base(propertyName, sourceVertex, destinationVertex)
        {
        }
    }
}
