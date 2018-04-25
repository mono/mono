//------------------------------------------------------------------------------
// <copyright file="DeviceSpecificDialogCachedState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;

    using System.Web.UI.MobileControls;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DeviceSpecificDialogCachedState : ICloneable
    {
        protected void SaveChoices(
            IDeviceSpecificDesigner designer,
            String deviceSpecificID,
            ICollection choices
        ) {
            DeviceSpecific deviceSpecific;
            if (!designer.GetDeviceSpecific(deviceSpecificID, out deviceSpecific))
            {
                return;
            }

            if(choices.Count == 0)
            {
                designer.SetDeviceSpecific(deviceSpecificID, null);
                return;
            }
            
            if (deviceSpecific == null)
            {
                deviceSpecific = new DeviceSpecific();
            }
            else
            {
                deviceSpecific.Choices.Clear();
            }
            foreach (ChoiceTreeNode node in choices)
            {
                node.CommitChanges();
                DeviceSpecificChoice choice = node.Choice.RuntimeChoice;
                deviceSpecific.Choices.Add(choice);
            }
            designer.SetDeviceSpecific(deviceSpecificID, deviceSpecific);
        }

        /// <summary>
        ///     Perform shallow copy of state.  Cached contents will still
        ///     point to the same instances.
        /// </summary>
        public Object Clone()
        {
            return MemberwiseClone();
        }
    }
}
