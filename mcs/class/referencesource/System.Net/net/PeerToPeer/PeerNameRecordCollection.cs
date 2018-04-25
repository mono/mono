//------------------------------------------------------------------------------
// <copyright file="PeerNameRecordCollection.cs" company="Microsoft">
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
    public class PeerNameRecordCollection : Collection<PeerNameRecord>
    {
        public PeerNameRecordCollection() { }
        protected override void SetItem(int index, PeerNameRecord item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
        protected override void InsertItem(int index, PeerNameRecord item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }
    }
}
