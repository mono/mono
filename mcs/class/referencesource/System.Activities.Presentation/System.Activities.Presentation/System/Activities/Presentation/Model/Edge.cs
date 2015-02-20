//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    internal class Edge : LinkBase
    {
        public Edge(ModelItem sourceVertex, ModelItem destinationVertex)
            : base(sourceVertex, destinationVertex)
        {
        }

        public Edge(string propertyName, ModelItem sourceVertex, ModelItem destinationVertex)
            : base(propertyName, sourceVertex, destinationVertex)
        {
        }
    }
}
