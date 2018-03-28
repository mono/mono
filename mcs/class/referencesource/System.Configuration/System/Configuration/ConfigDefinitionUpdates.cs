//------------------------------------------------------------------------------
// <copyright file="ConfigDefinitionUpdates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;

    //
    // Contains all the updates to section definitions across all location sections.
    //
    internal class ConfigDefinitionUpdates {
        private ArrayList   _locationUpdatesList;
        private bool        _requireLocationWritten;

        internal ConfigDefinitionUpdates() {
            _locationUpdatesList = new ArrayList();
        }

        //
        // Find the location update with a certain set of location attributes.
        //
        internal LocationUpdates FindLocationUpdates(OverrideModeSetting overrideMode, bool inheritInChildApps) {
            foreach (LocationUpdates locationUpdates in _locationUpdatesList) {
                if (    OverrideModeSetting.CanUseSameLocationTag(locationUpdates.OverrideMode, overrideMode) && 
                        locationUpdates.InheritInChildApps == inheritInChildApps) {
                    return locationUpdates;
                }
            }

            return null;
        }

        //
        // Add a section definition update to the correct location update.
        //
        internal DefinitionUpdate AddUpdate(OverrideModeSetting overrideMode, bool inheritInChildApps, bool moved, string updatedXml, SectionRecord sectionRecord) {
            LocationUpdates locationUpdates = FindLocationUpdates(overrideMode, inheritInChildApps);
            if (locationUpdates == null) {
                locationUpdates = new LocationUpdates(overrideMode, inheritInChildApps);
                _locationUpdatesList.Add(locationUpdates);
            }

            DefinitionUpdate definitionUpdate = new DefinitionUpdate(sectionRecord.ConfigKey, moved, updatedXml, sectionRecord);
            locationUpdates.SectionUpdates.AddSection(definitionUpdate);
            return definitionUpdate;
        }

        //
        // Determine which section definition updates are new.
        //
        internal void CompleteUpdates() {
            foreach (LocationUpdates locationUpdates in _locationUpdatesList) {
                locationUpdates.CompleteUpdates();
            }
        }

        internal ArrayList LocationUpdatesList {
            get {return _locationUpdatesList;}
        }

        internal bool RequireLocation {
            get { return _requireLocationWritten; }
            set { _requireLocationWritten = value; }
        }

        internal void FlagLocationWritten() {
            _requireLocationWritten = false;
        }
    }
}
