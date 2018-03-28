//------------------------------------------------------------------------------
// <copyright file="MobileControlsSectionHelper.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Configuration;
using System.Diagnostics;
using System.Globalization;

namespace System.Web.UI.MobileControls {
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal static class MobileControlsSectionHelper {

        private static void AddControlAdapters(IndividualDeviceConfig deviceConfig, DeviceElement device) {
            foreach (ControlElement control in device.Controls) {
                deviceConfig.AddControl(control.Control, control.Adapter);
            }
        }

        // Essentially this method does what MobileControlSectionHandler.Create()
        // does, but use MobileControlsSection for retrieving config data instead
        internal static ControlsConfig CreateControlsConfig(MobileControlsSection controlSection) {
            ControlsConfig config = new ControlsConfig(null);

            config["sessionStateHistorySize"] = controlSection.SessionStateHistorySize.ToString(CultureInfo.InvariantCulture);
            config["cookielessDataDictionaryType"] = controlSection.CookielessDataDictionaryType.AssemblyQualifiedName;
            config["allowCustomAttributes"] = controlSection.AllowCustomAttributes.ToString(CultureInfo.InvariantCulture);

            foreach (DeviceElement device in controlSection.Devices) {
                IndividualDeviceConfig deviceConfig = CreateDeviceConfig(config, device);
                AddControlAdapters(deviceConfig, device);

                if (!config.AddDeviceConfig(device.Name, deviceConfig)) {
                    // Problem is due to a duplicated name
                    throw new ConfigurationErrorsException(
                        SR.GetString(SR.MobileControlsSectionHandler_DuplicatedDeviceName, device.Name));
                }
            }

            // Passing null means no config file and line number info will be
            // shown when error happens.  That is because there is no XmlNode of
            // the config section is available when MobileControlsSection is
            // used.  But the error messages raised should still be good enough.
            config.FixupDeviceConfigInheritance(null);

            return config;
        }

        // Essentially this method does what MobileControlSectionHandler.CreateDeviceConfig()
        // does, but use MobileControlsSection for retrieving config data instead
        private static IndividualDeviceConfig CreateDeviceConfig(ControlsConfig config, DeviceElement device) {
            String nameOfDeviceToInheritFrom = device.InheritsFrom;
            if (nameOfDeviceToInheritFrom != null && nameOfDeviceToInheritFrom.Length == 0) {
                nameOfDeviceToInheritFrom = null;
            }

            IndividualDeviceConfig.DeviceQualifiesDelegate predicateDelegate = null;
            if (device.PredicateClass != null) {
                // If a predicate class is specified, so must a method.
                // The checking is already done in MobileControlsSection
                Debug.Assert(!String.IsNullOrEmpty(device.PredicateMethod));
                predicateDelegate = device.GetDelegate();
            }

            return new IndividualDeviceConfig(config,
                                              device.Name,
                                              predicateDelegate,
                                              device.PageAdapter,
                                              nameOfDeviceToInheritFrom);
        }
    }
}
