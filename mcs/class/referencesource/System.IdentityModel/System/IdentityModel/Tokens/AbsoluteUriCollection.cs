//-----------------------------------------------------------------------
// <copyright file="AbsoluteUriCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// A collection of absolute URIs.
    /// </summary>
    internal class AbsoluteUriCollection : Collection<Uri>
    {
        public AbsoluteUriCollection()
        {
        }

        protected override void InsertItem(int index, Uri item)
        {
            if (null == item || !item.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", SR.GetString(SR.ID0013));
            }
            
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Uri item)
        {
            if (null == item || !item.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("item", SR.GetString(SR.ID0013));
            }

            base.SetItem(index, item);
        }
    }
}
