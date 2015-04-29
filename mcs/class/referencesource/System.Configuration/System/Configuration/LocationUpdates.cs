//------------------------------------------------------------------------------
// <copyright file="LocationUpdates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    // 
    // LocationUpdates contains all the updates that share the same location characteristics.
    //
    internal class LocationUpdates {
        OverrideModeSetting _overrideMode;
        bool                _inheritInChildApps;
        SectionUpdates      _sectionUpdates;        // root of section

        internal LocationUpdates(OverrideModeSetting overrideMode, bool inheritInChildApps) {
            _overrideMode           = overrideMode;
            _inheritInChildApps     = inheritInChildApps;
            _sectionUpdates         = new SectionUpdates(string.Empty);
        }

        internal OverrideModeSetting OverrideMode {
            get {return _overrideMode;}
        }

        internal bool InheritInChildApps {
            get {return _inheritInChildApps;}
        }

        internal SectionUpdates SectionUpdates {
            get {return _sectionUpdates;}
        }

        internal bool IsDefault {
            get {
                return _overrideMode.IsDefaultForLocationTag && _inheritInChildApps == true;
            }
        }

        internal void CompleteUpdates() {
            _sectionUpdates.CompleteUpdates();
        }
    }
}
