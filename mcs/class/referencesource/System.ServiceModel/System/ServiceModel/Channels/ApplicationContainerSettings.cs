// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.Runtime;

    public sealed class ApplicationContainerSettings
    {
        public const int CurrentSession = ApplicationContainerSettingsDefaults.CurrentSession;
        public const int ServiceSession = ApplicationContainerSettingsDefaults.ServiceSession;
        const string GroupNameSuffixFormat = ";SessionId={0};PackageFullName={1}";
                
        int sessionId;

        internal ApplicationContainerSettings()
        {
            this.PackageFullName = ApplicationContainerSettingsDefaults.PackageFullNameDefaultString;
            this.sessionId = ApplicationContainerSettingsDefaults.CurrentSession;
        }

        ApplicationContainerSettings(ApplicationContainerSettings source)
        {
            this.PackageFullName = source.PackageFullName;
            this.sessionId = source.sessionId;
        }

        public string PackageFullName
        {
            get;
            set;
        }

        public int SessionId
        {
            get
            {
                return this.sessionId;
            }

            set
            {
                // CurrentSession default is -1 and expect the user to set 
                // non-negative windows session Id.
                if (value < ApplicationContainerSettingsDefaults.CurrentSession)
                {
                    throw FxTrace.Exception.Argument("value", SR.GetString(SR.SessionValueInvalid, value));
                }

                this.sessionId = value;
            }
        }

        internal bool TargetingAppContainer
        {
            get
            {
                return !string.IsNullOrEmpty(this.PackageFullName);
            }
        }

        internal ApplicationContainerSettings Clone()
        {
            return new ApplicationContainerSettings(this);
        }

        internal string GetConnectionGroupSuffix()
        {
            string suffix = string.Empty;
            if (AppContainerInfo.IsAppContainerSupported && this.TargetingAppContainer)
            {
                suffix = string.Format(CultureInfo.InvariantCulture, GroupNameSuffixFormat, this.SessionId, this.PackageFullName);
            }

            return suffix;
        }

        internal bool IsMatch(ApplicationContainerSettings applicationContainerSettings)
        {
            if (applicationContainerSettings == null)
            {
                return false;
            }

            if (this.PackageFullName != applicationContainerSettings.PackageFullName)
            {
                return false;
            }

            if (this.sessionId != applicationContainerSettings.sessionId)
            {
                return false;
            }

            return true;
        }
    }
}
