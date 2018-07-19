//------------------------------------------------------------------------------
// <copyright file="IDeviceSpecificDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Windows.Forms;

using System.Web.UI.MobileControls;

namespace System.Web.UI.Design.MobileControls
{
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal interface IDeviceSpecificDesigner
    {
        System.Web.UI.Control UnderlyingControl
        {
            get;
        }

        Object UnderlyingObject
        {
            get;
        }

        System.Windows.Forms.Control Header
        {
            get;
        }

        String CurrentDeviceSpecificID
        {
            get;
        }

        bool GetDeviceSpecific(String deviceSpecificID, out DeviceSpecific ds);
        void SetDeviceSpecificEditor(IRefreshableDeviceSpecificEditor editor);
        void SetDeviceSpecific(String deviceSpecificID, DeviceSpecific deviceSpecific);
        void InitHeader(int mergingContext);
        void RefreshHeader(int mergingContext);
        void UseCurrentDeviceSpecificID();
    }
}
