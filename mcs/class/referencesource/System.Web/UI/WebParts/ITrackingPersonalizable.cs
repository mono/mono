//------------------------------------------------------------------------------
// <copyright file="ITrackingPersonalizable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    /// <devdoc>
    /// Allows an object to track the personalization process,
    /// in terms of both loading and saving state, as well as
    /// optionally taking over tracking changes to personalized properties.
    /// </devdoc>
    public interface ITrackingPersonalizable {

        /// <devdoc>
        /// Indicates whether the object tracks its own changes. If so, it should return
        /// true. Otherwise it should return false, and the framework will perform a diff
        /// to determine if the object's personalization state has changed from its
        /// initial values.
        /// </devdoc>
        bool TracksChanges {
            get;
        }

        /// <devdoc>
        /// Called before personalization data is loaded into the object.
        /// An object that tracks changes should suspend its tracking during the load process.
        /// </devdoc>
        void BeginLoad();

        /// <devdoc>
        /// Called before personalization data is saved from the object.
        /// </devdoc>
        void BeginSave();

        /// <devdoc>
        /// Called after all the personalization data has been loaded into the object.
        /// </devdoc>
        void EndLoad();

        /// <devdoc>
        /// Called after all the personalization data has been saved from the object.
        /// </devdoc>
        void EndSave();
    }
}

