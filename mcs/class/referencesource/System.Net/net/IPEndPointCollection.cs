//------------------------------------------------------------------------------
// <copyright file="IPEndPointCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;

    [Serializable]
    public class IPEndPointCollection : Collection<IPEndPoint>
    {
        public IPEndPointCollection() { }
        protected override void SetItem(int index, IPEndPoint item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
        protected override void InsertItem(int index, IPEndPoint item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }
    }
}
