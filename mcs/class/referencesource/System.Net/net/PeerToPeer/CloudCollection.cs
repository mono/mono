//------------------------------------------------------------------------------
// <copyright file="CloudCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    [Serializable]
    public class CloudCollection : Collection<Cloud>
    {
        public CloudCollection() { }
        protected override void SetItem(int index, Cloud item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
        protected override void InsertItem(int index, Cloud item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }
    }
}
