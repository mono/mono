//------------------------------------------------------------------------------
// <copyright file="IRefreshableDeviceSpecificEditor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System.Web.UI.MobileControls;

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal interface IRefreshableDeviceSpecificEditor
    {
        bool RequestRefresh();
        void Refresh(String deviceSpecificID, DeviceSpecific deviceSpecific);
        void UnderlyingObjectsChanged();
        void BeginExternalDeviceSpecificEdit();
        void EndExternalDeviceSpecificEdit(bool commitChanges);
        void DeviceSpecificRenamed(String oldDeviceSpecificID, String newDeviceSpecificID);
        void DeviceSpecificDeleted(String DeviceSpecificID);
    }
}
