// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections;
    using Microsoft.Build.Framework;

    // In Dev10, we allowed use of non-remotable ITaskItems. While there's not really any good scenario
    // for those, we need to support that for back-compat. So we wrap each provided TaskItems in an MBRO.
    internal class DelegatingTaskItem : MarshalByRefObject, ITaskItem
    {
        private ITaskItem underlyingItem;

        // This is different from TaskItem.ctor(ITaskItem): it's a wrapper, not a clone.
        // So any changes are propagated back to the original object.
        public DelegatingTaskItem(ITaskItem underlyingItem)
        {
            this.underlyingItem = underlyingItem;
        }

        public string ItemSpec
        {
            get
            {
                return this.underlyingItem.ItemSpec;
            }

            set
            {
                this.underlyingItem.ItemSpec = value;
            }
        }

        public int MetadataCount
        {
            get { return this.underlyingItem.MetadataCount; }
        }

        public ICollection MetadataNames
        {
            get { return this.underlyingItem.MetadataNames; }
        }

        public IDictionary CloneCustomMetadata()
        {
            return this.underlyingItem.CloneCustomMetadata();
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            this.underlyingItem.CopyMetadataTo(destinationItem);
        }

        public string GetMetadata(string metadataName)
        {
            return this.underlyingItem.GetMetadata(metadataName);
        }

        public void RemoveMetadata(string metadataName)
        {
            this.underlyingItem.RemoveMetadata(metadataName);
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            this.underlyingItem.SetMetadata(metadataName, metadataValue);
        }
    }
}
