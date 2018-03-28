//------------------------------------------------------------------------------
// <copyright file="IPersonalizable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;

    /// <devdoc>
    /// Allows an object to participate in personalization, beyond
    /// the default support for automatically storing properties
    /// marked as personalizable.
    /// </devdoc>
    public interface IPersonalizable {

        /// <devdoc>
        /// Allows the implementor to indicate whether its personalized state
        /// has been changed and must be persisted. This can be used to
        /// optimize storage of personalization state.
        /// If the implementation cannot determine the changed-state of its
        /// state, then it should return true.
        /// </devdoc>
        bool IsDirty { get; }

        /// <devdoc>
        /// Allows the object to load its custom personalization data.
        /// The dictionary contains name/value pairs that represent the
        /// personalization state.
        /// </devdoc>
        void Load(PersonalizationDictionary state);

        /// <devdoc>
        /// Allows the object to store its personalized state for the current
        /// personalization scope.
        /// Personalized state must be stored in the form of name/value
        /// pairs. Values stored must be serializable using ObjectStateFormatter.
        /// </devdoc>
        void Save(PersonalizationDictionary state);
    }
}
