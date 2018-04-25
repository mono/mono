//------------------------------------------------------------------------------
// <copyright file="IListControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.MobileControls
{
    /*
     * IListControl interface.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal interface IListControl
    {
        void OnItemDataBind(ListDataBindEventArgs e);
        bool TrackingViewState
        {
            get;
        }
    }
}

