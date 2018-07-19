//------------------------------------------------------------------------------
// <copyright file="IDeviceSpecificChoiceDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal interface IDeviceSpecificChoiceDesigner
    {
        Object UnderlyingObject { get; }
        System.Web.UI.Control UnderlyingControl { get; }
    }
}
