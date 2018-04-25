//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;

    [ConfigurationCollection(typeof(ProfileElement),
        AddItemName = TrackingConfigurationStrings.TrackingProfile,
        RemoveItemName = TrackingConfigurationStrings.Remove,
        ClearItemsName = TrackingConfigurationStrings.Clear)]
    [Fx.Tag.XamlVisible(false)]
    public sealed class ProfileElementCollection : TrackingConfigurationCollection<ProfileElement>
    {
        internal ProfileElementCollection()
            : base()
        {
            this.AddElementName = TrackingConfigurationStrings.TrackingProfile;
            this.RemoveElementName = TrackingConfigurationStrings.Remove;
            this.ClearElementName = TrackingConfigurationStrings.Clear;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }
    }
}
