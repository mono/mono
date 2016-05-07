//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Runtime;

    internal class LinkBase
    {
        public LinkBase(ModelItem sourceVertex, ModelItem destinationVertex)
            : this(LinkType.Item, null, sourceVertex, destinationVertex)
        {
        }

        public LinkBase(string propertyName, ModelItem sourceVertex, ModelItem destinationVertex)
            : this(LinkType.Property, propertyName, sourceVertex, destinationVertex)
        {
        }

        private LinkBase(LinkType linkType, string propertyName, ModelItem sourceVertex, ModelItem destinationVertex)
        {
            Fx.Assert(linkType != LinkType.Item || propertyName == null, "propertyName should be null when linkType is LinkType.Item");
            Fx.Assert(sourceVertex != null, "sourceVertex should not be null");
            Fx.Assert(destinationVertex != null, "destinationVertex should not be null");

            this.LinkType = linkType;
            this.PropertyName = propertyName;
            this.SourceVertex = sourceVertex;
            this.DestinationVertex = destinationVertex;
        }

        public LinkType LinkType { get; private set; }

        public string PropertyName { get; private set; }

        public ModelItem DestinationVertex { get; private set; }

        public ModelItem SourceVertex { get; private set; }
    }
}
