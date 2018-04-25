// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    public sealed class NamedPipeSettings
    {
        internal NamedPipeSettings()
        {
            this.ApplicationContainerSettings = new ApplicationContainerSettings();
        }

        NamedPipeSettings(NamedPipeSettings elementToBeCloned)
        {
            if (elementToBeCloned.ApplicationContainerSettings != null)
            {
                this.ApplicationContainerSettings = elementToBeCloned.ApplicationContainerSettings.Clone();
            }
        }

        public ApplicationContainerSettings ApplicationContainerSettings
        {
            get;
            private set;
        }

        internal NamedPipeSettings Clone()
        {
            return new NamedPipeSettings(this);
        }

        internal bool IsMatch(NamedPipeSettings pipeSettings)
        {
            if (pipeSettings == null)
            {
                return false;
            }

            if (!this.ApplicationContainerSettings.IsMatch(pipeSettings.ApplicationContainerSettings))
            {
                return false;
            }

            return true;
        }
    }
}
